using MemberDesigner.DesignChecks;
using StructuralLoads;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignInputs.American
{
    public abstract class TimberCheckInputBaseClassUS : ITimberDesignCheckInput
    {
        public abstract eTimberDesignCheckType CheckType { get; }

        /// <summary>
        /// The height of the cross section - Major Axis (mm)
        /// </summary>
        public float h1 { get; set; }

        /// <summary>
        /// The height of the cross section - Minor Axis (mm)
        /// </summary>
        public float h2 { get; set; }

        /// <summary>
        /// Temperature value in degrees Celsius.
        /// </summary>
        public float Temperature { get; set; }

        public float TimeEffectFactor { get; set; }
        public float LoadDurationFactor { get; set; }
        public bool IsLumberIncised { get; set; }

        public eTimberType TimberType { get; set; }
        public eDesignParameter designParameter { get; set; }
        public eMemberConfigurationType memberConfigurationType { get; set; }
        public eMoistureContentCondition MoistureContentCondition { get; set; }
        public eTimberGrades TimberGrade { get; set; }
        public eLoadCombinationType loadCombinationType { get; set; }
        public eApplicationType ApplicationType {get; set;}
    }

}
