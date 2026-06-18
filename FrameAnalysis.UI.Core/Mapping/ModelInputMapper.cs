using FrameAnalysis.UI.Core.Documents;
using FrameAnalysis.UI.Core.Documents.Rows;
using FrameAnalysis.UI.Core.Units;
using FrameAnalysisProgram.INPUT_OUTPUT;
using StructuralLoads;

namespace FrameAnalysis.UI.Core.Mapping;

/// <summary>
/// Converts the editable <see cref="ProjectDocument"/> into the solver's tabular
/// <see cref="StructureInputData"/> DTO — the one place where the UI's object-reference
/// model is flattened into the 1-based positional ids the solver expects.
///
/// Each row's id is its (index + 1) in grid order, computed here from collection
/// position (independent of the rows' own <c>Id</c> fields), so reorder / insert /
/// delete in the grids is always safe. Optional load tables are emitted only when their
/// collection is non-empty. Dangling or missing references throw a clear
/// <see cref="InvalidOperationException"/> naming the offending row.
/// </summary>
public static class ModelInputMapper
{
    public static StructureInputData ToInputData(ProjectDocument doc) => ToInputData(doc, LoadScope.All);

    /// <summary>
    /// Maps the document to the solver DTO, including only the loads selected by
    /// <paramref name="scope"/>. The default <see cref="LoadScope.All"/> is the whole model;
    /// narrower scopes build the isolated per-load-case inputs the superposition basis needs.
    /// </summary>
    public static StructureInputData ToInputData(ProjectDocument doc, LoadScope scope)
    {
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(scope);

        // Reference-identity position lookups (1-based). The row VMs don't override
        // Equals/GetHashCode, so the default comparer is reference equality.
        Dictionary<NodeRowVm, int> nodeIds = BuildIndexLookup(doc.Nodes);
        Dictionary<MaterialRowVm, int> materialIds = BuildIndexLookup(doc.Materials);
        Dictionary<SectionRowVm, int> sectionIds = BuildIndexLookup(doc.Sections);
        Dictionary<ElementRowVm, int> elementIds = BuildIndexLookup(doc.Elements);

        return new StructureInputData
        {
            NodeTable = BuildNodeTable(doc),
            MaterialTable = BuildMaterialTable(doc),
            SectionTable = BuildSectionTable(doc),
            ElementTable = BuildElementTable(doc, nodeIds, materialIds, sectionIds),
            SupportTable = BuildSupportTable(doc, nodeIds),
            LoadTable = BuildLoadTable(doc, nodeIds, scope),
            DistributedLoadTable = BuildDistributedLoadTable(doc, elementIds, scope),
            PointLoadTable = BuildPointLoadTable(doc, elementIds, scope),
            TemperatureLoadTable = BuildTemperatureLoadTable(doc, elementIds, scope),
            SettlementTable = BuildSettlementTable(doc, nodeIds, scope),
        };
    }

    private static Dictionary<T, int> BuildIndexLookup<T>(IList<T> rows) where T : notnull
    {
        var map = new Dictionary<T, int>(rows.Count);
        for (int i = 0; i < rows.Count; i++)
            map[rows[i]] = i + 1;
        return map;
    }

    // --- Geometry / properties ---

    private static double[,] BuildNodeTable(ProjectDocument doc)
    {
        // NodeTable columns: [X, Y]. (Z is reserved for 3D and not exported yet.)
        var table = new double[doc.Nodes.Count, 2];
        for (int i = 0; i < doc.Nodes.Count; i++)
        {
            NodeRowVm n = doc.Nodes[i];
            table[i, 0] = n.X;
            table[i, 1] = n.Y;
        }
        return table;
    }

    private static double[,] BuildMaterialTable(ProjectDocument doc)
    {
        // MaterialTable columns: [ElasticModulus]. E is the only material input the analysis
        // needs, so it is mandatory here (design-only strength values are validated separately).
        // E is entered in MPa and converted to the canonical kN/m² the solver works in.
        var table = new double[doc.Materials.Count, 1];
        for (int i = 0; i < doc.Materials.Count; i++)
        {
            double e = doc.Materials[i].ElasticModulus;
            if (e <= 0)
                throw new InvalidOperationException(
                    $"Material {i + 1} has no elastic modulus (E) — required for analysis.");
            table[i, 0] = e * UnitSystem.MPaToKNm2;
        }
        return table;
    }

