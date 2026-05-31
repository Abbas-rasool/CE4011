using MemberDesigner.DesignInputs.Turkish;
using MemberDesigner.DesignChecks.Helpers;
using MemberDesigner.TimberDesignData.BaseClasses;
using MemberDesigner.TimberDesignData.Turkish;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignChecks.Turkish
{
    public class TimberParametersCheckTS: ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData>
    {

        #region CTOR

        public TimberParametersCheckTS()
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
            if (!(input is TimberParametersCheckInputTS castedInput))
                throw new ArgumentException("Input argument is not of the correct type");

            var checkData = new TimberParametersCheckDataTS();
            TimberDesignHelperTS timberDesignHelper = TimberDesignHelperTS.GetInstance();
            checkData.DesignStatus = eDesignStatus.Pass;

            float C_N;
            float C_B;
            float C_Y;
            float omega;
            float s = 0.15f;

            if (castedInput.IsFactorsModified)
            {

                C_N = castedInput.C_N;
                C_B = castedInput.C_B;
                C_Y = castedInput.C_Y;
                omega = castedInput.Omega;

            }
            else
            {
                double width = Math.Min(castedInput.h1, castedInput.h2);

                C_N = timberDesignHelper.GetC_N(castedInput.serviceClass);
                C_B = (float)timberDesignHelper.CalculateSizeFactor(castedInput.material, width, castedInput.CharacteristicDensity, castedInput.SectionLength, s, false);
                C_Y = timberDesignHelper.GetC_Y(castedInput.material, castedInput.serviceClass, castedInput.loadDurationClass);
                omega = timberDesignHelper.GetPartialFactor(castedInput.material);

            }

            checkData.S = s;
            checkData.C_N = C_N;
            checkData.C_B = C_B;
            checkData.C_Y = C_Y;
            checkData.Omega = omega;

            return checkData;
        }
    }
}
