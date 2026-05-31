using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignInputs.American
{
    public class TimberTensionCheckInputUS : TimberCheckInputBaseClassUS
    {

        /// <summary>
        /// Design value for tension parallel to the grain. (MPa)
        /// </summary>
        public float Ft { get; set; }

        /// <summary>
        /// All material removed by boring, grooving, dapping, notching, or other means should be deducted (check NDS 3.1.2). (mm^2)
        /// </summary>
        public float NetSectionArea { get; set; }

        /// <summary>
        /// Maximum tension required (N)
        /// </summary>
        public float MaxTensionDemand { get; set; }
        public override eTimberDesignCheckType CheckType
        {
            get { return eTimberDesignCheckType.Tension; }
        }
    }
}
