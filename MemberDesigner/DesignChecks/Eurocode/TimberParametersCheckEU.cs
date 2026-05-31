using MemberDesigner.DesignInputs.Eurocode;
using MemberDesigner.DesignChecks.Helpers;
using MemberDesigner.TimberDesignData.BaseClasses;
using MemberDesigner.TimberDesignData.Eurocode;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignChecks.Eurocode
{
    public class TimberParametersCheckEU : ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData>
    {

        #region CTOR

        public TimberParametersCheckEU()
        {
            _dependencies = new List<eTimberDesignCheckType>() { };
        }

        #endregion

        #region Private Fields
        private List<eTimberDesignCheckType> _dependencies;
        #endregion

        #region Public Fields
        public List<eTimberDesignCheckType> Dependencies { get => _dependencies; }
        public eTimberDesignCheckType CheckType => eTimberDesignCheckType.Parameters;

        #endregion

        public TimberDesignCheckData PerformCheck(ITimberDesignCheckInput input, params TimberDesignCheckData[] dependencies)
        {
            if (!(input is TimberParametersCheckInputEU castedInput))
                throw new ArgumentException("Input argument is not of the correct type");

            var checkData = new TimberParametersCheckDataEU();
            TimberDesignHelperEU timberDesignHelper = TimberDesignHelperEU.GetInstance();
            checkData.DesignStatus = eDesignStatus.Pass;

            float partialFactor;
            float modificationFactor;

            if (castedInput.IsFactorsModified)
            {
                partialFactor = castedInput.PartialFactor;
                modificationFactor = castedInput.ModificationFactor;
            }
            else
            {
                partialFactor = timberDesignHelper.GetPartialFactor(castedInput.material);
                modificationFactor = timberDesignHelper.GetK_ModFactor(castedInput.material, castedInput.serviceClass, castedInput.loadDurationClass);
            }


            checkData.PartialFactor = partialFactor;
            checkData.ModificationFactor = modificationFactor;

            return checkData;
        }
    }
}
