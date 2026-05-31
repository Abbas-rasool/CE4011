using MemberDesigner.DesignInputs.Eurocode;
using MemberDesigner.TimberDesignData.BaseClasses;
using MemberDesigner.TimberDesignData.Eurocode;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignChecks.Eurocode
{
    public class TimberDesCheckTensionEU : ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData>
    {

        #region CTOR

        public TimberDesCheckTensionEU()
        {
            _dependencies = new List<eTimberDesignCheckType>() { eTimberDesignCheckType.Parameters };
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
            if (!(input is TimberTensionCheckInputEU castedInput))
                throw new ArgumentException("Input argument is not of the correct type");

            var checkData = new TimberDesCheckDataTensionEU();
            checkData.DesignStatus = eDesignStatus.Pass;

            var parameters = (TimberParametersCheckDataEU)dependencies.FirstOrDefault(x => x.CheckType == eTimberDesignCheckType.Parameters);

            float partialFactor = parameters.PartialFactor;
            float modificationFactor = parameters.ModificationFactor;

            float designMatStrengthValue = (castedInput.Ft * modificationFactor) / partialFactor;
            checkData.DesignMatStrengthValue = designMatStrengthValue;
            checkData.NetSectionArea = castedInput.NetSectionArea;

            checkData.MaxTensionForce = castedInput.MaxTensionDemand;

            float tensionDemand = castedInput.MaxTensionDemand / castedInput.NetSectionArea;
            checkData.MaxTensionDemand = tensionDemand;

            checkData.MaxTensionDemand90 = castedInput.MaxTensionDemand90;

            if (castedInput.MaxTensionDemand90 != 0)
            {
                checkData.DesignStatus = eDesignStatus.Warning;
            }

            if (tensionDemand > designMatStrengthValue)
                checkData.DesignStatus = eDesignStatus.Fail;

            return checkData;
        }
    }
}
