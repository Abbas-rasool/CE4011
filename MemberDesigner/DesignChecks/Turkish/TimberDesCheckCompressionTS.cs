using MemberDesigner.DesignInputs.Turkish;
using MemberDesigner.DesignChecks.Helpers;
using MemberDesigner.TimberDesignData.BaseClasses;
using MemberDesigner.TimberDesignData.Turkish;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignChecks.Turkish
{
    public class TimberDesCheckCompressionTS : ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData>
    {

        #region CTOR

        public TimberDesCheckCompressionTS()
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
            if (!(input is TimberCompressionCheckInputTS castedInput))
                throw new ArgumentException("Input argument is not of the correct type");

            var checkData = new TimberDesCheckDataCompressionTS();
            TimberDesignHelperTS timberDesignHelper = TimberDesignHelperTS.GetInstance();
            checkData.DesignStatus = eDesignStatus.Pass;
            double appliedAngle = ConvertAngleToRadians(castedInput.AppliedAngle);

            var parameters = (TimberParametersCheckDataTS)dependencies.FirstOrDefault(x => x.CheckType == eTimberDesignCheckType.Parameters);

            float C_N = parameters.C_N;
            float C_B = parameters.C_B;
            float C_Y = parameters.C_Y;
            float omega = parameters.Omega;

            double materialStrength = (castedInput.f_c0k * C_N * C_B * C_Y) / omega;

            checkData.MaterialStrengthParallel = (float)materialStrength;

            double c = timberDesignHelper.GetMaterialFactor(castedInput.material);
            checkData.C = (float)c;

            double effectiveLengthMajor = castedInput.BucklingLengthCoe1 * castedInput.Length1;
            double effectiveLengthMinor = castedInput.BucklingLengthCoe2 * castedInput.Length2;

            checkData.BucklingLengthMajor = (float)effectiveLengthMajor;
            checkData.BucklingLengthMinor = (float)effectiveLengthMinor;

            double slendernessMajor = CalculateSlenderness(effectiveLengthMajor, castedInput.h1);
            double slendernessMinor = CalculateSlenderness(effectiveLengthMinor, castedInput.h2);

            checkData.SlendernessMajor = (float)slendernessMajor;
            checkData.SlendernessMinor = (float)slendernessMinor;

            double f_E_Major = timberDesignHelper.Calculatef_E(slendernessMajor, castedInput.E_005);
            double f_E_Minor = timberDesignHelper.Calculatef_E(slendernessMinor, castedInput.E_005);

            checkData.F_E_Major = (float)f_E_Major;
            checkData.F_E_Minor = (float)f_E_Minor;

            double C_P_Major = timberDesignHelper.CalculateC_P(f_E_Major, castedInput.f_c0k, c);
            double C_P_Minor = timberDesignHelper.CalculateC_P(f_E_Minor, castedInput.f_c0k, c);

            checkData.C_P_Major = (float)C_P_Major;
            checkData.C_P_Minor = (float)C_P_Minor;

            double majorCapacity = C_P_Major * materialStrength;
            double minorCapacity = C_P_Minor * materialStrength;

            double demandStress = castedInput.MaxCompressionDemandParallel / castedInput.SectionGrossArea;

            checkData.MajorCapacity = (float)majorCapacity;
            checkData.MinorCapacity = (float)minorCapacity;
            checkData.DemandStressParallel = (float)demandStress;

            if (majorCapacity < demandStress || minorCapacity < demandStress)
                checkData.DesignStatus = eDesignStatus.Fail;


            // Perpendicular Control -----------------------------------------------------------------------------------------------------------------------------
            // TO DO: try to update it later.
            double C_P_90 = 1;
            checkData.C_P_90 = (float)C_P_90;

            double StrengthMaterial90 = C_P_90 * ((castedInput.f_c90k * C_N * C_B * C_Y) / omega);
            double demandStress90 = castedInput.MaxCompressionDemandPerpendicular / castedInput.NetSectionAreaPerpendicular;

            checkData.MaterialStrengthPerp = (float)StrengthMaterial90;
            checkData.DemandStressPerpendicular = (float)demandStress90;

            if (StrengthMaterial90 < demandStress90)
                checkData.DesignStatus = eDesignStatus.Fail;

            // compressive stress at an angle a to the grain calculation---------------------------------------------------------------------------------------------------
            double angledStressAppliedArea = castedInput.SectionGrossArea / Math.Cos(appliedAngle);
            double angledStress = castedInput.MaxCompressionDemandAngled / angledStressAppliedArea;

            double numerator = C_P_90 * StrengthMaterial90 * materialStrength;
            double denominator = (materialStrength * Math.Pow(Math.Sin(appliedAngle), 2) + StrengthMaterial90 * Math.Pow(Math.Cos(appliedAngle), 2));
            double angledCapacity = numerator / denominator;

            checkData.AngledStressAppliedArea = (float)angledStressAppliedArea;
            checkData.AngledDemandStress = (float)angledStress;
            checkData.AngledCapacity = (float)angledCapacity;

            if (angledCapacity < angledStress)
                checkData.DesignStatus = eDesignStatus.Fail;

            return checkData;
        }

        private double ConvertAngleToRadians(double angle)
        {
            double angleInRadians = angle * Math.PI / 180.0;
            return angleInRadians;
        }

        /// <summary>
        /// Calculates the slenderness ratio (λ) for a rectangular section
        /// </summary>
        private double CalculateSlenderness(double effectiveLength, double height)
        {

            double i = height / Math.Sqrt(12); // strong axis

            // Slenderness ratios
            double lambda = effectiveLength / i;

            return lambda;
        }

    }
}
