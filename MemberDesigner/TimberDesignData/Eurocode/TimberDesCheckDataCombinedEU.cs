using System;
using System.Text;
using MemberDesigner.TimberDesignData.BaseClasses;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.TimberDesignData.Eurocode
{
    public class TimberDesCheckDataCombinedEU : TimberDesignCheckData
    {
        #region CTOR

        public TimberDesCheckDataCombinedEU()
        {
        }

        #endregion

        #region Public Properties

        public float KmInteractionFactor { get; set; }
        public float TensionStressRatio { get; set; }
        public float MomentStressRatioMajor { get; set; }
        public float MomentStressRatioMinor { get; set; }
        public float TensionCapacityLimitMajor { get; set; }
        public float TensionCapacityLimitMinor { get; set; }

        public float CompressionStressRatio { get; set; }
        public float CompressionCapacityLimitMajor { get; set; }
        public float CompressionCapacityLimitMinor { get; set; }

        public bool IsExtraCheckNeeded { get; set; } = false;
        public float IMajor { get; set; }
        public float IMinor { get; set; }
        public float GyrationRadiusMajor { get; set; }
        public float GyrationRadiusMinor { get; set; }
        public float SlendernessRatioMajor { get; set; }
        public float SlendernessRatioMinor { get; set; }
        public float SigmaRelMajor { get; set; }
        public float SigmaRelMinor { get; set; }
        public float Beta_C { get; set; }
        public float K_Major { get; set; }
        public float K_Minor { get; set; }
        public float Kc_Major { get; set; }
        public float Kc_Minor { get; set; }
        public float CapacityCheckLimitCompMajor { get; set; }
        public float CapacityCheckLimitCompMinor { get; set; }

        public float σ_m_crit { get; set; }
        public float SigmaRelMoment { get; set; }
        public float K_crit { get; set; }
        public float CapacityCheckLimitMoment { get; set; }
        public float MomentMajorAxisLimit { get; set; }

        #endregion

        #region Overrides

        public override eTimberDesignCheckType CheckType => eTimberDesignCheckType.CombinedBendingAxial;

        public override string GetTitle() => "Design Combined Bending and Axial Check";

        public override string GetSummary()
        {
            var sb = new StringBuilder();

            sb.AppendLine("Combined Bending and Axial Tension");
            sb.AppendLine($"Reduction Factor (k_m): {KmInteractionFactor}");
            sb.AppendLine($"Interaction Ratio (Major Axis) (R_Major,t): {TensionCapacityLimitMajor}");
            sb.AppendLine($"Interaction Ratio (Minor Axis) (R_Minor,t): {TensionCapacityLimitMinor}");
            sb.AppendLine();

            sb.AppendLine("Combined Bending and Axial Compression");
            sb.AppendLine($"Compression Interaction Ratio (Major Axis) (R_Major,C): {CompressionCapacityLimitMajor}");
            sb.AppendLine($"Compression Interaction Ratio (Minor Axis) (R_Minor,C): {CompressionCapacityLimitMinor}");
            sb.AppendLine();

            sb.AppendLine("Stability of Members");
            sb.AppendLine($"Slenderness (Major Axis) (λ_major): {SlendernessRatioMajor}");
            sb.AppendLine($"Slenderness (Minor Axis) (λ_minor): {SlendernessRatioMinor}");
            sb.AppendLine($"Bending Slenderness Ratio (Major Axis) (λ_rel,Major): {SigmaRelMajor}");
            sb.AppendLine($"Bending Slenderness Ratio (Minor Axis) (λ_rel,minor): {SigmaRelMinor}");

            if (!IsExtraCheckNeeded)
            {
                sb.AppendLine("Information: Where λ_rel,y ≤ 0.3 and λ_rel,z ≤ 0.3, no extra check is needed.");
            }
            else
            {
                sb.AppendLine($"Imperfection Factor (β_c): {Beta_C}");
                sb.AppendLine($"Intermediate Factor (Major Axis) (k_major): {K_Major}");
                sb.AppendLine($"Intermediate Factor (Minor Axis) (k_minor): {K_Minor}");
                sb.AppendLine($"Instability Factor (Major Axis) (k_c,major): {Kc_Major}");
                sb.AppendLine($"Instability Factor (Minor Axis) (k_c,minor): {Kc_Minor}");
                sb.AppendLine($"Stability Adjusted Interaction (Major Axis) (R_c,y): {CapacityCheckLimitCompMajor}");
                sb.AppendLine($"Stability Adjusted Interaction (Minor Axis) (R_c,z): {CapacityCheckLimitCompMinor}");
            }

            sb.AppendLine();
            sb.AppendLine("Lateral Torsional Stability");
            sb.AppendLine($"Stability Bending Stress (σ_m,crit): {σ_m_crit}");
            sb.AppendLine($"Relative Slenderness for Bending (λ_rel,m): {SigmaRelMoment}");

            string kCritEquation;
            if (SigmaRelMoment <= 0.75)
            {
                kCritEquation = "k_crit = 1";
            }
            else if (SigmaRelMoment > 0.75 && SigmaRelMoment <= 1.4)
            {
                kCritEquation = "k_crit = 1.56 - 0.75 * λ_rel,m";
            }
            else
            {
                kCritEquation = "k_crit = 1 / λ_rel,m^2";
            }

            sb.AppendLine($"Lateral Buckling Stability Factor (k_crit): {K_crit}  [{kCritEquation}]");
            sb.AppendLine($"Major Axis Moment Limit (LT Buckling) (f_major,crit): {MomentMajorAxisLimit}");
            sb.AppendLine($"Stability-Adjusted Moment Ratio (R_m,crit): {CapacityCheckLimitMoment}");

            return sb.ToString().TrimEnd();
        }

        public override string GetDetailedReportSection() => throw new NotImplementedException();

        public override double GetUtilizationRatio()
        {
            double max = 0;
            max = Math.Max(max, TensionCapacityLimitMajor);
            max = Math.Max(max, TensionCapacityLimitMinor);
            max = Math.Max(max, CompressionCapacityLimitMajor);
            max = Math.Max(max, CompressionCapacityLimitMinor);
            max = Math.Max(max, CapacityCheckLimitCompMajor);
            max = Math.Max(max, CapacityCheckLimitCompMinor);
            max = Math.Max(max, CapacityCheckLimitMoment);
            return max;
        }

        #endregion
    }
}
