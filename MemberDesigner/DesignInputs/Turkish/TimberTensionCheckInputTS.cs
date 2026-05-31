using MemberDesigner.DesignChecks;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignInputs.Turkish
{
    public class TimberTensionCheckInputTS : ITimberDesignCheckInput
    {

        /// <summary>
        /// All material removed by boring, grooving, dapping, notching, or other means should be deducted.
        /// Note: There's a method in helper class to calculate this. Maybe this check can be updated depending on UI implementations!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        /// </summary>
        public float NetSectionArea { get; set; }

        /// <summary>
        /// Design value for tension parallel to the grain. (MPa)
        /// </summary>
        public float Ft { get; set; }

        /// <summary>
        /// Maximum tension required parallel to the grain (N)
        /// </summary>
        public float MaxDemandTension { get; set; }

        public eTimberDesignCheckType CheckType
        {
            get { return eTimberDesignCheckType.Tension; }
        }
    }
}
