using MemberDesigner.DesignChecks;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignInputs.Eurocode
{
    public class TimberShearCheckInputEU : ITimberDesignCheckInput
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
        /// Design value for tension perpendicular to the grain. (MPa)
        /// </summary>
        public float Ft90 { get; set; }

        /// <summary>
        /// Shear strength (MPa)
        /// </summary>
        public float Fv { get; set; }

        /// <summary>
        /// The area rolling shear is applied into. (mm^2)
        /// </summary>
        public float RollingShearEffectiveArea { get; set; }

        /// <summary>
        /// Maximum shear required (N)
        /// </summary>
        public float MaxShearDemand { get; set; }

        /// <summary>
        /// Maximum rolling shear required (N)
        /// </summary>
        public float MaxRollingShearDemand { get; set; }

        /// <summary>
        /// The maximum demand for torsion (MPa)
        /// Note: later I can refine it if getting stress inputs are not doable!!!!!
        /// </summary>
        public float MaxTorsionStressDemand { get; set; }
        public eTimberMaterialType material { get; set; }

        public eTimberDesignCheckType CheckType
        {
            get { return eTimberDesignCheckType.Shear; }
        }
    }
}
