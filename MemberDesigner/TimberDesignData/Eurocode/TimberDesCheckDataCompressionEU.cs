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

        public override string GetSummary()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Design Compressive Strength (f_c,0,d): {DesignStrengthMatParallel}");
            sb.AppendLine($"Effective Area (A_ef): {NetSectionArea}");
            sb.AppendLine($"Design Compressive Stress (σ_c,0,d): {MaxDemandParallel}");
            sb.AppendLine($"Safety Factor (K_c90): {K_C90}");
            sb.AppendLine($"Design Compressive Strength Perpendicular (f_c,90,d): {DesignStrengthMatPerpendicular}");
            sb.AppendLine($"Effective Area (A_ef,90): {NetSectionArea90}");
            sb.AppendLine($"Design Compressive Stress Perpendicular (σ_c,90,d): {MaxDemandPerpendicular}");

            if (AppliedAngleRad > 0)
            {
                sb.AppendLine($"Load Application Angle (α): {AppliedAngleRad} rad");
                sb.AppendLine($"Design Compressive Strength (Angled) (f_c,α,d): {DesignStrengthMatAngled}");
                sb.AppendLine($"Effective Area (A_ef,α): {NetSectionAreaAngled}");
                sb.AppendLine($"Compressive Stress (Angled) (σ_c,α,d): {MaxDemandAngled}");
            }

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
