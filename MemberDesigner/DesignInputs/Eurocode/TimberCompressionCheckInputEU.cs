using MemberDesigner.DesignChecks;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignInputs.Eurocode
{
    public class TimberCompressionCheckInputEU : ITimberDesignCheckInput
    {
        /// <summary>
        /// Compressive stresses at an angle to the grain.
        /// </summary>
        public float AppliedAngle { get; set; }

        /// <summary>
        /// All material removed by boring, grooving, dapping, notching, or other means should be deducted.
        /// </summary>
        public float NetSectionArea { get; set; }

        /// <summary>
        /// the effective contact area in compression perpendicular to the grain.
        /// </summary>
        public float EffectiveArea90 { get; set; }

        /// <summary>
        /// It stores the type of support applied to a structural element (e.g., continuous or discrete), which influences how loads are distributed
        /// </summary>
        public eSupportTypeEU SupportType { get; set; }

        /// <summary>
        /// Design value for compression perpendicular to the grain. (MPa)
        /// </summary>
        public float Fc90 { get; set; }

        /// <summary>
        /// Design value for compression parallel to the grain. (MPa)
        /// </summary>
        public float Fc { get; set; }

        public float MaxCompressionAppliedAngled { get; set; }
        public float MaxCompressionDemandParallel { get; set; }
        public float MaxCompressionDemandPerpendicular { get; set; }

        public eTimberDesignCheckType CheckType
        {
            get { return eTimberDesignCheckType.Compression; }
        }
    }
}
