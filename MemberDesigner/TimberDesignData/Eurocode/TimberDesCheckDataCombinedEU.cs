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

        /// <summary>
        /// Plain-text value summary: governing interaction formulas, key ratios (2 dp), and the
        /// resulting utilization ratio.
        /// </summary>
        public override string GetSummary()
        {
            var sb = new StringBuilder();

            sb.AppendLine("Formula (tension): σt,0,d/ft,0,d + km·σm/fm,d ≤ 1.0   (EN 1995-1-1 §6.2.3)");
            sb.AppendLine("Formula (compression): σc,0,d/(kc·fc,0,d) + σm,y/fm,y,d + km·σm,z/fm,z,d ≤ 1.0   (§6.3.2)");
            sb.AppendLine($"Reduction factor (km): {KmInteractionFactor:0.00}");
            sb.AppendLine($"Axial+bending tension ratio (major / minor): {TensionCapacityLimitMajor:0.00} / {TensionCapacityLimitMinor:0.00}");
            sb.AppendLine($"Axial+bending compression ratio (major / minor): {CompressionCapacityLimitMajor:0.00} / {CompressionCapacityLimitMinor:0.00}");
            sb.AppendLine($"Relative slenderness (λrel,y / λrel,z): {SigmaRelMajor:0.00} / {SigmaRelMinor:0.00}");

            if (IsExtraCheckNeeded)
            {
                sb.AppendLine($"Instability factor (kc,y / kc,z): {Kc_Major:0.00} / {Kc_Minor:0.00}");
                sb.AppendLine($"Stability-adjusted ratio (major / minor): {CapacityCheckLimitCompMajor:0.00} / {CapacityCheckLimitCompMinor:0.00}");
            }
            else
            {
                sb.AppendLine("Note: λrel,y ≤ 0.3 and λrel,z ≤ 0.3 — no extra stability check required.");
            }

            sb.AppendLine($"Lateral-torsional factor (kcrit): {K_crit:0.00}");
            sb.AppendLine($"LT-stability moment ratio (R_m,crit): {CapacityCheckLimitMoment:0.00}");
            sb.AppendLine($"Utilization (D/C): {GetUtilizationRatio():0.00}");

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
