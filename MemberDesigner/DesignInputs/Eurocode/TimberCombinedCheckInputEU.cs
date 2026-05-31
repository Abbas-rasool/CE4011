using MemberDesigner.DesignChecks;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignInputs.Eurocode
{
    public class TimberCombinedCheckInputEU : ITimberDesignCheckInput
    {
        /// <summary>
        /// Design value for Bending. (MPa)
        /// </summary>
        public float Fm { get; set; }

        /// <summary>
        /// the effective length of the beam, depending on the suppport conditions and the load configuration. (mm)
        /// </summary>
        public float EffectiveBeamLength { get; set; }

        /// <summary>
        /// 5 percentile modulus of elasticitiy parallel bending.
        /// </summary>
        public float E_005 { get; set; }

        /// <summary>
        /// Design value for compression parallel to the grain. (MPa)
        /// </summary>
        public float Fc { get; set; }

        /// <summary>
        /// Effective length in the width face of the member (mm)
        /// </summary>
        public float MajorEffectiveLength { get; set; }

        /// <summary>
        /// Effective length in the thickness face of the member (mm)
        /// </summary>
        public float MinorEffectiveLength { get; set; }

        /// <summary>
        /// The height of the cross section - Major Axis (mm)
        /// </summary>
        public float h1 { get; set; }

        /// <summary>
        /// The height of the cross section - Minor Axis (mm)
        /// </summary>
        public float h2 { get; set; }

        public eTimberMaterialType material { get; set; }

        public eTimberDesignCheckType CheckType
        {
            get { return eTimberDesignCheckType.CombinedBendingAxial; }
        }
    }
}
