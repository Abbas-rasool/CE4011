using MemberDesigner.DesignChecks;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignInputs.Turkish
{
    public class TimberParametersCheckInputTS : ITimberDesignCheckInput
    {
        /// <summary>
        /// Size effect coeficient.
        /// </summary>
        public float S { get; set; }
        public float C_N { get; set; }
        public float C_B { get; set; }
        public float C_Y { get; set; }
        public float Omega { get; set; }

        /// <summary>
        /// The length of the section in mm.
        /// </summary>
        public float SectionLength { get; set; }

        /// <summary>
        /// the characteristic density of the section being used in kg/m^3 (same units given in the input table)
        /// </summary>
        public float CharacteristicDensity { get; set; }

        public eTimberMaterialType material { get; set; }
        public eServiceClass serviceClass { get; set; }
        public eLoadDurationClass loadDurationClass { get; set; }

        public bool IsFactorsModified { get; set; }

        /// <summary>
        /// The height of the cross section - Major Axis (mm)
        /// </summary>
        public float h1 { get; set; }

        /// <summary>
        /// The height of the cross section - Minor Axis (mm)
        /// </summary>
        public float h2 { get; set; }

        public eTimberDesignCheckType CheckType
        {
            get { return eTimberDesignCheckType.Parameters; }
        }
    }
}
