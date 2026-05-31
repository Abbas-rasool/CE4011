using MemberDesigner.DesignChecks;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignInputs.Eurocode
{
    public class TimberTensionCheckInputEU : ITimberDesignCheckInput
    {

        /// <summary>
        /// Design value for tension parallel to the grain. (MPa)
        /// </summary>
        public float Ft { get; set; }

        /// <summary>
        /// All material removed by boring, grooving, dapping, notching, or other means should be deducted.
        /// </summary>
        public float NetSectionArea { get; set; }

        /// <summary>
        /// Maximum tension required parallel to the grain (N)
        /// </summary>
        public float MaxTensionDemand { get; set; }

        /// <summary>
        /// Maximum tension required perpendicular to the grain (N)
        /// </summary>
        public float MaxTensionDemand90 { get; set; }

        public eTimberDesignCheckType CheckType
        {
            get { return eTimberDesignCheckType.Tension; }
        }
    }
}
