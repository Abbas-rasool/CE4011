using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignInputs.American
{
    public class TimberShearCheckInputUS : TimberCheckInputBaseClassUS
    {

        /// <summary>
        /// Moment of inertia of the section about the neutral axis (mm^4)
        /// </summary>
        public float Inertia { get; set; }

        /// <summary>
        /// First moment of area about the neutral axis for the area above (or below) the point of interest (mm^3)
        /// </summary>
        public float MomentofArea { get; set; }

        /// <summary>
        /// Design value for Shear parallel to the grain. (MPa)
        /// </summary>
        public float Fv { get; set; }

        /// <summary>
        /// Maximum shear required (N)
        /// It is only checked for major axis, NDS hasn't given anything to check for rolling shear!
        /// </summary>
        public float MaxShearDemand { get; set; }

        public override eTimberDesignCheckType CheckType
        {
            get { return eTimberDesignCheckType.Shear; }
        }
    }
}
