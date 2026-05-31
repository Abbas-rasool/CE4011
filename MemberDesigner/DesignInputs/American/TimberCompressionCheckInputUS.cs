using MemberDesigner.DesignInputs.American;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignInputs.American
{

    /// <summary>
    /// Represents the input parameters for a timber compression design check in American standards.
    /// </summary>
    public class TimberCompressionCheckInputUS : TimberCheckInputBaseClassUS
    {
        /// <summary>
        /// bearing length measured parallel to grain (mm) 
        /// </summary>
        public float BearingLength { get; set; }

        /// <summary>
        /// first length parameter (ℓ₁) representing the horizontal dimension of the timber (major) (mm).
        /// </summary>
        public float Length1 { get; set; }

        /// <summary>
        /// second length parameter (ℓ₂) representing the horizontal dimension of the timber (minor) (mm).
        /// </summary>
        public float Length2 { get; set; }

        /// <summary>
        /// Distance from center of spacer block to centroid of the group of split ring or shear plate connectors in end blocks (mm)
        /// </summary>
        public float Length3 { get; set; }

        /// <summary>
        /// End distance is the distance from the end of the timber (i.e., top or bottom) to the center of the first connector (split ring or shear plate) in the end block. (mm)
        /// </summary>
        public float EndDistance { get; set; }

        /// <summary>
        /// buckling length coeficient (major)
        /// </summary>
        public float BucklingLengthCoe1 { get; set; }

        /// <summary>
        /// buckling length coeficient (minor)
        /// </summary>
        public float BucklingLengthCoe2 { get; set; }

        /// <summary>
        /// modulus of elasticity (MPa)
        /// </summary>
        public float E { get; set; }

        /// <summary>
        /// adjusted modulus of elasticity (MPa)
        /// </summary>
        public float Emin { get; set; }

        /// <summary>
        /// Design value for compression parallel to the grain. (MPa)
        /// </summary>
        public float Fc { get; set; }

        /// <summary>
        /// Design value for compression perpendicular to the grain. (MPa)
        /// </summary>
        public float Fc90 { get; set; }

        /// <summary>
        /// All material removed by boring, grooving, dapping, notching, or other means should be deducted (check NDS 3.1.2). (mm^2)
        /// Section area should be the one parallel to the grain direction.
        /// </summary>
        public float NetSectionAreaParallel { get; set; }

        /// <summary>
        /// All material removed by boring, grooving, dapping, notching, or other means should be deducted (check NDS 3.1.2). (mm^2)       
        /// Section area should be the one perpendicular to the grain direction.
        /// </summary>
        public float NetSectionAreaPerpendicular { get; set; }

        /// <summary>
        /// Gross Section Area (mm^2) 
        /// </summary>
        public float GrossSectionArea { get; set; }

        /// <summary>
        /// Maximum compression demand on the member (Parallel to grain direction) (N)
        /// </summary>
        public float MaxCompressionDemandParallel { get; set; }

        /// <summary>
        /// Maximum compression demand on the member (Perpendicular to grain direction) (N)
        /// </summary>
        public float MaxCompressionDemandPerpendicular { get; set; }

        public eBuiltUPColumnConnectionType builtUPColumnType { get; set; }
        public bool IsRepetitiveMember { get; set; }
        public override eTimberDesignCheckType CheckType
        {
            get { return eTimberDesignCheckType.Compression; }
        }
    }

}
