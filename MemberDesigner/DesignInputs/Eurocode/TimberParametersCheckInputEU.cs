using MemberDesigner.DesignChecks;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignInputs.Eurocode
{
    public class TimberParametersCheckInputEU : ITimberDesignCheckInput
    {

        public eTimberMaterialType material { get; set; }
        public eServiceClass serviceClass { get; set; }
        public eLoadDurationClass loadDurationClass { get; set; }
        public float PartialFactor { get; set; }
        public float ModificationFactor { get; set; }

        public bool IsFactorsModified { get; set; }

        public eTimberDesignCheckType CheckType
        {
            get { return eTimberDesignCheckType.Parameters; }
        }
    }
}
