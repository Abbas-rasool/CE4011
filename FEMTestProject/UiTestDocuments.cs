using FrameAnalysis.UI.Core.Documents;
using FrameAnalysis.UI.Core.Documents.Rows;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Loads;

namespace FEMTestProject
{
    /// <summary>Shared ProjectDocument fixtures for the UI.Core tests.</summary>
    internal static class UiTestDocuments
    {
        /// <summary>
        /// Portal frame matching FAP's BuildPortalFrameInput: 4 nodes, 3 frame members,
        /// two fixed bases, a 50-unit horizontal joint load, and a -10 UDL on the top beam.
        /// </summary>
        public static ProjectDocument BuildPortalFrameDocument()
        {
            var doc = new ProjectDocument();

            var n1 = new NodeRowVm { X = 0, Y = 0 };
            var n2 = new NodeRowVm { X = 0, Y = 3 };
            var n3 = new NodeRowVm { X = 4, Y = 3 };
            var n4 = new NodeRowVm { X = 4, Y = 0 };
            doc.Nodes.Add(n1);
            doc.Nodes.Add(n2);
            doc.Nodes.Add(n3);
            doc.Nodes.Add(n4);

            var mat = new MaterialRowVm { ElasticModulus = 200000.0 };
            doc.Materials.Add(mat);

            // Sections in mm, inertia in mm⁴ (= 0.1 m × 0.1 m, I = 1e-4 m⁴ in canonical units).
            var sec = new SectionRowVm { Width = 100, Depth = 100, MomentOfInertia = 1.0e8 };
            doc.Sections.Add(sec);

            doc.Elements.Add(new ElementRowVm { StartNode = n1, EndNode = n2, Material = mat, Section = sec });
            doc.Elements.Add(new ElementRowVm { StartNode = n2, EndNode = n3, Material = mat, Section = sec });
            doc.Elements.Add(new ElementRowVm { StartNode = n4, EndNode = n3, Material = mat, Section = sec });

            doc.Supports.Add(new SupportRowVm { Node = n1, RestrainX = true, RestrainY = true, RestrainRz = true });
            doc.Supports.Add(new SupportRowVm { Node = n4, RestrainX = true, RestrainY = true, RestrainRz = true });

            doc.NodalLoads.Add(new NodalLoadRowVm { Node = n2, Fx = 50.0 });

            doc.DistributedLoads.Add(new DistributedLoadRowVm
            {
                Element = doc.Elements[1],
                MagnitudePerLength = -10.0,
                Direction = LoadDirection.Y
            });

            return doc;
        }

        /// <summary>
        /// A single member with a load but no supports — a rigid-body mechanism the analyzer
        /// must reject (fatal) rather than crash on.
        /// </summary>
        public static ProjectDocument BuildUnstableNoSupportsDocument()
        {
            var doc = new ProjectDocument();

            var n1 = new NodeRowVm { X = 0, Y = 0 };
            var n2 = new NodeRowVm { X = 5, Y = 0 };
            doc.Nodes.Add(n1);
            doc.Nodes.Add(n2);

            var mat = new MaterialRowVm { ElasticModulus = 200000.0 };
            doc.Materials.Add(mat);

            // Sections in mm, inertia in mm⁴ (= 0.1 m × 0.1 m, I = 1e-4 m⁴ in canonical units).
            var sec = new SectionRowVm { Width = 100, Depth = 100, MomentOfInertia = 1.0e8 };
            doc.Sections.Add(sec);

            doc.Elements.Add(new ElementRowVm { StartNode = n1, EndNode = n2, Material = mat, Section = sec });

            doc.NodalLoads.Add(new NodalLoadRowVm { Node = n2, Fy = -10.0 });

            return doc;
        }
    }
}
