using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignInputs.American
{
    public class TimberServicibilityCheckInputUS : TimberCheckInputBaseClassUS
    {
        /// <summary>
        /// Immediate deflection due to the long-term component of the design load, (mm).
        /// </summary>
        public float LongTermDeflection { get; set; }


        /// <summary>
        /// deflection due to the short-term or normal component of the design load, (mm).
        /// </summary>
        public float ShortTermDeflection { get; set; }

        /// <summary>
        /// The length of the member being considered. (mm)
        /// </summary>
        public float MemberLength { get; set; }
        public override eTimberDesignCheckType CheckType
        {
            get { return eTimberDesignCheckType.Serviceability; }
        }
    }
}
