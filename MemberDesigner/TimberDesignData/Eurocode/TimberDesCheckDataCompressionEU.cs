using System;
using System.Text;
using MemberDesigner.TimberDesignData.BaseClasses;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.TimberDesignData.Eurocode
{
    public class TimberDesCheckDataCompressionEU : TimberDesignCheckData
    {
        #region CTOR

        public TimberDesCheckDataCompressionEU()
        {
        }

        #endregion

        #region Public Properties

        // Strength values
        public float DesignStrengthMatParallel { get; set; }
        public float DesignStrengthMatPerpendicular { get; set; }
        public float K_C90 { get; set; }
        public float DesignStrengthMatAngled { get; set; }

        // Demands
        public float MaxDemandParallel { get; set; }
        public float MaxDemandPerpendicular { get; set; }
        public float MaxDemandAngled { get; set; }

        // Angle of loading (in radians)
        public float AppliedAngleRad { get; set; }
        public float NetSectionArea { get; set; }
        public float NetSectionArea90 { get; set; }
        public float NetSectionAreaAngled { get; set; }

        #endregion

        #region Overrides

        public override eTimberDesignCheckType CheckType => eTimberDesignCheckType.Compression;

        public override string GetTitle() => "Design Compression Check";

        /// <summary>
        /// Plain-text value summary: governing formula, key values (2 dp, internal units),
        /// and the resulting utilization ratio.
        /// </summary>
        public override string GetSummary()
        {
            var sb = new StringBuilder();

            sb.AppendLine("Formula: σc,0,d / fc,0,d ≤ 1.0   (fc,0,d = kmod·fc,0,k / γM)");
            sb.AppendLine($"Design compressive strength (fc,0,d): {DesignStrengthMatParallel:0.00}");
            sb.AppendLine($"Effective area (A_ef): {NetSectionArea:0.00}");
            sb.AppendLine($"Design compressive stress (σc,0,d): {MaxDemandParallel:0.00}");
            sb.AppendLine($"Bearing factor (kc,90): {K_C90:0.00}");
            sb.AppendLine($"Strength perpendicular (fc,90,d): {DesignStrengthMatPerpendicular:0.00}");
            sb.AppendLine($"Stress perpendicular (σc,90,d): {MaxDemandPerpendicular:0.00}");

            if (AppliedAngleRad > 0)
            {
                sb.AppendLine($"Load angle (α): {AppliedAngleRad:0.00} rad");
                sb.AppendLine($"Strength at angle (fc,α,d): {DesignStrengthMatAngled:0.00}");
                sb.AppendLine($"Stress at angle (σc,α,d): {MaxDemandAngled:0.00}");
            }

            sb.AppendLine($"Utilization (D/C): {GetUtilizationRatio():0.00}");
            return sb.ToString().TrimEnd();
        }

        public override string GetDetailedReportSection() => throw new NotImplementedException();

        public override double GetUtilizationRatio()
        {
            double parallel = DesignStrengthMatParallel > 0 ? MaxDemandParallel / DesignStrengthMatParallel : 0;
            double perp = DesignStrengthMatPerpendicular > 0 ? MaxDemandPerpendicular / DesignStrengthMatPerpendicular : 0;
            double angled = DesignStrengthMatAngled > 0 ? MaxDemandAngled / DesignStrengthMatAngled : 0;
            return Math.Max(parallel, Math.Max(perp, angled));
        }

        #endregion
    }
}
