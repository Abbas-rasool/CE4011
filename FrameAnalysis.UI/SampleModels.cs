using FrameAnalysis.UI.Core.Documents;
using FrameAnalysis.UI.Core.Documents.Rows;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Loads;
using static MemberDesigner.Designers.Enums;

namespace FrameAnalysis.UI
{
    /// <summary>
    /// Seed documents so a fresh launch shows something to look at. Temporary convenience for
    /// early development — file open / new-project will replace this later (Phase 7).
    /// </summary>
    internal static class SampleModels
    {
        /// <summary>A fixed-base C24 timber portal frame with a lateral joint load and a gravity
        /// UDL on the rafter — sized so the design checks pass with a sensible margin.</summary>
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

            // C24 timber: picking the strength class auto-fills the design values and sets E (≈11 GPa).
            var mat = new MaterialRowVm { Name = "Timber C24", StrengthClass = eStrengthClass.C24 };
            doc.Materials.Add(mat);

            // 150 × 300 mm section (mm / mm⁴); I about the in-plane bending axis = b·d³/12.
            var sec = new SectionRowVm
            {
                Name = "150 x 300",
                Width = 150.0,
                Depth = 300.0,
                MomentOfInertia = 150.0 * 300.0 * 300.0 * 300.0 / 12.0
            };
            doc.Sections.Add(sec);

            doc.Elements.Add(new ElementRowVm { StartNode = n1, EndNode = n2, Material = mat, Section = sec });
            doc.Elements.Add(new ElementRowVm { StartNode = n2, EndNode = n3, Material = mat, Section = sec });
            doc.Elements.Add(new ElementRowVm { StartNode = n4, EndNode = n3, Material = mat, Section = sec });

            doc.Supports.Add(new SupportRowVm { Node = n1, RestrainX = true, RestrainY = true, RestrainRz = true });
            doc.Supports.Add(new SupportRowVm { Node = n4, RestrainX = true, RestrainY = true, RestrainRz = true });

            // Modest timber-scale loads: 8 kN lateral at the eaves, 8 kN/m gravity on the rafter.
            doc.NodalLoads.Add(new NodalLoadRowVm { Node = n2, Fx = 8.0 });

            doc.DistributedLoads.Add(new DistributedLoadRowVm
            {
                Element = doc.Elements[1],
                MagnitudePerLength = -8.0,
                Direction = LoadDirection.Y
            });

            // Effective lengths = member lengths (columns 3 m, rafter 4 m) for the design checks.
            doc.MemberDesigns[0].EffectiveLengthMajor = doc.MemberDesigns[0].EffectiveLengthMinor = doc.MemberDesigns[0].EffectiveBeamLength = 3.0;
            doc.MemberDesigns[1].EffectiveLengthMajor = doc.MemberDesigns[1].EffectiveLengthMinor = doc.MemberDesigns[1].EffectiveBeamLength = 4.0;
            doc.MemberDesigns[2].EffectiveLengthMajor = doc.MemberDesigns[2].EffectiveLengthMinor = doc.MemberDesigns[2].EffectiveBeamLength = 3.0;

            return doc;
        }
    }
}
