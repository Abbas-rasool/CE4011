using MemberDesigner.DesignChecks;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignInputs.Turkish
{
    public class TimberCompressionCheckInputTS : ITimberDesignCheckInput
    {

        /// <summary>
        /// Compressive stresses at an angle to the grain.
        /// </summary>
        public float AppliedAngle { get; set; }

        /// <summary>
        /// Section area should be the one perpendicular to the grain direction. (Recheck this for area increase later)!!! (mm^2)
        /// </summary>
        public float NetSectionAreaPerpendicular { get; set; }

        /// <summary>
        /// The length of the section in mm.
        /// </summary>
        public float SectionLength { get; set; }

        /// <summary>
        /// the characteristic density of the section being used in kg/m^3 (same units given in the input table)
        /// </summary>
        public float CharacteristicDensity { get; set; }

        /// <summary>
        /// Characteristic compressive strength parallel to grain (MPa)
        /// </summary>
        public float f_c0k { get; set; }

        /// <summary>
        /// Characteristic compressive strength perpendicular to the grain (MPa)
        /// </summary>
        public float f_c90k { get; set; }


        /// <summary>
        /// The effective elastic modulus (E, at 0.05 percentile) (MPa)
        /// </summary>
        public float E_005 { get; set; }

        /// <summary>
        /// The height of the cross section - Major Axis (mm)
        /// </summary>
        public float h1 { get; set; }

        /// <summary>
        /// The height of the cross section - Minor Axis (mm)
        /// </summary>
        public float h2 { get; set; }

        /// <summary>
        /// buckling length coeficient (major)
        /// </summary>
        public float BucklingLengthCoe1 { get; set; }

        /// <summary>
        /// buckling length coeficient (minor)
        /// </summary>
        public float BucklingLengthCoe2 { get; set; }

        /// <summary>
        /// first length parameter (ℓ₁) (major) (mm).
        /// </summary>
        public float Length1 { get; set; }

        /// <summary>
        /// second length parameter (ℓ₂) (minor) (mm).
        /// </summary>
        public float Length2 { get; set; }

        /// <summary>
        /// cross sectional gross area (mm^2)
        /// </summary>
        public float SectionGrossArea { get; set; }

        /// <summary>
        /// Maximum compression demand (N)
        /// </summary>
        public float MaxCompressionDemandParallel { get; set; }

        /// <summary>
        /// Maximum compression demand on the member (Perpendicular to grain direction) (N)
        /// </summary>
        public float MaxCompressionDemandPerpendicular { get; set; }

        /// <summary>
        /// Maximum compression demand on the member (at an angle to grain direction) (N)
        /// </summary>
        public float MaxCompressionDemandAngled { get; set; }
        public eTimberMaterialType material { get; set; }

        public eTimberDesignCheckType CheckType
        {
            get { return eTimberDesignCheckType.Compression; }
        }
    }
}
