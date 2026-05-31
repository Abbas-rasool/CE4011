using MemberDesigner.DesignInputs.American;
using MemberDesigner.DesignChecks.Helpers;
using MemberDesigner.TimberDesignData.American;
using MemberDesigner.TimberDesignData.BaseClasses;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignChecks.American
{
    public class TimberDesCheckBendingUS : ITimberDesignCheck<ITimberDesignCheckInput, TimberDesCheckDataBendingUS>
    {
        #region CTOR

        public TimberDesCheckBendingUS()
        {
            _dependencies = new List<eTimberDesignCheckType>() { };
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
        /// This method checks the tension capacity of the member.
        /// </summary>
        public TimberDesCheckDataBendingUS PerformCheck(ITimberDesignCheckInput input, params TimberDesignCheckData[] dependencies)
        {
            if (!(input is TimberBendingCheckInputUS castedInput))
                throw new ArgumentException("Input argument is not of the correct type");

            var checkData = new TimberDesCheckDataBendingUS();
            TimberDesignHelperAmerican timberDesignHelper = TimberDesignHelperAmerican.GetInstance();
            checkData.DesignStatus = eDesignStatus.Pass;

            double thickness = Math.Min(castedInput.h1, castedInput.h2);
            double width = Math.Max(castedInput.h1, castedInput.h2);

            double reputativeMemberFactor = 1;
            double flatUseFactor = 1;
            double flatUseFactorEmin = 1;

            double thicknessLimit = 4 * 25.4; // Convert from inches to mm
            double studSpacingLimit = 24 * 25.4; // Convert from inches to mm
            double bendingCapacityLimit = 1150 * 0.00689476; // Convert from psi to N/mm2

            switch (castedInput.TimberType)
            {
                case eTimberType.SL:

                    timberDesignHelper.GetAdjustmentFactorsSL(castedInput, checkData);
                    if (castedInput.IsRepetitiveMember == true && thickness <= thicknessLimit && castedInput.StudSpacing < studSpacingLimit)
                    {
                        reputativeMemberFactor = 1.15;
                    }
                    checkData.RepetitiveMemberFactor = (float)reputativeMemberFactor;

                    if (castedInput.Fb * reputativeMemberFactor <= bendingCapacityLimit)
                    {
                        checkData.WetServiceFactor = 1.0f;
                    }

                    flatUseFactor = timberDesignHelper.GetFlatUseFactor(width, thickness, castedInput.TimberGrade, castedInput.designParameter);
                    checkData.FlatUseFactor = (float)flatUseFactor;

                    flatUseFactorEmin = timberDesignHelper.GetFlatUseFactor(width, thickness, castedInput.TimberGrade, eDesignParameter.Emin);
                    checkData.FlatUseFactorEmin = (float)flatUseFactorEmin;

                    break;

                default:

                    // Unsupported timber type
                    return null;

            }

            // Configuration type dispatch
            switch (castedInput.memberConfigurationType)
            {
                case eMemberConfigurationType.SolidMembers:
                case eMemberConfigurationType.NotSpacedCombinedColumns:

                    PerformTimberBendingCheckSolidMember(castedInput, checkData);

                    break;

                case eMemberConfigurationType.SpacedColumns:

                    ControlSlendernessSpacedColumns(castedInput, checkData);
                    PerformTimberBendingCheckSpacedColumns(castedInput, checkData);

                    break;

                default:

                    // Unknown configuration
                    checkData.DesignStatus = eDesignStatus.Fail;
                    break;
            }

            if (checkData.SlendernessPassed == false)
            {
                return checkData;
            }
            // Capacity Check Major/Minor
            else if (checkData.MajorDemandStress > checkData.BendingDesignValueMajorAxis || checkData.MinorDemandStress > checkData.BendingDesignValueMinorAxis)
            {
                checkData.DesignStatus = eDesignStatus.Fail;
            }

            return checkData;
        }

        #region PrivateMethods

        /// <summary>
        /// This method performs the necessary calculations for solid members bending capacity controls.
        /// </summary>
        private void PerformTimberBendingCheckSolidMember(TimberBendingCheckInputUS castedInput, TimberDesCheckDataBendingUS checkData)
        {

            double thickness = Math.Min(castedInput.h1, castedInput.h2);
            double width = Math.Max(castedInput.h1, castedInput.h2);

            // Adjusted bending stress F_b**
            double adjustedF_b = castedInput.Fb *
                                 checkData.LoadDurationFactor *
                                 checkData.WetServiceFactor *
                                 checkData.TemperatureFactor *
                                 checkData.SizeFactor *
                                 checkData.IncisingFactor *
                                 checkData.RepetitiveMemberFactor *
                                 checkData.FormatConversionFactor *
                                 checkData.ResistanceFactor *
                                 checkData.TimeEffectFactor;

            // Slenderness ratios
            double slendernessRatioMajor = Math.Sqrt((castedInput.EffectiveLengthMajor * castedInput.h1) /
                                                     Math.Pow(castedInput.h2, 2));
            checkData.SlendernessRatioMajor = (float)slendernessRatioMajor;

            double slendernessRatioMinor = Math.Sqrt((castedInput.EffectiveLengthMinor * castedInput.h2) /
                                                     Math.Pow(castedInput.h1, 2));
            checkData.SlendernessRatioMinor = (float)slendernessRatioMinor;

            double criticalRatio = Math.Max(slendernessRatioMajor, slendernessRatioMinor);

            // Adjusted modulus of elasticity (major includes flat use factor)
            double adjustedMinModulus = castedInput.Emin *
                                             checkData.WetServiceFactor *
                                             checkData.TemperatureFactor *
                                             checkData.IncisingFactor *
                                             checkData.FlatUseFactorEmin *
                                             checkData.FormatConversionFactorEmin *
                                             checkData.ResistanceFactorEmin;

            checkData.AdjustedMinModulus = (float)adjustedMinModulus;

            // F_BE calculations (note: swapped intentionally)
            double F_BE_Major = CalculateF_BE(adjustedMinModulus, slendernessRatioMajor);
            double F_BE_Minor = CalculateF_BE(adjustedMinModulus, slendernessRatioMinor);

            checkData.F_BE_Major = (float)F_BE_Major;
            checkData.F_BE_Minor = (float)F_BE_Minor;

            // Slenderness check and C_L factors
            double C_LMajor, C_LMinor;

            if (width < thickness || castedInput.IsLaterallySupported)
            {
                C_LMajor = 1;
                C_LMinor = 1;
            }
            else
            {
                if (criticalRatio > 50)
                {
                    checkData.DesignStatus = eDesignStatus.Fail;
                    checkData.DesignArguments.Add(eTimberMemberDesignArgument.ReduceSlendernessRatio);
                    checkData.SlendernessPassed = false;
                    return;
                }

                C_LMajor = CalculateCL(F_BE_Major, adjustedF_b);
                C_LMinor = CalculateCL(F_BE_Minor, adjustedF_b);
            }

            checkData.C_L_Major = (float)C_LMajor;
            checkData.C_L_Minor = (float)C_LMinor;

            // Save intermediate adjusted F_b excluding C_L
            checkData.BendingDesignValueMajorAxis_ExclCL = (float)(adjustedF_b * checkData.FlatUseFactor);
            checkData.BendingDesignValueMinorAxis_ExclCL = (float)(adjustedF_b);

            // Final adjusted bending design values
            double adjustedF_bMajor = checkData.BendingDesignValueMajorAxis_ExclCL * C_LMajor;
            double adjustedF_bMinor = adjustedF_b * C_LMinor;

            checkData.BendingDesignValueMajorAxis = (float)adjustedF_bMajor;
            checkData.BendingDesignValueMinorAxis = (float)adjustedF_bMinor;
            checkData.BendingDesignValueMajorAxis_ExclCV = (float)adjustedF_bMajor;
            checkData.BendingDesignValueMinorAxis_ExclCV = (float)adjustedF_bMinor;

            // Demand stresses (rectangular section assumption)
            double MajorDemandStress = (6 * castedInput.MaxDemandMomentMajor) /
                                       (castedInput.h2 * Math.Pow(castedInput.h1, 2));

            double MinorDemandStress = (6 * castedInput.MaxDemandMomentMinor) /
                                       (castedInput.h1 * Math.Pow(castedInput.h2, 2));

            checkData.MajorDemandStress = (float)MajorDemandStress;
            checkData.MinorDemandStress = (float)MinorDemandStress;
        }

        /// <summary>
        /// This method performs the necessary calculations for Spaced Columns bending capacity controls.
        /// </summary>
        private void PerformTimberBendingCheckSpacedColumns(TimberBendingCheckInputUS castedInput, TimberDesCheckDataBendingUS checkData)
        {
            // Adjusted bending stress F_b**
            double adjustedF_b = castedInput.Fb *
                                 checkData.LoadDurationFactor *
                                 checkData.WetServiceFactor *
                                 checkData.TemperatureFactor *
                                 checkData.SizeFactor *
                                 checkData.IncisingFactor *
                                 checkData.RepetitiveMemberFactor *
                                 checkData.FormatConversionFactor *
                                 checkData.ResistanceFactor *
                                 checkData.TimeEffectFactor;

            // Adjusted modulus of elasticity (major includes flat use factor)
            double adjustedMinModulus = castedInput.Emin *
                                 checkData.WetServiceFactor *
                                 checkData.TemperatureFactor *
                                 checkData.IncisingFactor *
                                 checkData.FlatUseFactorEmin *
                                 checkData.FormatConversionFactorEmin *
                                 checkData.ResistanceFactorEmin;

            checkData.AdjustedMinModulus = (float)adjustedMinModulus;


            // F_BE calculations (note: swapped intentionally)
            double F_BE_Major = CalculateF_BE(adjustedMinModulus, checkData.SlendernessRatioMajor);
            double F_BE_Minor = CalculateF_BE(adjustedMinModulus, checkData.SlendernessRatioMinor);

            checkData.F_BE_Major = (float)F_BE_Major;
            checkData.F_BE_Minor = (float)F_BE_Minor;

            // Slenderness check and C_L factors
            double C_LMajor, C_LMinor;

            if (checkData.SlendernessPassed == true || castedInput.IsLaterallySupported)
            {
                C_LMajor = 1;
                C_LMinor = 1;
            }
            else
            {

                C_LMajor = CalculateCL(F_BE_Major, adjustedF_b);
                C_LMinor = CalculateCL(F_BE_Minor, adjustedF_b);
            }

            checkData.C_L_Major = (float)C_LMajor;
            checkData.C_L_Minor = (float)C_LMinor;

            // Save intermediate adjusted F_b excluding C_L
            checkData.BendingDesignValueMajorAxis_ExclCL = (float)(adjustedF_b * checkData.FlatUseFactor);
            checkData.BendingDesignValueMinorAxis_ExclCL = (float)(adjustedF_b);

            // Final adjusted bending design values
            double adjustedF_bMajor = checkData.BendingDesignValueMajorAxis_ExclCL * C_LMajor;
            double adjustedF_bMinor = adjustedF_b * C_LMinor;

            checkData.BendingDesignValueMajorAxis = (float)adjustedF_bMajor;
            checkData.BendingDesignValueMinorAxis = (float)adjustedF_bMinor;
            checkData.BendingDesignValueMajorAxis_ExclCV = (float)adjustedF_bMajor;
            checkData.BendingDesignValueMinorAxis_ExclCV = (float)adjustedF_bMinor;

            // Demand stresses (rectangular section assumption)
            double MajorDemandStress = castedInput.MaxDemandMomentMajor / castedInput.SectionModulusMajor;
            double MinorDemandStress = castedInput.MaxDemandMomentMinor / castedInput.SectionModulusMinor;

            checkData.MajorDemandStress = (float)MajorDemandStress;
            checkData.MinorDemandStress = (float)MinorDemandStress;
        }

        /// <summary>
        /// Checks the slenderness ratio for spaced-columns.
        /// </summary>
        private void ControlSlendernessSpacedColumns(TimberBendingCheckInputUS castedInput, TimberDesCheckDataBendingUS checkData)
        {
            double firstLimit = castedInput.EffectiveLengthMinor / 20;
            double secondLimit = castedInput.EffectiveLengthMinor / 10;

            double K_c = 1;
            if (castedInput.EndDistance <= firstLimit)
            {
                // condition "a"
                K_c = 2.5;
            }
            else if (castedInput.EndDistance > firstLimit && castedInput.EndDistance <= secondLimit)
            {
                // condition "b"
                K_c = 3.0;
            }
            else
            {
                checkData.DesignStatus = eDesignStatus.Fail;
                checkData.DesignArguments.Add(eTimberMemberDesignArgument.GeometricConstraintsShouldBeRechecked);
                checkData.SlendernessPassed = false;
                return;
            }

            double slendernessRatioMinor = (K_c * castedInput.EffectiveLengthMinor) / castedInput.h2;
            checkData.SlendernessRatioMinor = (float)slendernessRatioMinor;

            double slendernessRatioMajor = (castedInput.BucklingLengthCoe * castedInput.EffectiveLengthMajor) / castedInput.h1;
            checkData.SlendernessRatioMajor = (float)slendernessRatioMajor;

            if (slendernessRatioMinor > 80 || slendernessRatioMajor > 50)
            {
                checkData.DesignStatus = eDesignStatus.Fail;
                checkData.DesignArguments.Add(eTimberMemberDesignArgument.ReduceSlendernessRatio);
                checkData.SlendernessPassed = false;
            }
        }

        /// <summary>
        /// calculates F_BE, a parameter used for C_L calculations.
        /// </summary>
        private double CalculateF_BE(double E_min, double slendernessRatio)
        {
            double Emin = E_min * 145.038; // Convert from N/mm2 to psi

            double F_BE = (1.20 * Emin) / (Math.Pow(slendernessRatio, 2));

            double Fbe = F_BE / 145.038; // Convert from psi to N/mm2

            return Fbe;
        }

        /// <summary>
        /// Calculates the beam stability factor based on 3.3-6. (C_L)
        /// </summary>
        private double CalculateCL(double F_bE, double F_b)
        {
            if (F_b == 0)
            {
                throw new ArgumentException("F_b cannot be zero, as it would lead to division by zero.");
            }

            double ratio_F_bE_F_b = F_bE / F_b;

            // Term 1: (1 + (F_bE / F_b)) / 1.9
            double term1 = 1 + ratio_F_bE_F_b;
            term1 = term1 / 1.9;

            double term2 = ratio_F_bE_F_b / 0.95;

            double term_inside_sqrt = Math.Pow(term1, 2) - term2;

            if (term_inside_sqrt < 0)
            {
                throw new InvalidOperationException(
                    $"The term inside the square root is negative ({term_inside_sqrt}). " +
                    "This would result in a complex number. Please check your inputs."
                );
            }

            // Calculate CL
            double CL = term1 - Math.Sqrt(term_inside_sqrt);

            return CL;
        }

        #endregion


    }

}
