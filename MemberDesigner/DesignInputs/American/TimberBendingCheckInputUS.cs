using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignInputs.American
{
    public class TimberBendingCheckInputUS : TimberCheckInputBaseClassUS
    {
        public float StudSpacing { get; set; }

        /// <summary>
        /// Design value for Bending. (MPa)
        /// </summary>
        public float Fb { get; set; }

        /// <summary>
        /// Effective length (unsupported length) under bending (Major) (mm)
        /// </summary>
        public float EffectiveLengthMajor { get; set; }

        /// <summary>
        /// Effective length (unsupported length) under bending (Minor) (mm)
        /// </summary>
        public float EffectiveLengthMinor { get; set; }

        /// <summary>
        /// Max demand moment - major axis (N.mm)
        /// </summary>
        public float MaxDemandMomentMajor { get; set; }

        /// <summary>
        /// Max demand moment - minor axis (N.mm)
        /// </summary>
        public float MaxDemandMomentMinor { get; set; }

        /// <summary>
        /// modulus of elasticity (MPa)
        /// </summary>
        public float E { get; set; }

        /// <summary>
        /// adjusted modulus of elasticity (MPa)
        /// </summary>
        public float Emin { get; set; }

        /// <summary>
        /// buckling length coeficient for the flat dimention of the spaced columns (dimension two in the picture) (mm)
        /// </summary>
        public float BucklingLengthCoe { get; set; }

        /// <summary>
        /// End distance is the distance from the end of the timber (i.e., top or bottom) to the center of the first connector (split ring or shear plate) in the end block. (mm)
        /// </summary>
        public float EndDistance { get; set; }

        /// <summary>
        /// The section modulus for major access (mm^3)
        /// </summary>
        public float SectionModulusMajor { get; set; }

        /// <summary>
        /// The section modulus for minor acess (mm^3)
        /// </summary>
        public float SectionModulusMinor { get; set; }

        /// <summary>
        /// When rectangular sawn lumber bending members are laterally supported in accordance with 4.4.1, C_L = 1
        /// When the compression edge of a bending member is supported throughout it's length to prevent lateral displacement, and the ends at points of bearing have lateral support to prevent rotation, C_L = 1
        /// </summary>
        public bool IsLaterallySupported { get; set; } = true;
        public bool IsRepetitiveMember { get; set; }

        public override eTimberDesignCheckType CheckType
        {
            get { return eTimberDesignCheckType.Bending; }
        }
    }
}
