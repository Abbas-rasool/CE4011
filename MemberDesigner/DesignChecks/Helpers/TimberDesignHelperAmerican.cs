using System;
using System.Collections.Generic;
using System.Text;
using MemberDesigner.DesignInputs.American;
using MemberDesigner.TimberDesignData.BaseClasses;
using StructuralLoads;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignChecks.Helpers
{
    public class TimberDesignHelperAmerican
    {
        #region Singleton Definition

        private static TimberDesignHelperAmerican? _instance;

        public static TimberDesignHelperAmerican GetInstance()
        {
            if (_instance == null) _instance = new TimberDesignHelperAmerican();

            return _instance;
        }

        public static void KillInstance()
        {
            _instance = null;
        }

        #endregion

        #region Public Methods Sawn Lumber

        /// <summary>
        /// Returns flat use factor (C_fu) based on dimensions, timber grade, and design parameter.
        /// </summary>
        public double GetFlatUseFactor(
            double widthMm,
            double thicknessMm,
            eTimberGrades timberGrade,
            eDesignParameter designParameter)
        {
            // Convert from mm to inches
            double width = widthMm/25.4; // Convert from mm to inches
            double thickness = thicknessMm/25.4; // Convert from mm to inches

            // Case 1: Thin members (thickness < 5 in.)
            if (thickness < 5)
            {
                if (width <= 3)
                    return thickness <= 4 ? 1.0 : 1.0;

                if (width <= 4)
                    return thickness <= 3 ? 1.1 : (thickness <= 4 ? 1.0 : 1.0);

                if (width <= 5)
                    return thickness <= 3 ? 1.1 : (thickness <= 4 ? 1.05 : 1.0);

                if (width <= 8)
                    return thickness <= 3 ? 1.15 : (thickness <= 4 ? 1.05 : 1.0);

                return thickness <= 3 ? 1.2 : (thickness <= 4 ? 1.1 : 1.0);
            }

            // Case 2: Thick members (thickness >= 5 in.)
            if (designParameter == eDesignParameter.Fb)
            {
                switch (timberGrade)
                {
                    case eTimberGrades.SS:
                        return 0.86;
                    case eTimberGrades.No1:
                        return 0.74;
                    default:
                        return 1.0;
                }
            }

            if (designParameter == eDesignParameter.E ||
                designParameter == eDesignParameter.Emin)
            {
                return timberGrade == eTimberGrades.No1 ? 0.9 : 1.0;
            }

            return 1.0;
        }

        /// <summary>
        /// This method returns the Adjustment factors for Sawn Lumber.
        /// </summary>
        public void GetAdjustmentFactorsSL(TimberCheckInputBaseClassUS castedInput, TimberDesignCheckDataAmerican checkData)
        {
            double thickness = Math.Min(castedInput.h1, castedInput.h2);
            double width = Math.Max(castedInput.h1, castedInput.h2);

            checkData.LoadDurationFactor = castedInput.LoadDurationFactor;
            checkData.WetServiceFactor = (float)GetWetSericeFactor(castedInput.designParameter, castedInput.MoistureContentCondition, thickness);
            checkData.TemperatureFactor = (float)GetTemperatureFactor(castedInput.designParameter, castedInput.MoistureContentCondition, castedInput.Temperature);
            checkData.SizeFactor = (float)ReturnSizeFactor(castedInput.TimberGrade, castedInput.designParameter, castedInput.ApplicationType, width, thickness);
            checkData.IncisingFactor = (float)GetIncisingFactor(castedInput.IsLumberIncised, castedInput.designParameter);

            checkData.FormatConversionFactor = 1;
            checkData.ResistanceFactor = 1;
            checkData.TimeEffectFactor = 1;

            checkData.FormatConversionFactorEmin = 1;
            checkData.ResistanceFactorEmin = 1; // ASD: no resistance factor (LRFD overrides below)

            if (castedInput.loadCombinationType == eLoadCombinationType.LRFD)
            {
                checkData.FormatConversionFactor = (float)GetFormatConversionFactor(castedInput.designParameter, eApplicationType.Member);
                checkData.ResistanceFactor = (float)GetResistanceFactor(castedInput.designParameter, eApplicationType.Member);
                checkData.TimeEffectFactor = castedInput.TimeEffectFactor;

                checkData.FormatConversionFactorEmin = 1.76f;
                checkData.ResistanceFactorEmin = 0.85f;
            }

            // This is for laminated members, To be done if the scope is increased later.
            checkData.VolumeFactor = 1;

        }

        /// <summary>
        /// ASD Only!
        /// Returns the load duration factor based on the load duration given.
        /// Note: C_D for the shortest duration load in a combination of loads shall apply for that load combination.
        /// Note: Based on the statment above, C_d changes for each loading case (shall be handled inside the Capacity Check Class.
        /// </summary>
        public double GetLoadDurationFactor(eLoadDuration loadDuration)
        {
            switch (loadDuration)
            {
                case eLoadDuration.Permanent:
                    return 0.9;

                case eLoadDuration.TenYears:
                    return 1.0;

                case eLoadDuration.TwoMonths:
                    return 1.15;

                case eLoadDuration.SevenDays:
                    return 1.25;

                case eLoadDuration.TenMinutes:
                    return 1.6;

                case eLoadDuration.Impact:
                    return 2.0;

                default:
                    throw new ArgumentOutOfRangeException(nameof(loadDuration), loadDuration, "Invalid load duration type");
            }
        }

        /// <summary>
        /// C_M: Moisture Service Condition of Lumber
        /// Note: when (F_b)(C_F) <= 1150 psi, C_M = 1.0
        /// Note: when (F_c)(C_F) <= 750 psi, C_M = 1.0
        /// </summary>
        public double GetWetSericeFactor(eDesignParameter designParameter, eMoistureContentCondition moistureContentCondition, double thickness)
        {
            if (moistureContentCondition == eMoistureContentCondition.DryServiceConditon)
            {
                return 1.0;
            }
            else
            {
                thickness = thickness/25.4; // Convert from mm to inches

                if (thickness <= 4)
                {
                    switch (designParameter)
                    {
                        case eDesignParameter.Fb:
                            return 0.85;
                        case eDesignParameter.Ft:
                            return 1.00;
                        case eDesignParameter.Fv:
                            return 0.97;
                        case eDesignParameter.Fc90:
                            return 0.67;
                        case eDesignParameter.Fc:
                            return 0.8;
                        case eDesignParameter.E:
                            return 0.90;
                        case eDesignParameter.Emin:
                            return 0.90;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(designParameter), designParameter, "Invalid design parameter type");
                    }
                }
                else
                {
                    switch (designParameter)
                    {
                        case eDesignParameter.Fb:
                            return 1.0;
                        case eDesignParameter.Ft:
                            return 1.0;
                        case eDesignParameter.Fv:
                            return 1.0;
                        case eDesignParameter.Fc90:
                            return 0.67;
                        case eDesignParameter.Fc:
                            return 0.91;
                        case eDesignParameter.E:
                        case eDesignParameter.Emin:
                            return 1.0;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(designParameter), designParameter, "Invalid design parameter type");
                    }
                }

            }
        }


        /// <summary>
        /// The Temperature is in Degree Celsius (C)
        /// This is the modification factor to account for Temperature changes. 
        /// </summary>
        public double GetTemperatureFactor(eDesignParameter designParameter, eMoistureContentCondition moistureContentCondition, double temperature)
        {
            double LowTemperature = 37.78;    // 100°F
            double MediumLowTemperature = 51.67; // 125°F
            double MediumHighTemperature = 65.55; // 150°F

            if (temperature > MediumHighTemperature)
            {
                // Prolonged heating to temperatures above 150°F can cause a permanent loss of strength
                return 0;
            }

            double Ct = 1.0;

            // Common condition for all design parameters
            if (designParameter == eDesignParameter.Fb || designParameter == eDesignParameter.Fv ||
                designParameter == eDesignParameter.Fc || designParameter == eDesignParameter.Fc90)
            {
                if (moistureContentCondition == eMoistureContentCondition.DryServiceConditon)
                {
                    if (temperature <= LowTemperature)
                    {
                        Ct = 1.0;
                    }
                    else if (temperature <= MediumLowTemperature)
                    {
                        Ct = 0.8;
                    }
                    else if (temperature <= MediumHighTemperature)
                    {
                        Ct = 0.7;
                    }
                }
                else
                {
                    if (temperature <= LowTemperature)
                    {
                        Ct = 1.0;
                    }
                    else if (temperature <= MediumLowTemperature)
                    {
                        Ct = 0.7;
                    }
                    else if (temperature <= MediumHighTemperature)
                    {
                        Ct = 0.5;
                    }
                }
            }
            else if (designParameter == eDesignParameter.Ft || designParameter == eDesignParameter.E || designParameter == eDesignParameter.Emin)
            {
                if (temperature <= LowTemperature)
                {
                    Ct = 1.0;
                }
                else if (temperature <= MediumLowTemperature)
                {
                    Ct = 0.9;
                }
            }
            return Ct;
        }

        /// <summary>
        /// C_i: Incising Factor for Sawn Lumber
        /// Note: maximum incising parameters are assumed to have been followed (check NDS section 4.3.8).
        /// </summary>
        public double GetIncisingFactor(bool isLumberIncised, eDesignParameter designParameter)
        {
            if (!isLumberIncised)
            {
                return 1.0;
            }
            else
            {
                switch (designParameter)
                {
                    case eDesignParameter.E:
                    case eDesignParameter.Emin:
                        return 0.95;

                    case eDesignParameter.Fb:
                    case eDesignParameter.Ft:
                    case eDesignParameter.Fc:
                    case eDesignParameter.Fv:
                        return 0.80;

                    case eDesignParameter.Fc90:
                        return 1.00;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(designParameter), designParameter, "Invalid design parameter type");
                }
            }
        }

        /// <summary>
        /// returns format conversion factor: K_F (LRFD only)
        /// </summary>
        public double GetFormatConversionFactor(eDesignParameter designParameter, eApplicationType applicationType)
        {
            if (applicationType == eApplicationType.Connection)
            {
                return 3.32;
            }
            else
            {
                switch (designParameter)
                {
                    case eDesignParameter.Fb:
                        return 2.54;

                    case eDesignParameter.Ft:
                        return 2.70;

                    case eDesignParameter.Fv:
                    //case eDesignParameter.Frt:
                    case eDesignParameter.Fs:
                        return 2.88;

                    case eDesignParameter.Fc:
                        return 2.40;

                    case eDesignParameter.Fc90:
                        return 1.67;

                    case eDesignParameter.Emin:
                        return 1.76;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(designParameter), designParameter, "Invalid design parameter type");
                }
            }
        }


        /// <summary>
        /// returns resistance factor: ∅ (LRFD only)
        /// </summary>
        public double GetResistanceFactor(eDesignParameter designParameter, eApplicationType applicationType)
        {
            if (applicationType == eApplicationType.Connection)
            {
                return 0.65;
            }
            else
            {
                switch (designParameter)
                {
                    case eDesignParameter.Fb:
                        return 0.85;

                    case eDesignParameter.Ft:
                        return 0.80;

                    case eDesignParameter.Fv:
                    //case eDesignParameter.Frt:
                    case eDesignParameter.Fs:
                        return 0.75;

                    case eDesignParameter.Fc:
                    case eDesignParameter.Fc90:
                        return 0.90;

                    case eDesignParameter.Emin:
                        return 0.85;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(designParameter), designParameter, "Invalid design parameter type");
                }
            }
        }

        /// <summary>
        /// Main method to call the size factor: C_F
        /// </summary>
        public double ReturnSizeFactor(eTimberGrades timberGrades, eDesignParameter designParameter, eApplicationType applicationType, double width, double thickness)
        {
            width = width/25.4; // Convert from mm to inches
            thickness = thickness/25.4; // Convert from mm to inches

            double sizeFactor;
            if (thickness > 4)
            {
                sizeFactor = CalculateSizeFactorForThickDimensionLumber(width);
            }
            else
            {
                switch (applicationType)
                {
                    case eApplicationType.Connection:
                    case eApplicationType.Member:

                        sizeFactor = GetSizeFactorForThinLumber(timberGrades, designParameter, width, thickness);
                        break;

                    case eApplicationType.Deck:

                        if (thickness <= 2)
                        {
                            sizeFactor = 1.10;
                        }
                        else if (thickness <= 3)
                        {
                            sizeFactor = 1.04;
                        }
                        else
                        {
                            sizeFactor = 1;
                        }
                        break;

                    default:
                        sizeFactor = 1.0;
                        break;

                }
            }
            return sizeFactor;
        }

        #endregion

        #region Private Methods Sawn Lumber

        /// <summary>
        /// 4.3.6.2 calculates size factor for dimension lumber 100 mm or thicker (5'').
        /// </summary>
        private double CalculateSizeFactorForThickDimensionLumber(double depth)
        {
            if (depth > 12)
            {
                double sizeFactor = Math.Pow((12 / depth), (1 / 9));
                return sizeFactor;
            }
            else
            {
                return 1.0;
            }

        }

        /// <summary>
        /// Tabulated bending, tensionn, and compression parallel to grain design values for dimension lumber 50 mm to 100 mm (2'' to 4'') thick 
        /// shall be multiplied by the following size factor. (Table 4A, Adjustment Factors).
        /// </summary>
        private double GetSizeFactorForThinLumber(eTimberGrades timberGrades, eDesignParameter designParameter, double width, double thickness)
        {
            // For timber grades with similar logic
            if (timberGrades == eTimberGrades.Cons || timberGrades == eTimberGrades.Stan)
                return 1.0;

            if (timberGrades == eTimberGrades.Ut)
            {
                if (width <= 3)
                {
                    switch (designParameter)
                    {
                        case eDesignParameter.Fb:
                            return (thickness <= 3) ? 0.4 : 0;
                        case eDesignParameter.Ft:
                            return 0.4;
                        case eDesignParameter.Fc:
                            return 0.6;
                        default:
                            return 1;
                    }
                }
                else if (width <= 4)
                    return 1;
                else
                    return 1;
            }

            if (timberGrades == eTimberGrades.SS || timberGrades == eTimberGrades.No1 ||
                timberGrades == eTimberGrades.No2 || timberGrades == eTimberGrades.No3)
            {
                double factor = GetSizeFactorSS(designParameter, width, thickness);
                return factor;
            }

            if (timberGrades == eTimberGrades.Stud)
            {
                if (width <= 4)
                {
                    switch (designParameter)
                    {
                        case eDesignParameter.Fb:
                        case eDesignParameter.Ft:
                            return 1.1;

                        case eDesignParameter.Fc:
                            return 1.05;

                        default:
                            return 1;
                    }
                }
                else if (width <= 6)
                {
                    return 1.0;
                }
                else
                {
                    double factor = GetSizeFactorSS(designParameter, width, thickness);
                    return factor;
                }
            }

            return 0;
        }

        /// <summary>
        /// Get's the size factor for the SS, No.1, No.2, No.3, and stud types. 
        /// </summary>
        private double GetSizeFactorSS(eDesignParameter designParameter, double width, double thickness)
        {
            if (width <= 4)
            {
                switch (designParameter)
                {
                    case eDesignParameter.Fb:
                    case eDesignParameter.Ft:
                        return 1.5;

                    case eDesignParameter.Fc:
                        return 1.15;

                    default:
                        return 1;
                }
            }
            else if (width <= 5)
            {
                switch (designParameter)
                {
                    case eDesignParameter.Fb:
                    case eDesignParameter.Ft:
                        return 1.4;

                    case eDesignParameter.Fc:
                        return 1.10;

                    default:
                        return 1;
                }
            }
            else if (width <= 6)
            {
                switch (designParameter)
                {
                    case eDesignParameter.Fb:
                    case eDesignParameter.Ft:
                        return 1.3;

                    case eDesignParameter.Fc:
                        return 1.1;

                    default:
                        return 1;
                }
            }
            else if (width <= 8)
            {
                switch (designParameter)
                {
                    case eDesignParameter.Fb:
                        if (thickness <= 3)
                        {
                            return 1.2;
                        }
                        else
                        {
                            return 1.3;
                        }

                    case eDesignParameter.Ft:
                        return 1.2;

                    case eDesignParameter.Fc:
                        return 1.05;

                    default:
                        return 1;
                }
            }
            else if (width <= 10)
            {
                switch (designParameter)
                {
                    case eDesignParameter.Fb:
                        if (thickness <= 3)
                        {
                            return 1.1;
                        }
                        else
                        {
                            return 1.2;
                        }

                    case eDesignParameter.Ft:
                        return 1.1;

                    case eDesignParameter.Fc:
                        return 1.0;

                    default:
                        return 1;
                }
            }
            else if (width <= 12)
            {
                switch (designParameter)
                {
                    case eDesignParameter.Fb:
                        if (thickness <= 3)
                        {
                            return 1.0;
                        }
                        else
                        {
                            return 1.1;
                        }

                    case eDesignParameter.Ft:
                        return 1.1;

                    case eDesignParameter.Fc:
                        return 1.0;

                    default:
                        return 1;
                }
            }
            else
            {
                switch (designParameter)
                {
                    case eDesignParameter.Fb:
                        if (thickness <= 3)
                        {
                            return 0.9;
                        }
                        else
                        {
                            return 1.0;
                        }

                    case eDesignParameter.Ft:
                    case eDesignParameter.Fc:
                        return 0.9;

                    default:
                        return 1;
                }
            }
        }

        #endregion
    }
}
