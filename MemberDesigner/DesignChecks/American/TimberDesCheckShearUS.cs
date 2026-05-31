using MemberDesigner.DesignInputs.American;
using MemberDesigner.DesignChecks.Helpers;
using MemberDesigner.TimberDesignData.American;
using MemberDesigner.TimberDesignData.BaseClasses;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignChecks.American
{
    public class TimberDesCheckShearUS : ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData>
    {

        #region CTOR

        public TimberDesCheckShearUS()
        {
            _dependencies = new List<eTimberDesignCheckType>() { };
        }

        #endregion

        #region Private Fields
        private List<eTimberDesignCheckType> _dependencies;
        #endregion


        #region Public Fields
        public List<eTimberDesignCheckType> Dependencies { get => _dependencies; }
        public eTimberDesignCheckType CheckType => eTimberDesignCheckType.Shear;

        #endregion

        /// <summary>
        /// This method checks the shear capacity of members.
        /// 3.4.1: A check of the strength of wood bending members in shear perpendicular to grain is not required.
        /// </summary>
        public TimberDesignCheckData PerformCheck(ITimberDesignCheckInput input, params TimberDesignCheckData[] dependencies)
        {
            if (!(input is TimberShearCheckInputUS castedInput))
                throw new ArgumentException("Input argument is not of the correct type");

            var checkData = new TimberDesCheckDataShearUS();
            TimberDesignHelperAmerican timberDesignHelper = TimberDesignHelperAmerican.GetInstance();
            checkData.DesignStatus = eDesignStatus.Pass;

            double thickness = Math.Min(castedInput.h1, castedInput.h2);
            double width = Math.Max(castedInput.h1, castedInput.h2);

            switch (castedInput.TimberType)
            {
                case eTimberType.SL:

                    timberDesignHelper.GetAdjustmentFactorsSL(castedInput, checkData);
                    break;

                default:

                    // Unsupported timber type
                    return null;
            }


            double adjustedShearStrength = castedInput.Fv *
                checkData.LoadDurationFactor *
                checkData.WetServiceFactor *
                checkData.TemperatureFactor *
                checkData.IncisingFactor *
                checkData.FormatConversionFactor *
                checkData.ResistanceFactor *
                checkData.TimeEffectFactor;

            double maxShearDemand = 0;

            // Configuration type dispatch
            switch (castedInput.memberConfigurationType)
            {
                case eMemberConfigurationType.SolidMembers:
                case eMemberConfigurationType.NotSpacedCombinedColumns:

                    // For a bending member with rectangular cross section of breadth, b, and depth, d.
                    maxShearDemand = (3 * castedInput.MaxShearDemand) / (2 * castedInput.h1 * castedInput.h2);
                    break;

                case eMemberConfigurationType.SpacedColumns:

                    maxShearDemand = (castedInput.MomentofArea * castedInput.MaxShearDemand) / (castedInput.Inertia * castedInput.h2);
                    break;

                default:

                    // Unknown configuration
                    checkData.DesignStatus = eDesignStatus.Fail;
                    break;
            }

            checkData.MemberShearCapacityMat = (float)adjustedShearStrength;
            checkData.MaxShearDemand = (float)maxShearDemand;

            // Capacity Check Shear
            if (maxShearDemand > adjustedShearStrength)
            {
                checkData.DesignStatus = eDesignStatus.Fail;
            }

            return checkData;
        }
    }
}
