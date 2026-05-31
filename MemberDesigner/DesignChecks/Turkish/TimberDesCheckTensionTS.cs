using MemberDesigner.DesignInputs.Turkish;
using MemberDesigner.TimberDesignData.BaseClasses;
using MemberDesigner.TimberDesignData.Turkish;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignChecks.Turkish
{
    public class TimberDesCheckTensionTS : ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData>
    {

        #region CTOR

        public TimberDesCheckTensionTS()
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
            if (!(input is TimberTensionCheckInputTS castedInput))
                throw new ArgumentException("Input argument is not of the correct type");

            var checkData = new TimberDesCheckDataTensionTS();
            checkData.DesignStatus = eDesignStatus.Pass;

            var parameters = (TimberParametersCheckDataTS)dependencies.FirstOrDefault(x => x.CheckType == eTimberDesignCheckType.Parameters);

            float C_N = parameters.C_N;
            float C_B = parameters.C_B;
            float C_Y = parameters.C_Y;
            float omega = parameters.Omega;

            float materialResistance = (castedInput.Ft * C_B * C_N * C_Y) / omega;
            float demandStress = castedInput.MaxDemandTension / castedInput.NetSectionArea;

            checkData.MaterialResistance = materialResistance;
            checkData.DemandStress = demandStress;

            if (demandStress > materialResistance)
                checkData.DesignStatus = eDesignStatus.Fail;

            return checkData;
        }
    }
}
