using MemberDesigner.DesignInputs.Eurocode;
using MemberDesigner.TimberDesignData.BaseClasses;
using MemberDesigner.TimberDesignData.Eurocode;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignChecks.Eurocode
{
    public class TimberDesCheckCompressionEU : ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData>
    {

        #region CTOR

        public TimberDesCheckCompressionEU()
        {
            _dependencies = new List<eTimberDesignCheckType>() { eTimberDesignCheckType.Parameters };
        }

        #endregion

        #region Private Fields
        private List<eTimberDesignCheckType> _dependencies;
        #endregion

        #region Public Fields
        public List<eTimberDesignCheckType> Dependencies { get => _dependencies; }
        public eTimberDesignCheckType CheckType => eTimberDesignCheckType.Compression;

        #endregion

        /// <summary>
        /// This method checks the compression capacity of the member.
        /// </summary>
        public TimberDesignCheckData PerformCheck(ITimberDesignCheckInput input, params TimberDesignCheckData[] dependencies)
        {
            if (!(input is TimberCompressionCheckInputEU castedInput))
                throw new ArgumentException("Input argument is not of the correct type");

            var checkData = new TimberDesCheckDataCompressionEU();
            checkData.DesignStatus = eDesignStatus.Pass;

            var parameters = (TimberParametersCheckDataEU)dependencies.FirstOrDefault(x => x.CheckType == eTimberDesignCheckType.Parameters);

            float partialFactor = parameters.PartialFactor;
            float modificationFactor = parameters.ModificationFactor;

            float designStrengthValueMatParallel = (castedInput.Fc * modificationFactor) / partialFactor;

            // K_c90 for a conservative approach is assumed to be 1, maybe refine it later if necessary!
            float k_c90 = 1f;
            float designStrengthValueMatPerpend = k_c90 * (castedInput.Fc90 * modificationFactor) / partialFactor;

            // compressive stress at an angle a to the grain calculation (Check the input to be in correct form!)
            float angledStressAppliedArea = (float)(castedInput.NetSectionArea / Math.Cos(castedInput.AppliedAngle));

            double term1 = (designStrengthValueMatParallel / (k_c90 * designStrengthValueMatPerpend)) * Math.Pow(Math.Sin(castedInput.AppliedAngle), 2);
            double term2 = Math.Pow(Math.Cos(castedInput.AppliedAngle), 2);
            double designStrengthMatAngled = designStrengthValueMatParallel / (term1 + term2);

            float compressionDemandParallel = castedInput.MaxCompressionDemandParallel / castedInput.NetSectionArea;
            float compressionDemandPerpendicular = castedInput.MaxCompressionDemandPerpendicular / castedInput.EffectiveArea90;
            float compressionDemandAppliedAngle = castedInput.MaxCompressionAppliedAngled / angledStressAppliedArea;

            // Direction-specific compression checks (even though the angled check includes them implicitly)
            bool failsParallel = compressionDemandParallel > designStrengthValueMatParallel;
            bool failsPerpendicular = compressionDemandPerpendicular > designStrengthValueMatPerpend;
            bool failsAngled = compressionDemandAppliedAngle > designStrengthMatAngled;

            checkData.K_C90 = (float)k_c90;

            checkData.DesignStrengthMatParallel = designStrengthValueMatParallel;
            checkData.DesignStrengthMatPerpendicular = designStrengthValueMatPerpend;
            checkData.DesignStrengthMatAngled = (float)designStrengthMatAngled;

            checkData.MaxDemandParallel = compressionDemandParallel;
            checkData.MaxDemandPerpendicular = compressionDemandPerpendicular;
            checkData.MaxDemandAngled = compressionDemandAppliedAngle;

            checkData.AppliedAngleRad = castedInput.AppliedAngle;
            checkData.NetSectionArea = castedInput.NetSectionArea;
            checkData.NetSectionArea90 = castedInput.EffectiveArea90;
            checkData.NetSectionAreaAngled = angledStressAppliedArea;


            if (failsParallel || failsPerpendicular || failsAngled)
                checkData.DesignStatus = eDesignStatus.Fail;


            return checkData;
        }
    }
}
