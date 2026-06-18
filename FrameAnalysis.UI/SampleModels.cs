using FrameAnalysis.UI.Core.Documents;
using FrameAnalysis.UI.Core.Documents.Rows;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Loads;

namespace FrameAnalysis.UI
{
    /// <summary>
    /// Seed documents so a fresh launch shows something to look at. Temporary convenience for
    /// early development — file open / new-project will replace this later (Phase 7).
    /// </summary>
    internal static class SampleModels
    {
        /// <summary>A fixed-base portal frame with a lateral joint load and a UDL on the beam.</summary>
        public static ProjectDocument PortalFrame()
        {
            var doc = new ProjectDocument { ProjectName = "Portal Frame (sample)" };

            var n1 = new NodeRowVm { X = 0, Y = 0 };
            var n2 = new NodeRowVm { X = 0, Y = 3 };
            var n3 = new NodeRowVm { X = 4, Y = 3 };
            var n4 = new NodeRowVm { X = 4, Y = 0 };
            doc.Nodes.Add(n1);
            doc.Nodes.Add(n2);
            doc.Nodes.Add(n3);
            doc.Nodes.Add(n4);

            var mat = new MaterialRowVm { Name = "Steel", ElasticModulus = 200000.0 };
            doc.Materials.Add(mat);

            var sec = new SectionRowVm { Name = "0.1 x 0.1", Width = 0.1, Depth = 0.1, MomentOfInertia = 0.0001 };
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
    }
}
