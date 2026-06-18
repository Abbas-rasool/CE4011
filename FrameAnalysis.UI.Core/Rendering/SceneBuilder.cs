using FrameAnalysis.UI.Core.Documents;
using FrameAnalysis.UI.Core.Documents.Rows;
using FrameAnalysisProgram.ANALYSIS_CORE;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Loads;

namespace FrameAnalysis.UI.Core.Rendering;

/// <summary>
/// Translates a <see cref="ProjectDocument"/> (and, optionally, an analysis result) into a
/// plain <see cref="Scene"/>. This is the boundary where document view-models and solver
/// results are flattened into presentation geometry — the renderer never sees either.
/// Incomplete rows (e.g. an element missing a node) are skipped rather than throwing, so
/// the picture stays live while the user is still entering data.
/// </summary>
public static class SceneBuilder
{
    public static Scene Build(
        ProjectDocument doc,
        FrameAnalysisResult? result = null,
        double deflectionScale = 1.0)
    {
        ArgumentNullException.ThrowIfNull(doc);

        var nodes = new List<SceneNode>(doc.Nodes.Count);
        for (int i = 0; i < doc.Nodes.Count; i++)
        {
            NodeRowVm n = doc.Nodes[i];
            nodes.Add(new SceneNode(i + 1, n.X, n.Y));
        }

        var members = new List<SceneMember>(doc.Elements.Count);
        for (int i = 0; i < doc.Elements.Count; i++)
        {
            ElementRowVm e = doc.Elements[i];
            if (e.StartNode is null || e.EndNode is null)
                continue;

            members.Add(new SceneMember(
                i + 1,
                e.StartNode.X, e.StartNode.Y,
                e.EndNode.X, e.EndNode.Y,
                e.Kind == ElementKind.Truss));
        }

        var supports = new List<SceneSupport>(doc.Supports.Count);
        foreach (SupportRowVm s in doc.Supports)
        {
            if (s.Node is null)
                continue;
            supports.Add(new SceneSupport(s.Node.X, s.Node.Y, s.RestrainX, s.RestrainY, s.RestrainRz));
        }

        var nodalLoads = new List<SceneNodalLoad>();
        foreach (NodalLoadRowVm l in doc.NodalLoads)
        {
            if (l.Node is null || (l.Fx == 0.0 && l.Fy == 0.0 && l.Mz == 0.0))
                continue;
            nodalLoads.Add(new SceneNodalLoad(l.Node.X, l.Node.Y, l.Fx, l.Fy, l.Mz));
        }

        var distributedLoads = new List<SceneDistributedLoad>();
        foreach (DistributedLoadRowVm d in doc.DistributedLoads)
        {
            if (d.Element?.StartNode is null || d.Element.EndNode is null)
                continue;
            (double dx, double dy) = UnitDirection(d.Direction);
            distributedLoads.Add(new SceneDistributedLoad(
                d.Element.StartNode.X, d.Element.StartNode.Y,
                d.Element.EndNode.X, d.Element.EndNode.Y,
                dx, dy, d.MagnitudePerLength));
        }

        var pointLoads = new List<ScenePointLoad>();
        foreach (PointLoadRowVm p in doc.PointLoads)
        {
            if (p.Element?.StartNode is null || p.Element.EndNode is null)
                continue;
            (double px, double py) = PointOnMember(p.Element.StartNode, p.Element.EndNode, p.DistanceFromStart);
            (double dx, double dy) = UnitDirection(p.Direction);
            pointLoads.Add(new ScenePointLoad(px, py, dx, dy, p.Magnitude));
        }

        IReadOnlyList<SceneDeflectedMember> deflected = Array.Empty<SceneDeflectedMember>();
        IReadOnlyList<SceneReaction> reactions = Array.Empty<SceneReaction>();
        if (result is not null)
        {
            deflected = BuildDeflectedMembers(doc, result, deflectionScale);
            reactions = BuildReactions(doc, result);
        }

        return new Scene
        {
            Nodes = nodes,
            Members = members,
            Supports = supports,
            NodalLoads = nodalLoads,
            DistributedLoads = distributedLoads,
            PointLoads = pointLoads,
            DeflectedMembers = deflected,
            Reactions = reactions,
            Bounds = ComputeBounds(nodes),
        };
    }

