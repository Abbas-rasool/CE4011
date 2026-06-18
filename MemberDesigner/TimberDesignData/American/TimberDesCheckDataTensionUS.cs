using System;
using System.Collections.Generic;
using System.Text;
using MemberDesigner.TimberDesignData.BaseClasses;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.TimberDesignData.American
{
    public class TimberDesCheckDataTensionUS : TimberDesignCheckDataAmerican
    {
        #region CTOR

        public TimberDesCheckDataTensionUS()
        {
        }

        #endregion

        #region Public Properties - Tension Design Values

        public float AdjustedTensionDesignValue { get; set; }
        public float TensionDemandStress { get; set; }

        #endregion

        #region Overrides

        public override eTimberDesignCheckType CheckType => eTimberDesignCheckType.Tension;

        public override string GetTitle()
        {
            return "Tension Design (US)";
        }

        /// <summary>
        /// Detached summary returning a plain string.
        /// </summary>
        public override string GetSummary()
        {
            return string.Join(Environment.NewLine, new[]
            {
                "Formula: ft / Ft' ≤ 1.0",
                $"Tension demand stress (ft): {TensionDemandStress:0.00}",
                $"Adjusted tension capacity (Ft'): {AdjustedTensionDesignValue:0.00}",
                $"Utilization (D/C): {GetUtilizationRatio():0.00}"
            });
        }

        /// <summary>
        /// Detached detailed report returning a plain string.
        /// </summary>
        public override string GetDetailedReportSection()
        {
            throw new NotImplementedException();
        }

        public override double GetUtilizationRatio()
        {
            // Simple utilization logic: Demand over Capacity
            return AdjustedTensionDesignValue > 0 ? (TensionDemandStress / AdjustedTensionDesignValue) : 0;
        }

        #endregion
    }
}
