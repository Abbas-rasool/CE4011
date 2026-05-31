using MemberDesigner.DesignChecks;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignInputs.Eurocode
{
    public class TimberBendingCheckInputEU : ITimberDesignCheckInput
    {
        /// <summary>
        /// The height of the cross section - Major Axis (mm)
        /// </summary>
        public float h1 { get; set; }

        /// <summary>
        /// The height of the cross section - Minor Axis (mm)
        /// </summary>
        public float h2 { get; set; }

        /// <summary>
        /// Design value for Bending. (MPa)
        /// </summary>
        public float Fm { get; set; }

        /// <summary>
        /// Max demand moment - major axis (N.mm)
        /// </summary>
        public float MaxDemandMomentMajor { get; set; }

        /// <summary>
        /// Max demand moment - minor axis (N.mm)
        /// </summary>
        public float MaxDemandMomentMinor { get; set; }
        public eTimberMaterialType material { get; set; }

        public eTimberDesignCheckType CheckType
        {
            get { return eTimberDesignCheckType.Bending; }
        }
    }
}
