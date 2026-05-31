using MemberDesigner.DesignInputs.Eurocode;
using MemberDesigner.DesignChecks.Helpers;
using MemberDesigner.TimberDesignData.BaseClasses;
using MemberDesigner.TimberDesignData.Eurocode;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignChecks.Eurocode
{
    public class TimberDesCheckCombinedEU : ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData>
    {

        #region CTOR

        public TimberDesCheckCombinedEU()
        {
            _dependencies = new List<eTimberDesignCheckType>
        {
            eTimberDesignCheckType.Bending,
            eTimberDesignCheckType.Tension,
            eTimberDesignCheckType.Compression
        };
        }

        #endregion

        #region Private Fields
        private List<eTimberDesignCheckType> _dependencies;
        #endregion

        #region Public Fields
        public List<eTimberDesignCheckType> Dependencies { get => _dependencies; }
        public eTimberDesignCheckType CheckType => eTimberDesignCheckType.CombinedBendingAxial;

        #endregion

        /// <summary>
        /// This method checks the combined axial and bending capacity of a member.
        /// </summary>
        public TimberDesignCheckData PerformCheck(ITimberDesignCheckInput input, params TimberDesignCheckData[] dependencies)
        {
            if (!(input is TimberCombinedCheckInputEU castedInput))
                throw new ArgumentException("Input argument is not of the correct type");

            for (int i = 0; i < _dependencies.Count; i++)
            {
                if (!(dependencies.Any(x => x.CheckType == _dependencies[i])))
                    throw new ArgumentException("Dependency not provided");
            }

            TimberDesignHelperEU timberDesignHelper = TimberDesignHelperEU.GetInstance();

            var bendingCheckData = (TimberDesCheckDataBendingEU)dependencies.FirstOrDefault(x => x.CheckType == eTimberDesignCheckType.Bending);
            var tensionCheckData = (TimberDesCheckDataTensionEU)dependencies.FirstOrDefault(x => x.CheckType == eTimberDesignCheckType.Tension);
            var compressionCheckData = (TimberDesCheckDataCompressionEU)dependencies.FirstOrDefault(x => x.CheckType == eTimberDesignCheckType.Compression);

            var checkData = new TimberDesCheckDataCombinedEU();
            checkData.DesignStatus = eDesignStatus.Pass;

            double width = Math.Max(castedInput.h1, castedInput.h2);
            double thickness = Math.Min(castedInput.h1, castedInput.h2);

            checkData.KmInteractionFactor = bendingCheckData.KmInteractionFactor;

            // combined tension + bending check --------------------------------------------------------------------


            double tensionRatio = tensionCheckData.MaxTensionDemand / tensionCheckData.DesignMatStrengthValue;
            checkData.TensionStressRatio = (float)tensionRatio;


            double majorMomentRatio = bendingCheckData.MajorDemandStress / bendingCheckData.DesignMatStrength;
            double minorMomentRatio = bendingCheckData.MinorDemandStress / bendingCheckData.DesignMatStrength;

            checkData.MomentStressRatioMajor = (float)majorMomentRatio;
            checkData.MomentStressRatioMinor = (float   )minorMomentRatio;

            double capacityCheckTension1 = tensionRatio + majorMomentRatio + minorMomentRatio * checkData.KmInteractionFactor;
            double capacityCheckTension2 = tensionRatio + majorMomentRatio * checkData.KmInteractionFactor + minorMomentRatio;

            checkData.TensionCapacityLimitMajor = (float)capacityCheckTension1;
            checkData.TensionCapacityLimitMinor = (float)capacityCheckTension2;

            if (capacityCheckTension1 > 1 || capacityCheckTension2 > 1)
                checkData.DesignStatus = eDesignStatus.Fail;


            // combined compression + bending check --------------------------------------------------------------------------------------------------

            double compressionRatio = compressionCheckData.MaxDemandParallel / compressionCheckData.DesignStrengthMatParallel;
            checkData.CompressionStressRatio = (float)compressionRatio;

            double capacityCheckCompression1 = Math.Pow(compressionRatio, 2) + majorMomentRatio + minorMomentRatio * bendingCheckData.KmInteractionFactor;
            double capacityCheckCompression2 = Math.Pow(compressionRatio, 2) + majorMomentRatio * bendingCheckData.KmInteractionFactor + minorMomentRatio;

            checkData.CompressionCapacityLimitMajor = (float)capacityCheckCompression1;
            checkData.CompressionCapacityLimitMinor = (float)capacityCheckCompression2;

            if (capacityCheckCompression1 > 1 || capacityCheckCompression2 > 1)
                checkData.DesignStatus = eDesignStatus.Fail;


            // Stability Checks---------------------------------------------------------------------------------------------------------------------------

            // Compression/ Compression + Bending.
            (double Kc_Major, double Kc_Minor) = CalculateK_Factors(castedInput, checkData, timberDesignHelper);

            if (checkData.IsExtraCheckNeeded)
            {
                double capacityCheckCompMajor = compressionRatio / Kc_Major + majorMomentRatio + minorMomentRatio * bendingCheckData.KmInteractionFactor;
                double capacityCheckCompMinor = compressionRatio / Kc_Minor + majorMomentRatio * bendingCheckData.KmInteractionFactor + minorMomentRatio;

                checkData.CapacityCheckLimitCompMajor = (float)capacityCheckCompMajor;
                checkData.CapacityCheckLimitCompMinor = (float)capacityCheckCompMinor;

                if (capacityCheckCompMajor > 1 || capacityCheckCompMinor > 1)
                    checkData.DesignStatus = eDesignStatus.Fail;
            }


            // Bending/ Bending + Compression.............................................................................................................................

            // This is only applicable for rectangular sections.
            double σ_m_crit = timberDesignHelper.CalculateSigmaMCrit(width, thickness, castedInput.EffectiveBeamLength, castedInput.E_005);
            checkData.σ_m_crit = (float)σ_m_crit;

            double sigmaRelMoment = Math.Sqrt(castedInput.Fm / σ_m_crit);
            checkData.SigmaRelMoment = (float)sigmaRelMoment;

            // Note (5): The factor may be taken as 1.0 for a beam where lateral displacement of it's compressive edge is prevented throughout it's length
            // and where torsional rotation is prevented at its supports.
            // The above note was disregarded, aiming for a conservative design.

            double K_crit = timberDesignHelper.CalculateKCrit(sigmaRelMoment);
            checkData.K_crit = (float)K_crit;

            // The critical of k(c,y) and k(c,z)- 6.25 and 6.26 (Minimum of them)
            double governingK_c = Math.Min(Kc_Major, Kc_Minor);

            double criticalMomentRatio = Math.Max(majorMomentRatio, minorMomentRatio);

            double term1 = criticalMomentRatio / K_crit;
            double term2 = compressionRatio / governingK_c;

            double momentRatioToCheck = Math.Pow(term1, 2) + term2;
            checkData.CapacityCheckLimitMoment = (float)momentRatioToCheck;

            if (momentRatioToCheck > 1)
                checkData.DesignStatus = eDesignStatus.Fail;

            double momentMajorAxisLimit = K_crit * bendingCheckData.DesignMatStrength;
            checkData.MomentMajorAxisLimit = (float)momentMajorAxisLimit;

            // Bending only major axis
            if (bendingCheckData.MajorDemandStress > momentMajorAxisLimit)
                checkData.DesignStatus = eDesignStatus.Fail;

            return checkData;
        }


        #region Private Methods

        /// <summary>
        /// Method to calculate k(c,y) and k(c,z)- 6.25 and 6.26
        /// </summary>
        private (double Kc_Major, double Kc_Minor) CalculateK_Factors(
            TimberCombinedCheckInputEU castedInput,
            TimberDesCheckDataCombinedEU checkData,
            TimberDesignHelperEU timberDesignHelper
)
        {
            double Kc_Major = 1;
            double Kc_Minor = 1;

            double IMajor = castedInput.h2 * Math.Pow(castedInput.h1, 3) / 12.0;
            double IMinor = castedInput.h1 * Math.Pow(castedInput.h2, 3) / 12.0;
            checkData.IMajor = (float)IMajor;
            checkData.IMinor = (float)IMinor;

            double sectionArea = castedInput.h1 * castedInput.h2;

            double RMajor = Math.Sqrt(IMajor / sectionArea);
            double RMinor = Math.Sqrt(IMinor / sectionArea);
            checkData.GyrationRadiusMajor = (float)RMajor;
            checkData.GyrationRadiusMinor = (float)RMinor;

            double slendernessRatioMajor = castedInput.MajorEffectiveLength / RMajor;
            double slendernessRatioMinor = castedInput.MinorEffectiveLength / RMinor;
            checkData.SlendernessRatioMajor = (float)slendernessRatioMajor;
            checkData.SlendernessRatioMinor = (float)slendernessRatioMinor;

            double sigmaRelMajor = (slendernessRatioMajor / Math.PI) * Math.Sqrt(castedInput.Fc / castedInput.E_005);
            double sigmaRelMinor = (slendernessRatioMinor / Math.PI) * Math.Sqrt(castedInput.Fc / castedInput.E_005);
            checkData.SigmaRelMajor = (float)sigmaRelMajor;
            checkData.SigmaRelMinor = (float)sigmaRelMinor;

            if (sigmaRelMajor > 0.3 || sigmaRelMinor > 0.3)
            {
                checkData.IsExtraCheckNeeded = true;

                double Beta_C = timberDesignHelper.GetBetaC(castedInput.material);
                checkData.Beta_C = (float)Beta_C;

                double K_Major = 0.5 * (1 + Beta_C * (sigmaRelMajor - 0.3) + Math.Pow(sigmaRelMajor, 2));
                double K_Minor = 0.5 * (1 + Beta_C * (sigmaRelMinor - 0.3) + Math.Pow(sigmaRelMinor, 2));

                checkData.K_Major = (float)K_Major;
                checkData.K_Minor = (float)K_Minor;

                Kc_Major = 1 / (K_Major + Math.Sqrt(Math.Pow(K_Major, 2) - Math.Pow(sigmaRelMajor, 2)));
                Kc_Minor = 1 / (K_Minor + Math.Sqrt(Math.Pow(K_Minor, 2) - Math.Pow(sigmaRelMinor, 2)));
            }

            checkData.Kc_Major = (float)Kc_Major;
            checkData.Kc_Minor = (float)Kc_Minor;

            return (Kc_Major, Kc_Minor);

        }

        /// <summary>
        /// Saint-Venant torsion constant J for a rectangle (width × thickness).
        /// Assumes width >= thickness (order doesn’t matter; code enforces it).
        /// Uses the approximation:
        /// J ≈ a*b^3 * [16/3 − 3.36*(b/a)*(1 − (b^4)/(12*a^4))],
        /// where a = width/2, b = thickness/2, and a ≥ b.
        /// Returns units of length^4 if inputs are length.
        /// </summary>
        //private double CalculateTorsionConstant(double width, double thickness)
        //{
        //    if (width <= 0 || thickness <= 0) throw new ArgumentOutOfRangeException();

        //    // Ensure a is the larger half-dimension, b the smaller
        //    double a = Math.Max(width, thickness) / 2.0;
        //    double b = Math.Min(width, thickness) / 2.0;

        //    double ratio = b / a;
        //    double bracket = (16.0 / 3.0) - 3.36 * ratio * (1.0 - Math.Pow(ratio, 4) / 12.0);

        //    return a * Math.Pow(b, 3) * bracket;
        //}

        #endregion
    }
}
