using System;
using MemberDesigner.TimberDesignData.BaseClasses;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.TimberDesignData.Turkish
{
    public class TimberDesCheckDataBendingTS : TimberDesignCheckData
    {
        #region CTOR

        public TimberDesCheckDataBendingTS()
        {
        }

        #endregion

        #region Public Properties

        public float C_B { get; set; }
        public float C_E { get; set; }
        public float MaterialStrength { get; set; }
        public float MajorDemandStress { get; set; }
        public float MinorDemandStress { get; set; }
        public float MajorCheckFactor { get; set; }
        public float MinorCheckFactor { get; set; }
        public float σ_yb { get; set; }
        public float λ_yb { get; set; }
        public float C_yb { get; set; }
        public float SectionBucklingStrength { get; set; }

        #endregion

        #region Overrides

        public override eTimberDesignCheckType CheckType => eTimberDesignCheckType.Bending;

        public override string GetTitle() => "Bending Design (TR)";

        /// <summary>
        /// Plain-text value summary: governing formula, key values (2 dp, internal units),
        /// and the resulting utilization ratio.
        /// </summary>
        public override string GetSummary()
        {
            return string.Join(Environment.NewLine, new[]
            {
                "Formula: σm / fm,d ≤ 1.0   (fm,d = Cyb·fm·CB·CE / ω)",
                $"Material bending strength (fm): {MaterialStrength:0.00}",
                $"Demand stress major (σm,major): {MajorDemandStress:0.00}",
                $"Demand stress minor (σm,minor): {MinorDemandStress:0.00}",
                $"Section buckling strength: {SectionBucklingStrength:0.00}",
                $"Stability factor (Cyb): {C_yb:0.00}",
                $"Check ratio major / minor: {MajorCheckFactor:0.00} / {MinorCheckFactor:0.00}",
                $"Utilization (D/C): {GetUtilizationRatio():0.00}"
            });
        }

        public override string GetDetailedReportSection() => throw new NotImplementedException();

        public override double GetUtilizationRatio() => Math.Max(MajorCheckFactor, MinorCheckFactor);

        #endregion
    }
}
