using MemberDesigner.DesignInputs.American;
using MemberDesigner.DesignChecks.Helpers;
using MemberDesigner.TimberDesignData.American;
using MemberDesigner.TimberDesignData.BaseClasses;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignChecks.American
{
    public class TimberDesCheckCompressionUS : ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData>
    {
        #region CTOR

        public TimberDesCheckCompressionUS()
        {
            _dependencies = new List<eTimberDesignCheckType>(){};
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
            if (!(input is TimberCompressionCheckInputUS castedInput))
                throw new ArgumentException("Input argument is not of the correct type");

            var checkData = new TimberDesCheckDataCompressionUS();
            var helper = TimberDesignHelperAmerican.GetInstance();
            checkData.DesignStatus = eDesignStatus.Pass;

            double thickness = Math.Min(castedInput.h1, castedInput.h2);
            double width = Math.Max(castedInput.h1, castedInput.h2);
            double flatUseFactorEmin = 1;

            // Material type dispatch
            switch (castedInput.TimberType)
            {
                case eTimberType.SL:

                    flatUseFactorEmin = helper.GetFlatUseFactor(width, thickness, castedInput.TimberGrade, eDesignParameter.Emin);
                    checkData.FlatUseFactorEmin = (float)flatUseFactorEmin;

                    helper.GetAdjustmentFactorsSL(castedInput, checkData);
                    double reputativeMemberFactor = 1;

                    checkData.FormatConversionFactorF90 = 1.67f;
                    checkData.ResistanceFactor90 = 0.9f;

                    if (castedInput.IsRepetitiveMember == true && thickness <= 4)
                        reputativeMemberFactor = 1.15;

                    checkData.RepetitiveMemberFactor = (float)reputativeMemberFactor;

                    if (castedInput.Fc * reputativeMemberFactor <= 5.17f)
                    {
                        checkData.WetServiceFactor = 1.0f;
                    }

                    checkData.MaterialAdjustmentFactor = (float)GetMaterialAdjustmentFactor(castedInput.TimberType);

                    // Fc90 perpendicular
                    double bearingAreaFactor = CalculateBearingAreaFactor(castedInput.BearingLength);
                    checkData.BearingAreaFactor = (float)bearingAreaFactor;

                    checkData.EffectiveColumnLength = Math.Min(
                    castedInput.BucklingLengthCoe1 * castedInput.Length1,
                    castedInput.BucklingLengthCoe2 * castedInput.Length2);

                    checkData.AdjustedEmin = castedInput.Emin
                        * checkData.FlatUseFactorEmin
                        * checkData.WetServiceFactor
                        * checkData.TemperatureFactor
                        * checkData.IncisingFactor
                        * checkData.FormatConversionFactorEmin
                        * checkData.ResistanceFactorEmin;

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

                    ControlSlendernessSolidMembers(castedInput, checkData);
                    ComputeCompressionCapacities(castedInput, checkData);

                    break;

                case eMemberConfigurationType.SpacedColumns:
                    
                    ControlSlendernessSpacedColumns(castedInput, checkData);
                    ComputeCompressionCapacities(castedInput, checkData);

                    break;

                default:

                    // Unknown configuration
                    checkData.DesignStatus = eDesignStatus.Fail;
                    break;
            }

            // Demands
            double parallelDemandGross = castedInput.MaxCompressionDemandParallel / castedInput.GrossSectionArea;
            double parallelDemandNet = castedInput.MaxCompressionDemandParallel / castedInput.NetSectionAreaParallel;
            double maxPerpendicularDemand = castedInput.MaxCompressionDemandPerpendicular / castedInput.NetSectionAreaPerpendicular;

            checkData.ParallelDemandGross = (float)parallelDemandGross;
            checkData.ParallelDemandNet = (float)parallelDemandNet;
            checkData.MaxPerpendicularDemand = (float)maxPerpendicularDemand;

            // Capacity Check - Parallel
            if (parallelDemandGross > checkData.GrossCompressionCapacity || parallelDemandNet > checkData.NetCompressionCapacity)
            {
                checkData.DesignStatus = eDesignStatus.Fail;
            }

            // Capacity Check - Perpendicular
            if (maxPerpendicularDemand > checkData.NetCompressionCapacityPerpendicular)
            {
                checkData.DesignStatus = eDesignStatus.Fail;
            }

            // Warning only if still "Pass"
            if (checkData.DesignStatus == eDesignStatus.Pass &&
                maxPerpendicularDemand > 0.75 * checkData.NetCompressionCapacity)
            {
                checkData.DesignStatus = eDesignStatus.Warning;
            }

            return checkData;
        }


        #region Private Methods

        /// <summary>
        /// Checks the slenderness ratio for spaced-columns.
        /// </summary>
        private void ControlSlendernessSpacedColumns(TimberCompressionCheckInputUS castedInput, TimberDesCheckDataCompressionUS checkData)
        {
            double firstLimit = castedInput.Length1 / 20;
            double secondLimit = castedInput.Length1 / 10;

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
                checkData.SlendernessPassed = false;
                return;
            }

            double slendernessRatio1 = (K_c * castedInput.Length1) / castedInput.h1;
            checkData.SlendernessRatioMinor = (float)slendernessRatio1;

            double slendernessRatio2 = (castedInput.BucklingLengthCoe2 * castedInput.Length2) / castedInput.h2;
            checkData.SlendernessRatioMajor = (float)slendernessRatio2;

            // The buckling factor K for ℓ₃ can be assumed as K = 1.0 (pinned-pinned).
            double slendernessRatio3 = (castedInput.Length3 / castedInput.h2);
            checkData.SlendernessRatioConnectingMembers = (float)slendernessRatio3;

            if (slendernessRatio1 > 80 || slendernessRatio2 > 50 || slendernessRatio3 > 40)
            {
                checkData.DesignStatus = eDesignStatus.Fail;
                checkData.SlendernessPassed = false;
            }
        }

        /// <summary>
        /// Checks the slenderness ratio limit for solid members or Built-up Columns.
        /// </summary>
        private void ControlSlendernessSolidMembers(TimberCompressionCheckInputUS castedInput, TimberDesCheckDataCompressionUS checkData)
        {
            // slenderness ratio check (can't be more than 50)
            double slendernessRatio1 = (castedInput.BucklingLengthCoe1 * castedInput.Length1) / castedInput.h1;
            double slendernessRatio2 = (castedInput.BucklingLengthCoe2 * castedInput.Length2) / castedInput.h2;

            checkData.SlendernessRatioMajor = (float)slendernessRatio1;
            checkData.SlendernessRatioMinor = (float)slendernessRatio2;
            double criticalSlenderness = Math.Max(slendernessRatio1, slendernessRatio2);

            if (criticalSlenderness > 50)
            {
                checkData.DesignStatus = eDesignStatus.Fail;
                checkData.SlendernessPassed = false;
            }
        }

        /// <summary>
        /// This method calculates the capacities for solid members.
        /// </summary>
        private void ComputeCompressionCapacities(TimberCompressionCheckInputUS castedInput, TimberDesCheckDataCompressionUS checkData)
        {

            // Buckling stresses
            double bucklingStressMajor = CalculateCriticalBucklingDesignStress(checkData.SlendernessRatioMajor, checkData.AdjustedEmin);
            double bucklingStressMinor = CalculateCriticalBucklingDesignStress(checkData.SlendernessRatioMinor, checkData.AdjustedEmin);

            checkData.BucklingDesignStressMajor = (float)bucklingStressMajor;
            checkData.BucklingDesignStressMinor = (float)bucklingStressMinor;

            // The coefficient to account for built-up columns.
            double k_f = 1.0;

            if (bucklingStressMajor > bucklingStressMinor)
            {
                switch (castedInput.builtUPColumnType)
                {
                    case eBuiltUPColumnConnectionType.Bolted:
                        k_f = 0.75;
                        break;
                    case eBuiltUPColumnConnectionType.Nailed:
                        k_f = 0.60;
                        break;
                    default:
                        k_f = 1.0;
                        break;
                }
            }

            double criticalBucklingStress = Math.Min(bucklingStressMajor, bucklingStressMinor);
            checkData.CriticalBucklingStress = (float)criticalBucklingStress;

            // Reference compression design value before C_p
            double refFcBeforeCp =
                castedInput.Fc *
                checkData.LoadDurationFactor *
                checkData.WetServiceFactor *
                checkData.TemperatureFactor *
                checkData.SizeFactor *
                checkData.IncisingFactor *
                checkData.FormatConversionFactor *
                checkData.ResistanceFactor *
                checkData.TimeEffectFactor;

            // Column stability factor
            double columnStabilityFactor = k_f * CalculateColumnStabilityFactor(criticalBucklingStress, refFcBeforeCp, checkData.MaterialAdjustmentFactor);
            checkData.ColumnStabilityFactor = (float)columnStabilityFactor;

            // Final reference Fc
            double grossCompressionDesignValue = refFcBeforeCp * columnStabilityFactor;

            // Compression capacities
            checkData.GrossCompressionCapacity = (float)(grossCompressionDesignValue);
            checkData.NetCompressionCapacity = (float)(refFcBeforeCp);

            double designStress90 = castedInput.Fc90 *
                                    checkData.WetServiceFactor *
                                    checkData.TemperatureFactor *
                                    checkData.IncisingFactor *
                                    checkData.BearingAreaFactor *
                                    checkData.FormatConversionFactorF90 *
                                    checkData.ResistanceFactor90;

            checkData.NetCompressionCapacityPerpendicular = (float)(designStress90);
        }

        /// <summary>
        /// Calculates the bearing area factor: C_b
        /// </summary>
        private double CalculateBearingAreaFactor(double bearingLengthMM)
        {
            double bearLength = bearingLengthMM / 25.4;

            double bearingAreaFactor = 1;
            if (bearLength < 6)
            {
                bearingAreaFactor = (bearLength + 0.375) / bearLength;
            }

            return bearingAreaFactor;
        }

        /// <summary>
        /// Calculates the column stability factor (C_p) based on the critical buckling stress,
        /// reference compression value, and material adjustment factor.
        /// </summary>
        /// <param name="criticalBucklingStress">The critical buckling design stress (F_ce).</param>
        /// <param name="referenceCompressionDesignValueBeforeCp">The reference compression design value (F_c*).</param>
        /// <param name="materialAdjustmentFactor">The material adjustment factor (C).</param>
        /// <returns>The column stability factor (C_p).</returns>
        private double CalculateColumnStabilityFactor(double criticalBucklingStress, double referenceCompressionDesignValueBeforeCp, double materialAdjustmentFactor)
        {

            double term1 = (1 + (criticalBucklingStress / referenceCompressionDesignValueBeforeCp)) / (2 * materialAdjustmentFactor);
            double insideSqrt = Math.Pow(term1, 2) - (criticalBucklingStress / (referenceCompressionDesignValueBeforeCp * materialAdjustmentFactor));

            double Cp = term1 - Math.Sqrt(insideSqrt);

            return Cp;
        }

        /// <summary>
        /// Returns the material adjustment factor: C
        /// </summary>
        private double GetMaterialAdjustmentFactor(eTimberType timberType)
        {
            switch (timberType)
            {
                case eTimberType.SL:
                    return 0.8;
                case eTimberType.RTPP:
                    return 0.85;
                case eTimberType.SGLT:
                case eTimberType.SCL:
                case eTimberType.CLT:
                    return 0.9;
                default:
                    return 0.8;
            }
        }


        /// <summary>
        /// Calculates the critical buckling design stress: F_ce.
        /// </summary>
        private double CalculateCriticalBucklingDesignStress(double slendernessRatio, double adjustedElasticModulus)
        {
            // F_cE = 0.822 · E_min' / (le/d)²  (NDS 3.7.1.5). The coefficient is dimensionless and
            // E_min' and F_cE share units (MPa), so no psi conversion is applied.
            return (0.822 * adjustedElasticModulus) / Math.Pow(slendernessRatio, 2);
        }
        #endregion
    }
}