    private static List<SceneDeflectedMember> BuildDeflectedMembers(
        ProjectDocument doc,
        FrameAnalysisResult result,
        double scale)
    {
        // Station results are keyed by the solver element id, which the mapper assigns as
        // (grid position + 1) — the same id this builder uses for SceneMember.
        var byId = new Dictionary<int, MemberStationResult>(result.MemberStations.Count);
        foreach (MemberStationResult ms in result.MemberStations)
            byId[ms.ElementId] = ms;

        var list = new List<SceneDeflectedMember>(doc.Elements.Count);
        for (int i = 0; i < doc.Elements.Count; i++)
        {
            ElementRowVm e = doc.Elements[i];
            if (e.StartNode is null || e.EndNode is null)
                continue;
            if (!byId.TryGetValue(i + 1, out MemberStationResult? stations))
                continue;

            double sx = e.StartNode.X, sy = e.StartNode.Y;
            double dx = e.EndNode.X - sx, dy = e.EndNode.Y - sy;
            double length = Math.Sqrt(dx * dx + dy * dy);
            if (length <= 0.0)
                continue;

            double c = dx / length, s = dy / length; // member axis direction

            var points = new ScenePoint[stations.X.Count];
            for (int k = 0; k < stations.X.Count; k++)
            {
                double x = stations.X[k];                       // distance along member
                double u = stations.DeflectionAxial[k];         // local, unscaled
                double v = stations.DeflectionTransverse[k];

                // Base point on the straight member axis, plus the local (u, v) offset
                // rotated back to world and amplified by the deflection scale.
                double baseX = sx + c * x, baseY = sy + s * x;
                double offX = c * u - s * v, offY = s * u + c * v;
                points[k] = new ScenePoint(baseX + offX * scale, baseY + offY * scale);
            }

            list.Add(new SceneDeflectedMember(i + 1, points));
        }

        return list;
    }

    private static List<SceneReaction> BuildReactions(ProjectDocument doc, FrameAnalysisResult result)
    {
        var list = new List<SceneReaction>(result.Reactions.Count);
        foreach (NodalReaction r in result.Reactions)
        {
            int index = r.NodeId - 1;
            if (index < 0 || index >= doc.Nodes.Count)
                continue;
            NodeRowVm node = doc.Nodes[index];
            list.Add(new SceneReaction(node.X, node.Y, r.Fx, r.Fy, r.Mz));
        }

        return list;
    }

    private static (double X, double Y) UnitDirection(LoadDirection direction) => direction switch
    {
        LoadDirection.X => (1.0, 0.0),
        LoadDirection.Y => (0.0, 1.0),
        _ => (0.0, 0.0), // Z has no in-plane glyph
    };

    private static (double X, double Y) PointOnMember(NodeRowVm start, NodeRowVm end, double distanceFromStart)
    {
        double dx = end.X - start.X;
        double dy = end.Y - start.Y;
        double length = Math.Sqrt(dx * dx + dy * dy);
        double t = length > 0.0 ? Math.Clamp(distanceFromStart / length, 0.0, 1.0) : 0.0;
        return (start.X + dx * t, start.Y + dy * t);
    }

    private static SceneBounds ComputeBounds(IReadOnlyList<SceneNode> nodes)
    {
        if (nodes.Count == 0)
            return SceneBounds.Empty;

        double minX = double.MaxValue, minY = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue;
        foreach (SceneNode n in nodes)
        {
            if (n.X < minX) minX = n.X;
            if (n.Y < minY) minY = n.Y;
            if (n.X > maxX) maxX = n.X;
            if (n.Y > maxY) maxY = n.Y;
        }

        return new SceneBounds(minX, minY, maxX, maxY, false);
    }
}
