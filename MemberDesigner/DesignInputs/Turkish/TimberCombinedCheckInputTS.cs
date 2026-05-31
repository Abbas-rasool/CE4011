using MemberDesigner.DesignChecks;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignInputs.Turkish
{
    public class TimberCombinedCheckInputTS : ITimberDesignCheckInput
    {
        public eTimberMaterialType material { get; set; }

        public eTimberDesignCheckType CheckType
        {
            get { return eTimberDesignCheckType.CombinedBendingAxial; }
        }
    }
}