    private static double[,] BuildSectionTable(ProjectDocument doc)
    {
        // SectionTable columns: [Width, Length(=Depth), MomentOfInertia]. Width/Depth/I are
        // stored in the user's chosen section unit; convert to the canonical metre system.
        var table = new double[doc.Sections.Count, 3];
        for (int i = 0; i < doc.Sections.Count; i++)
        {
            SectionRowVm s = doc.Sections[i];
            table[i, 0] = s.Width * doc.Units.SectionToM;
            table[i, 1] = s.Depth * doc.Units.SectionToM;
            table[i, 2] = s.MomentOfInertia * doc.Units.InertiaToM4;
        }
        return table;
    }

    private static int[,] BuildElementTable(
        ProjectDocument doc,
        Dictionary<NodeRowVm, int> nodeIds,
        Dictionary<MaterialRowVm, int> materialIds,
        Dictionary<SectionRowVm, int> sectionIds)
    {
        // ElementTable columns:
        // [StartNodeId, EndNodeId, MaterialId, SectionId, ElementType, MomentRelease]
        var table = new int[doc.Elements.Count, 6];
        for (int i = 0; i < doc.Elements.Count; i++)
        {
            ElementRowVm e = doc.Elements[i];
            int number = i + 1;
            table[i, 0] = ResolveId(nodeIds, e.StartNode, "start node", "Element", number);
            table[i, 1] = ResolveId(nodeIds, e.EndNode, "end node", "Element", number);
            table[i, 2] = ResolveId(materialIds, e.Material, "material", "Element", number);
            table[i, 3] = ResolveId(sectionIds, e.Section, "section", "Element", number);
            table[i, 4] = (int)e.Kind;     // 0 = Frame, 1 = Truss
            table[i, 5] = (int)e.Release;  // 0 = None .. 3 = Both (ignored for truss)
        }
        return table;
    }

    private static int[,] BuildSupportTable(ProjectDocument doc, Dictionary<NodeRowVm, int> nodeIds)
    {
        // SupportTable columns: [NodeId, Rx, Ry, Rz] with 1 = restrained.
        var table = new int[doc.Supports.Count, 4];
        for (int i = 0; i < doc.Supports.Count; i++)
        {
            SupportRowVm s = doc.Supports[i];
            table[i, 0] = ResolveId(nodeIds, s.Node, "node", "Support", i + 1);
            table[i, 1] = s.RestrainX ? 1 : 0;
            table[i, 2] = s.RestrainY ? 1 : 0;
            table[i, 3] = s.RestrainRz ? 1 : 0;
        }
        return table;
    }

    // --- Loads ---

    private static double[,] BuildLoadTable(ProjectDocument doc, Dictionary<NodeRowVm, int> nodeIds, LoadScope scope)
    {
        // LoadTable columns: [NodeId, Fx, Fy, Mz, Nature]
        var rows = new List<NodalLoadRowVm>();
        foreach (NodalLoadRowVm l in doc.NodalLoads)
            if (scope.IncludesAction(l.Nature)) rows.Add(l);

        var table = new double[rows.Count, 5];
        for (int i = 0; i < rows.Count; i++)
        {
            NodalLoadRowVm l = rows[i];
            table[i, 0] = ResolveId(nodeIds, l.Node, "node", "Joint load", i + 1);
            table[i, 1] = l.Fx;
            table[i, 2] = l.Fy;
            table[i, 3] = l.Mz;
            table[i, 4] = (int)l.Nature;
        }
        return table;
    }

