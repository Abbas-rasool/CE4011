using StructuralLoads;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.Designers
{
    /// <summary>
    /// All inputs needed to design a single timber member, code-agnostic at the call site.
    /// The UI fills this (resolved material strengths from the database, section geometry,
    /// member parameters, project settings, and demands from the analysis result), and
    /// <see cref="TimberDesignCheckInputFactory"/> reads it to build the per-check inputs.
    /// Replaces the static placeholder values previously hardcoded in the factory.
    /// Units: stresses/moduli in MPa, lengths in mm, areas in mm², moduli of section in mm³,
    /// inertia in mm⁴, forces in N, moments in N·mm.
    /// </summary>
    public sealed class TimberMemberDesignContext
    {
        public eTimberCode Code { get; set; }

        // --- Resolved material strength values (consistent SI units across all codes) ---
        public eTimberMaterialType MaterialType { get; set; } = eTimberMaterialType.SolidTimber;
        public float BendingStrength { get; set; }        // f_m,k  / Fb
        public float TensionStrength { get; set; }        // f_t,0,k / Ft
        public float TensionPerpStrength { get; set; }    // f_t,90,k
        public float CompressionStrength { get; set; }    // f_c,0,k / Fc
        public float CompressionPerpStrength { get; set; }// f_c,90,k / Fc90
        public float ShearStrength { get; set; }          // f_v,k  / Fv
        public float ModulusMean { get; set; }            // E_0,mean / E
        public float ModulusBuckling { get; set; }        // E_0,05  / Emin
        public float Density { get; set; }                // ρ_k

        // --- Section geometry ---
        public float H1 { get; set; }                     // depth, major axis
        public float H2 { get; set; }                     // depth, minor axis
        public float GrossArea { get; set; }
        public float NetArea { get; set; }
        public float SectionModulusMajor { get; set; }
        public float SectionModulusMinor { get; set; }
        public float Inertia { get; set; }
        public float FirstMomentOfArea { get; set; }      // Q at neutral axis

        // --- Demands (from analysis; minor moment = 0 in the 2D solver) ---
        public float MomentMajor { get; set; }
        public float MomentMinor { get; set; }
        public float AxialTension { get; set; }           // ≥ 0
        public float AxialCompression { get; set; }       // ≥ 0
        public float Shear { get; set; }

        // --- Environment & factors ---
        public eServiceClass ServiceClass { get; set; }                  // EC5 / TR
        public eMoistureContentCondition MoistureCondition { get; set; } // US
        public eLoadDurationClass LoadDurationClass { get; set; }        // EC5 / TR
        public float LoadDurationFactor { get; set; } = 1f;              // US C_D
        public float TimeEffectFactor { get; set; } = 1f;               // US λ
        public float Temperature { get; set; } = 20f;
        public bool FactorsModified { get; set; }
        public float PartialFactor { get; set; }
        public float ModificationFactor { get; set; }

        /// <summary>US (ASCE 7) combination format — LRFD or ASD. Selects the NDS design path
        /// (LRFD uses the time-effect factor λ; ASD uses the load-duration factor C_D).</summary>
        public eLoadCombinationType DesignMethod { get; set; } = eLoadCombinationType.ASD;

        // --- Member parameters ---
        public float EffectiveLengthMajor { get; set; }
        public float EffectiveLengthMinor { get; set; }
        public float EffectiveBeamLength { get; set; }
        public float BucklingLengthCoeMajor { get; set; } = 1f;
        public float BucklingLengthCoeMinor { get; set; } = 1f;
        public bool IsLaterallySupported { get; set; } = true;
        public bool IsRepetitiveMember { get; set; }
        public eSupportTypeEU SupportType { get; set; } = eSupportTypeEU.Continuous;
        public float AppliedAngle { get; set; }

        // --- US grade / configuration (solid members only this phase) ---
        public eTimberGrades TimberGrade { get; set; } = eTimberGrades.No2;
        public eTimberType TimberType { get; set; } = eTimberType.SL;
        public eMemberConfigurationType MemberConfiguration { get; set; } = eMemberConfigurationType.SolidMembers;
        public eApplicationType ApplicationType { get; set; } = eApplicationType.Member;
    }
}
