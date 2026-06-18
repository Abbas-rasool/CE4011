using System;
using MemberDesigner.TimberDesignData.BaseClasses;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.TimberDesignData.Eurocode
{
    public class TimberDesCheckDataBendingEU : TimberDesignCheckData
    {
        #region CTOR

        public TimberDesCheckDataBendingEU()
        {
        }

        #endregion

        #region Public Properties

        public float MajorCheckFactor { get; set; }
        public float MinorCheckFactor { get; set; }
        public float MajorDemandMoment { get; set; }
        public float MinorDemandMoment { get; set; }
        public float MajorDemandStress { get; set; }
        public float MinorDemandStress { get; set; }
        public float DesignMatStrength { get; set; }
        public float KmInteractionFactor { get; set; }

        #endregion

        #region Overrides

        public override eTimberDesignCheckType CheckType => eTimberDesignCheckType.Bending;

        public override string GetTitle() => "Design Bending Check";

        /// <summary>
        /// Plain-text value summary: governing formula, key values (2 dp, internal units),
        /// and the resulting utilization ratio.
        /// </summary>
        public override string GetSummary()
        {
            return string.Join(Environment.NewLine, new[]
            {
                "Formula: σm,y,d/fm,y,d + km·σm,z,d/fm,z,d ≤ 1.0   (and the km-swapped pair)",
                $"Design bending strength (fm,d): {DesignMatStrength:0.00}",
                $"Reduction factor (km): {KmInteractionFactor:0.00}",
                $"Design bending stress major (σm,y,d): {MajorDemandStress:0.00}",
                $"Design bending stress minor (σm,z,d): {MinorDemandStress:0.00}",
                $"Check ratio major (R_major): {MajorCheckFactor:0.00}",
                $"Check ratio minor (R_minor): {MinorCheckFactor:0.00}",
                $"Utilization (D/C): {GetUtilizationRatio():0.00}"
            });
        }

        public override string GetDetailedReportSection() => throw new NotImplementedException();

        public override double GetUtilizationRatio() => Math.Max(MajorCheckFactor, MinorCheckFactor);

        #endregion
    }
}
