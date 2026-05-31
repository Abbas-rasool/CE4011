using System;
using System.Collections.Generic;
using System.Text;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignChecks.Helpers
{
    public abstract class TimberBaseDesignHelper
    {

        #region Public Methods
        public double CalculateCi(double b0, double wallWidth)
        {
            if (wallWidth >= b0)
            {
                return 1.0;
            }

            return wallWidth / b0;
        }

        public double CalculateV_dc(double A_top, double f_c0d, double λ_et)
        {
            double V_dc;

            if (λ_et < 30)
            {
                V_dc = (A_top * f_c0d) / 120.0;
            }
            else if (λ_et >= 30 && λ_et < 60)
            {
                V_dc = (A_top * f_c0d * λ_et) / 3600.0;
            }
            else // λ_et >= 60
            {
                V_dc = (A_top * f_c0d) / 60.0;
            }

            return V_dc;
        }

        /// <summary>
        /// Table C.1/ Tablo 4.2 b - The factor η
        /// </summary>
        public double Get_η_Factor(eLoadDurationClass loadDurationClass, eShaftConnectionType shaftConnectionType, eBuiltUPColumnConnectionType connectionType)
        {
            double eta = 0;

            switch (loadDurationClass)
            {
                case eLoadDurationClass.PermanentAction:
                case eLoadDurationClass.LongTermAction:
                    if (shaftConnectionType == eShaftConnectionType.Pack)
                    {
                        switch (connectionType)
                        {
                            case eBuiltUPColumnConnectionType.Glued:
                                eta = 1;
                                break;
                            case eBuiltUPColumnConnectionType.Nailed:
                                eta = 4;
                                break;
                            case eBuiltUPColumnConnectionType.Bolted:
                                eta = 3.5;
                                break;
                        }
                    }
                    else if (shaftConnectionType == eShaftConnectionType.Gusset)
                    {
                        switch (connectionType)
                        {
                            case eBuiltUPColumnConnectionType.Glued:
                                eta = 3;
                                break;
                            case eBuiltUPColumnConnectionType.Nailed:
                                eta = 6;
                                break;
                        }
                    }
                    break;

                case eLoadDurationClass.MediumTermAction:
                case eLoadDurationClass.ShortTermAction:
                    if (shaftConnectionType == eShaftConnectionType.Pack)
                    {
                        switch (connectionType)
                        {
                            case eBuiltUPColumnConnectionType.Glued:
                                eta = 1;
                                break;
                            case eBuiltUPColumnConnectionType.Nailed:
                                eta = 3;
                                break;
                            case eBuiltUPColumnConnectionType.Bolted:
                                eta = 2.5;
                                break;
                        }
                    }
                    else if (shaftConnectionType == eShaftConnectionType.Gusset)
                    {
                        switch (connectionType)
                        {
                            case eBuiltUPColumnConnectionType.Glued:
                                eta = 2;
                                break;
                            case eBuiltUPColumnConnectionType.Nailed:
                                eta = 4.5;
                                break;
                        }
                    }
                    break;
            }

            return eta;
        }


        public (double A_tot, double I_tot) CalculateI_tot(int shaftCount, double b, double h, double a)
        {
            // Validate input to ensure it's a valid shaft count
            if (shaftCount < 2 || shaftCount > 4)
            {
                throw new ArgumentException("Invalid shaft count. Must be 2, 3, or 4.", nameof(shaftCount));
            }

            double A = b * h;
            double A_tot = shaftCount * A;
            double I_tot = 0;

            switch (shaftCount)
            {
                case 2:
                    {
                        double term1 = 2 * h + a;
                        I_tot = (b * (Math.Pow(term1, 3) - Math.Pow(a, 3))) / 12;
                        break;
                    }
                case 3:
                    {
                        double term1 = 3 * h + 2 * a;
                        double term2 = h + 2 * a;
                        I_tot = (b * (Math.Pow(term1, 3) - Math.Pow(term2, 3) + Math.Pow(h, 3))) / 12;
                        break;
                    }
                case 4:
                    {
                        double term1 = 4 * h + 3 * a;
                        double term2 = 2 * h + 3 * a;
                        double term3 = 2 * h + a;
                        I_tot = (b * (Math.Pow(term1, 3) - Math.Pow(term2, 3) + Math.Pow(term3, 3) - Math.Pow(a, 3))) / 12;
                        break;
                    }
            }

            return (A_tot, I_tot);
        }

        /// <summary>
        /// Calculates the size factor kh or kl based on material type and dimensions.
        /// C_B for Turkish Standards.
        /// </summary>
        public double CalculateSizeFactor(
            eTimberMaterialType materialType,
            double width,
            double characteristicDensity = 0,
            double length = 0,
            double sizeExponent = 0.2,
            bool isTension = false)
        {
            switch (materialType)
            {
                case eTimberMaterialType.SolidTimber:
                    if (characteristicDensity <= 700 && width < 150)
                    {
                        double kh = Math.Pow(150 / width, 0.2);
                        return Math.Min(kh, 1.3);
                    }
                    return 1.0;

                case eTimberMaterialType.GluedLaminatedTimber:
                    if (width < 600)
                    {
                        double kh = Math.Pow(600 / width, 0.1);
                        return Math.Min(kh, 1.1);
                    }
                    return 1.0;

                case eTimberMaterialType.LVL:
                    return 1.0;
                // This case would be fixed later if we wanna include LVL.
                //if (isTension)
                //{
                //    if (length < 3000)
                //    {
                //        double kl = Math.Pow(3000 / length, sizeExponent * 0.5);
                //        return Math.Min(kl, 1.1);
                //    }
                //    return 1.0;
                //}
                //else // bending
                //{
                //    if (width < 300)
                //    {
                //        double kh = Math.Pow(300 / width, sizeExponent);
                //        return Math.Min(kh, 1.2);
                //    }
                //    return 1.0;
                //}

                default:
                    return 1.0; // No adjustment for other material types
            }
        }

        /// <summary>
        /// calculates omega for TS (Ω)
        /// This method returns the recommended partial factors for material properties and resistances.
        /// </summary>
        public float GetPartialFactor(eTimberMaterialType material)
        {
            switch (material)
            {
                case eTimberMaterialType.SolidTimber:
                case eTimberMaterialType.ParticleBoards:
                case eTimberMaterialType.FibreboardsHard:
                case eTimberMaterialType.FibreboardsMedium:
                case eTimberMaterialType.FibreboardsMDF:
                case eTimberMaterialType.FibreboardsSoft:
                case eTimberMaterialType.Connections:
                case eTimberMaterialType.CLT: // This was only in TS but we implemented in both TS and EC.
                    return 1.3f;

                case eTimberMaterialType.GluedLaminatedTimber:
                case eTimberMaterialType.PunchedMetalPlateFasteners:
                    return 1.25f;

                case eTimberMaterialType.LVL:
                case eTimberMaterialType.Plywood:
                case eTimberMaterialType.OSB:
                    return 1.2f;

                case eTimberMaterialType.AccidentalCombinations:
                    return 1.0f;

                default:
                    return 1.0f;
            }
        }

        /// <summary>
        /// Calculates K_def
        /// Returns a factor for the evaluation of creep deformation taking into account the relevant service class.
        /// </summary>
        public double GetK_CreepDeformation(eServiceClass serviceClassEU, eTimberMaterialType materialTypeEU)
        {
            switch (materialTypeEU)
            {
                case eTimberMaterialType.SolidTimber:
                case eTimberMaterialType.GluedLaminatedTimber:
                case eTimberMaterialType.LVL:
                    if (serviceClassEU == eServiceClass.ServiceClass1) return 0.60;
                    if (serviceClassEU == eServiceClass.ServiceClass2) return 0.80;
                    if (serviceClassEU == eServiceClass.ServiceClass3) return 2.00;
                    break;

                case eTimberMaterialType.Plywood:
                    if (serviceClassEU == eServiceClass.ServiceClass1) return 0.80;
                    if (serviceClassEU == eServiceClass.ServiceClass2) return 1.00;
                    if (serviceClassEU == eServiceClass.ServiceClass3) return 2.50;
                    break;

                case eTimberMaterialType.OSB:
                    if (serviceClassEU == eServiceClass.ServiceClass1) return 1.50;
                    if (serviceClassEU == eServiceClass.ServiceClass2) return 2.25;
                    break;

                case eTimberMaterialType.ParticleBoards:
                    if (serviceClassEU == eServiceClass.ServiceClass1) return 2.25;
                    if (serviceClassEU == eServiceClass.ServiceClass2) return 3.00;
                    break;

                case eTimberMaterialType.FibreboardsHard:
                    if (serviceClassEU == eServiceClass.ServiceClass1) return 2.25;
                    if (serviceClassEU == eServiceClass.ServiceClass2) return 3.00;
                    break;

                case eTimberMaterialType.FibreboardsMedium:
                    if (serviceClassEU == eServiceClass.ServiceClass1) return 3.00;
                    if (serviceClassEU == eServiceClass.ServiceClass2) return 4.00;
                    break;

                case eTimberMaterialType.FibreboardsMDF:
                    if (serviceClassEU == eServiceClass.ServiceClass1) return 2.25;
                    if (serviceClassEU == eServiceClass.ServiceClass2) return 3.00;
                    break;
            }

            // Return 1 if combination is invalid or not defined in the table
            return 1;
        }

        /// <summary>
        /// This method calculates σ_m,crit.
        /// Formula: σ_m,crit = (0.78 * b^2 / (h * l_ef)) * E_0.05
        /// σ_yb for turkish standards.
        /// </summary>
        public double CalculateSigmaMCrit(double width, double thickness, double L_ef, double E_005)
        {
            return (0.78 * Math.Pow(width, 2) / (thickness * L_ef)) * E_005;
        }

        /// <summary>
        /// This function calculates k_crit, a factor taking into account reduced bending strength due to lateral buckling.
        /// C_YB for turkish standards.
        /// </summary>
        public double CalculateKCrit(double relativeStiffnessRatioMoment)
        {
            if (relativeStiffnessRatioMoment <= 0.75)
            {
                return 1.0;
            }
            else if (relativeStiffnessRatioMoment <= 1.4)
            {
                return 1.56 - 0.75 * relativeStiffnessRatioMoment;
            }
            else
            {
                return 1.0 / Math.Pow(relativeStiffnessRatioMoment, 2);
            }
        }
        #endregion
    }
}
