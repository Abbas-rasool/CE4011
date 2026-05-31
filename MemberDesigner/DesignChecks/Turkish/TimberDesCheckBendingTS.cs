using MemberDesigner.DesignInputs.Turkish;
using MemberDesigner.DesignChecks;
using MemberDesigner.DesignChecks.Helpers;
using MemberDesigner.TimberDesignData.BaseClasses;
using MemberDesigner.TimberDesignData.Turkish;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignChecks.Turkish
{
    public class TimberDesCheckBendingTS : ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData>
    {

        #region CTOR

        public TimberDesCheckBendingTS()
        {
            _dependencies = new List<eTimberDesignCheckType>() { eTimberDesignCheckType.Parameters };
        }

        #endregion

        #region Private Fields
        private List<eTimberDesignCheckType> _dependencies;
        #endregion

        #region Public Fields
        public List<eTimberDesignCheckType> Dependencies { get => _dependencies; }
        public eTimberDesignCheckType CheckType => eTimberDesignCheckType.Bending;

        #endregion

        /// <summary>
        /// This method checks the bending capacity of a member.
        /// </summary>
        public TimberDesignCheckData PerformCheck(ITimberDesignCheckInput input, params TimberDesignCheckData[] dependencies)
        {
            if (!(input is TimberBendingCheckInputTS castedInput))
                throw new ArgumentException("Input argument is not of the correct type");

            var checkData = new TimberDesCheckDataBendingTS();
            TimberDesignHelperTS timberDesignHelper = TimberDesignHelperTS.GetInstance();
            checkData.DesignStatus = eDesignStatus.Pass;

            var parameters = (TimberParametersCheckDataTS)dependencies.FirstOrDefault(x => x.CheckType == eTimberDesignCheckType.Parameters);

            float C_N = parameters.C_N;
            float C_B = parameters.C_B;
            float C_Y = parameters.C_Y;
            float omega = parameters.Omega;

            double materialStrength = (castedInput.Fm * C_N * C_B * C_Y) / omega;
            checkData.MaterialStrength = (float)materialStrength;

            // Demand stresses (rectangular section assumption)
            double MajorDemandStress = (6 * castedInput.MaxDemandMomentMajor) /
                                       (castedInput.h2 * Math.Pow(castedInput.h1, 2));

            double MinorDemandStress = (6 * castedInput.MaxDemandMomentMinor) /
                                       (castedInput.h1 * Math.Pow(castedInput.h2, 2));

            checkData.MajorDemandStress = (float)MajorDemandStress;
            checkData.MinorDemandStress = (float)MinorDemandStress;

            // (rectangular section assumption)
            double C_E = 1;
            if (castedInput.material == eTimberMaterialType.SolidTimber || castedInput.material == eTimberMaterialType.GluedLaminatedTimber)
            {
                C_E = 0.7;
            }
            checkData.C_E = (float)C_E;

            double MajorCheck = (MajorDemandStress / materialStrength) + C_E * (MinorDemandStress / materialStrength);
            double MinorCheck = C_E * (MajorDemandStress / materialStrength) + (MinorDemandStress / materialStrength);

            checkData.MajorCheckFactor = (float)MajorCheck;
            checkData.MinorCheckFactor = (float)MinorCheck;

            if (MajorCheck > 1 || MinorCheck > 1)
                checkData.DesignStatus = eDesignStatus.Fail;

            // 4.2.2-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

            // This is only applicable for rectangular sections.
            double width = Math.Max(castedInput.h1, castedInput.h2);
            double thickness = Math.Min(castedInput.h1, castedInput.h2);

            double σ_yb = timberDesignHelper.CalculateSigmaMCrit(width, thickness, castedInput.EffectiveBeamLength, castedInput.E_005);
            double λ_yb = Math.Sqrt(materialStrength / σ_yb);
            double C_yb = timberDesignHelper.CalculateKCrit(λ_yb);

            checkData.σ_yb = (float)σ_yb;
            checkData.λ_yb = (float)λ_yb;
            checkData.C_yb = (float)C_yb;

            double sectionBucklingStrength = materialStrength * C_yb;
            checkData.SectionBucklingStrength = (float)sectionBucklingStrength;

            if (MajorDemandStress > sectionBucklingStrength)
                checkData.DesignStatus = eDesignStatus.Fail;

            return checkData;
        }

    }
}
