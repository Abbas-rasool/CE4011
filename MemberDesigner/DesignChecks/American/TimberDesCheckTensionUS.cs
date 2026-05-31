using MemberDesigner.DesignInputs.American;
using MemberDesigner.DesignChecks.Helpers;
using MemberDesigner.TimberDesignData.American;
using MemberDesigner.TimberDesignData.BaseClasses;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignChecks.American
{
    public class TimberDesCheckTensionUS : ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData>
    {

        #region CTOR

        public TimberDesCheckTensionUS()
        {
            _dependencies = new List<eTimberDesignCheckType>() { };
        }

        #endregion

        #region Private Fields
        private List<eTimberDesignCheckType> _dependencies;
        #endregion

        #region Public Fields
        public List<eTimberDesignCheckType> Dependencies { get => _dependencies; }
        public eTimberDesignCheckType CheckType => eTimberDesignCheckType.Tension;

        #endregion

        /// <summary>
        /// This method checks the tension capacity of the member.
        /// </summary>
        public TimberDesignCheckData PerformCheck(ITimberDesignCheckInput input, params TimberDesignCheckData[] dependencies)
        {
            if (!(input is TimberTensionCheckInputUS castedInput))
                throw new ArgumentException("Input argument is not of the correct type");

            var checkData = new TimberDesCheckDataTensionUS();
            TimberDesignHelperAmerican timberDesignHelper = TimberDesignHelperAmerican.GetInstance();
            checkData.DesignStatus = eDesignStatus.Pass;

            switch (castedInput.TimberType)
            {
                case eTimberType.SL:

                    timberDesignHelper.GetAdjustmentFactorsSL(castedInput, checkData);
                    break;

                default:

                    // Unsupported timber type
                    return null;
            }


            double referenceTensionDesignValue = castedInput.Ft *
                checkData.LoadDurationFactor *
                checkData.WetServiceFactor *
                checkData.TemperatureFactor *
                checkData.SizeFactor *
                checkData.IncisingFactor *
                checkData.FormatConversionFactor *
                checkData.ResistanceFactor *
                checkData.TimeEffectFactor;

            checkData.AdjustedTensionDesignValue = (float)referenceTensionDesignValue;

            double demandStress = castedInput.MaxTensionDemand / castedInput.NetSectionArea;
            checkData.TensionDemandStress = (float)demandStress;

            // Capacity Check Perpendicular
            if (demandStress > checkData.AdjustedTensionDesignValue)
            {
                checkData.DesignStatus = eDesignStatus.Fail;
            }


            // Configuration type dispatch
            //switch (castedInput.memberConfigurationType)
            //{
            //    case eMemberConfigurationType.SolidMembers:
            //    case eMemberConfigurationType.BuiltUpColumns:
            //        break;

            //    case eMemberConfigurationType.SpacedColumns:
            //        break;

            //    default:

            //        // Unknown configuration
            //        checkData.DesignStatus = PBTypes.Common.eDesignStatus.Fail;
            //        break;
            //}

            return checkData;
        }
    }
}
