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

        /// <summary>Q4 (slope-deflection demo). A two-storey frame on a single left column:
        /// A→B→D vertically, with beams B–C and D–E reaching 3 m to the right. Supports at
        /// A, C and E are pins; EI is constant (every member shares one material and section).
        /// Loads: 15 kN and 10 kN lateral at B and D, 16 kN/m on B–C and 12 kN/m on D–E.</summary>
        public static ProjectDocument Q4Frame()
        {
            var doc = new ProjectDocument { ProjectName = "Q4 — Slope-Deflection Frame" };

            // Nodes (m): left column A(0,0) → B(0,4) → D(0,8); beams reach 3 m right to C and E.
            var a = new NodeRowVm { X = 0, Y = 0 };
            var b = new NodeRowVm { X = 0, Y = 4 };
            var c = new NodeRowVm { X = 3, Y = 4 };
            var d = new NodeRowVm { X = 0, Y = 8 };
            var e = new NodeRowVm { X = 3, Y = 8 };
            doc.Nodes.Add(a);
            doc.Nodes.Add(b);
            doc.Nodes.Add(c);
            doc.Nodes.Add(d);
            doc.Nodes.Add(e);

            // One material + one section shared by every member ⇒ EI is constant (per the problem).
            var mat = new MaterialRowVm { Name = "Timber C24", StrengthClass = eStrengthClass.C24 };
            doc.Materials.Add(mat);

            var sec = new SectionRowVm
            {
                Name = "300 x 500",
                Width = 300.0,
                Depth = 500.0,
                MomentOfInertia = 300.0 * 500.0 * 500.0 * 500.0 / 12.0
            };
            doc.Sections.Add(sec);

            // Members: two column segments (A–B, B–D) and two beams (B–C, D–E).
            var colAB = new ElementRowVm { StartNode = a, EndNode = b, Material = mat, Section = sec };
            var colBD = new ElementRowVm { StartNode = b, EndNode = d, Material = mat, Section = sec };
            var beamBC = new ElementRowVm { StartNode = b, EndNode = c, Material = mat, Section = sec };
            var beamDE = new ElementRowVm { StartNode = d, EndNode = e, Material = mat, Section = sec };
            doc.Elements.Add(colAB);
            doc.Elements.Add(colBD);
            doc.Elements.Add(beamBC);
            doc.Elements.Add(beamDE);

            // Supports: A, C and E are pins — restrain both translations, free rotation.
            doc.Supports.Add(new SupportRowVm { Node = a, RestrainX = true, RestrainY = true, RestrainRz = false });
            doc.Supports.Add(new SupportRowVm { Node = c, RestrainX = true, RestrainY = true, RestrainRz = false });
            doc.Supports.Add(new SupportRowVm { Node = e, RestrainX = true, RestrainY = true, RestrainRz = false });

            // Lateral joint loads (kN, +X to the right).
            doc.NodalLoads.Add(new NodalLoadRowVm { Node = b, Fx = 15.0 });
            doc.NodalLoads.Add(new NodalLoadRowVm { Node = d, Fx = 10.0 });

            // Gravity UDLs (kN/m, downward) on the beams.
            doc.DistributedLoads.Add(new DistributedLoadRowVm { Element = beamBC, MagnitudePerLength = -16.0, Direction = LoadDirection.Y });
            doc.DistributedLoads.Add(new DistributedLoadRowVm { Element = beamDE, MagnitudePerLength = -12.0, Direction = LoadDirection.Y });

            // Effective lengths = member lengths for the design checks (columns 4 m, beams 3 m).
            doc.MemberDesigns[0].EffectiveLengthMajor = doc.MemberDesigns[0].EffectiveLengthMinor = doc.MemberDesigns[0].EffectiveBeamLength = 4.0;
            doc.MemberDesigns[1].EffectiveLengthMajor = doc.MemberDesigns[1].EffectiveLengthMinor = doc.MemberDesigns[1].EffectiveBeamLength = 4.0;
            doc.MemberDesigns[2].EffectiveLengthMajor = doc.MemberDesigns[2].EffectiveLengthMinor = doc.MemberDesigns[2].EffectiveBeamLength = 3.0;
            doc.MemberDesigns[3].EffectiveLengthMajor = doc.MemberDesigns[3].EffectiveLengthMinor = doc.MemberDesigns[3].EffectiveBeamLength = 3.0;

            return doc;
        }
    }
}
