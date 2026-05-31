using MemberDesigner.DesignInputs.American;
using MemberDesigner.TimberDesignData.American;
using MemberDesigner.TimberDesignData.BaseClasses;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignChecks.American
{
    public class TimberDesCheckServiceabilityUS : ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData>
    {

        #region CTOR

        public TimberDesCheckServiceabilityUS()
        {
            _dependencies = new List<eTimberDesignCheckType>() { };
        }

        #endregion

        #region Private Fields
        private List<eTimberDesignCheckType> _dependencies;
        #endregion


        #region Public Fields
        public List<eTimberDesignCheckType> Dependencies { get => _dependencies; }
        public eTimberDesignCheckType CheckType => eTimberDesignCheckType.Serviceability;

        #endregion


        /// <summary>
        /// This method checks the deflection limit for the members
        /// </summary>
        public TimberDesignCheckData PerformCheck(ITimberDesignCheckInput input, params TimberDesignCheckData[] dependencies)
        {
            if (!(input is TimberServicibilityCheckInputUS castedInput))
                throw new ArgumentException("Input argument is not of the correct type");

            var checkData = new TimberDesCheckDataShearUS();
            checkData.DesignStatus = eDesignStatus.Pass;

            // Total deflection
            double deformationFactor = GetTimeDependentDeformationFactor(castedInput.TimberType, castedInput.MoistureContentCondition);
            double totalDeflection = deformationFactor * castedInput.LongTermDeflection + castedInput.ShortTermDeflection;

            double deflectionLimit = castedInput.MemberLength / 360;

            if (deflectionLimit > totalDeflection)
            {
                checkData.DesignStatus = eDesignStatus.Fail;
                checkData.DesignArguments.Add(eTimberMemberDesignArgument.IncreaseCrossSectionArea);
                checkData.DesignArguments.Add(eTimberMemberDesignArgument.ReduceMemberLength);
            }

            return checkData;
        }

        #region Private Methods
        /// <summary>
        /// This method returns the time dependent deformation (creep) factor.
        /// </summary>
        private double GetTimeDependentDeformationFactor(eTimberType timberType, eMoistureContentCondition moistureContentCondition)
        {
            if (moistureContentCondition == eMoistureContentCondition.DryServiceConditon)
            {
                switch(timberType){

                    case eTimberType.SL:
                    case eTimberType.SGLT:
                    case eTimberType.PWJ:
                    case eTimberType.SCL:
                        return 1.5;

                    case eTimberType.WSP:
                    case eTimberType.CLT:
                        return 2.0;

                    // returning 2 (highest given) for the types not mentioned.
                    default:
                        return 2.0;
                }
            }
            else
            {
                switch (timberType)
                {
                    case eTimberType.SGLT:
                    case eTimberType.SL:
                        return 2.0;

                    // returning 2 (highest given) for the types not mentioned.
                    default:
                        return 2.0;
                }
            }
        }

        #endregion
    }
}