    private static double[,]? BuildDistributedLoadTable(ProjectDocument doc, Dictionary<ElementRowVm, int> elementIds, LoadScope scope)
    {
        var rows = new List<DistributedLoadRowVm>();
        foreach (DistributedLoadRowVm d in doc.DistributedLoads)
            if (scope.IncludesAction(d.Nature)) rows.Add(d);
        if (rows.Count == 0) return null;

        // DistributedLoadTable columns: [ElementId, MagnitudePerLength, Direction, Nature]
        var table = new double[rows.Count, 4];
        for (int i = 0; i < rows.Count; i++)
        {
            DistributedLoadRowVm d = rows[i];
            table[i, 0] = ResolveId(elementIds, d.Element, "element", "Distributed load", i + 1);
            table[i, 1] = d.MagnitudePerLength;
            table[i, 2] = (int)d.Direction;
            table[i, 3] = (int)d.Nature;
        }
        return table;
    }

    private static double[,]? BuildPointLoadTable(ProjectDocument doc, Dictionary<ElementRowVm, int> elementIds, LoadScope scope)
    {
        var rows = new List<PointLoadRowVm>();
        foreach (PointLoadRowVm p in doc.PointLoads)
            if (scope.IncludesAction(p.Nature)) rows.Add(p);
        if (rows.Count == 0) return null;

        // PointLoadTable columns: [ElementId, DistanceFromStart, Magnitude, Direction, Nature]
        var table = new double[rows.Count, 5];
        for (int i = 0; i < rows.Count; i++)
        {
            PointLoadRowVm p = rows[i];
            table[i, 0] = ResolveId(elementIds, p.Element, "element", "Point load", i + 1);
            table[i, 1] = p.DistanceFromStart;
            table[i, 2] = p.Magnitude;
            table[i, 3] = (int)p.Direction;
            table[i, 4] = (int)p.Nature;
        }
        return table;
    }

    private static double[,]? BuildTemperatureLoadTable(ProjectDocument doc, Dictionary<ElementRowVm, int> elementIds, LoadScope scope)
    {
        // Temperature actions are of nature Thermal.
        if (!scope.IncludesAction(eLoadNature.Thermal) || doc.TemperatureLoads.Count == 0)
            return null;

        // TemperatureLoadTable columns:
        // [ElementId, UniformTemperatureChange, TemperatureGradient, ThermalExpansionCoefficient, MemberDepth]
        var table = new double[doc.TemperatureLoads.Count, 5];
        for (int i = 0; i < doc.TemperatureLoads.Count; i++)
        {
            TemperatureLoadRowVm t = doc.TemperatureLoads[i];
            table[i, 0] = ResolveId(elementIds, t.Element, "element", "Temperature load", i + 1);
            table[i, 1] = t.UniformTemperatureChange;
            table[i, 2] = t.TemperatureGradient;
            table[i, 3] = t.ThermalExpansionCoefficient;
            table[i, 4] = t.MemberDepth;
        }
        return table;
    }

    private static double[,]? BuildSettlementTable(ProjectDocument doc, Dictionary<NodeRowVm, int> nodeIds, LoadScope scope)
    {
        if (!scope.IncludeSettlements || doc.Settlements.Count == 0)
            return null;

        // SettlementTable columns: [NodeId, dUx, dUy, dRz]. dUx/dUy are stored in the user's
        // settlement length unit (converted to m); dRz is a rotation in radians.
        var table = new double[doc.Settlements.Count, 4];
        for (int i = 0; i < doc.Settlements.Count; i++)
        {
            SettlementRowVm s = doc.Settlements[i];
            table[i, 0] = ResolveId(nodeIds, s.Node, "node", "Settlement", i + 1);
            table[i, 1] = s.DeltaUx * doc.Units.SettlementToM;
            table[i, 2] = s.DeltaUy * doc.Units.SettlementToM;
            table[i, 3] = s.DeltaRz;
        }
        return table;
    }

    private static int ResolveId<T>(Dictionary<T, int> lookup, T? row, string role, string ownerKind, int ownerNumber)
        where T : class
    {
        if (row is null)
            throw new InvalidOperationException($"{ownerKind} {ownerNumber} has no {role} assigned.");

        if (!lookup.TryGetValue(row, out int id))
            throw new InvalidOperationException(
                $"{ownerKind} {ownerNumber} references a {role} that is not part of the model.");

        return id;
    }
}
