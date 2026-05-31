using MemberDesigner.DesignChecks;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignInputs.American
{
    public class TimberCombinedBendingAxialCheckInputUS : ITimberDesignCheckInput
    {
        public eMemberConfigurationType memberConfigurationType { get; set; }

        /// <summary>
        /// Max demand moment - major axis (N.mm)
        /// </summary>
        public float MaxDemandMomentMajor { get; set; }

        /// <summary>
        /// Max demand moment - minor axis (N.mm)
        /// </summary>
        public float MaxDemandMomentMinor { get; set; }

        public eTimberDesignCheckType CheckType
        {
            get { return eTimberDesignCheckType.CombinedBendingAxial; }
        }
    }
}
