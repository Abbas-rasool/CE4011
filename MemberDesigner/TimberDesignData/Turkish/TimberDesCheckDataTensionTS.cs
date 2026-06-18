using System;
using MemberDesigner.TimberDesignData.BaseClasses;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.TimberDesignData.Turkish
{
    public class TimberDesCheckDataTensionTS : TimberDesignCheckData
    {
        #region CTOR

        public TimberDesCheckDataTensionTS()
        {
        }

        #endregion

        #region Public Properties

        public float C_B { get; set; }
        public float MaterialResistance { get; set; }
        public float DemandStress { get; set; }

        #endregion

        #region Overrides

        public override eTimberDesignCheckType CheckType => eTimberDesignCheckType.Tension;

        public override string GetTitle() => "Tension Design (TR)";

        /// <summary>
        /// Plain-text value summary: governing formula, key values (2 dp, internal units),
        /// and the resulting utilization ratio.
        /// </summary>
        public override string GetSummary()
        {
            return string.Join(Environment.NewLine, new[]
            {
                "Formula: σt / ft,d ≤ 1.0",
                $"Demand stress (σt): {DemandStress:0.00}",
                $"Tension resistance (ft,d): {MaterialResistance:0.00}",
                $"Modification factor (C_B): {C_B:0.00}",
                $"Utilization (D/C): {GetUtilizationRatio():0.00}"
            });
        }

        public override string GetDetailedReportSection() => throw new NotImplementedException();

        public override double GetUtilizationRatio()
            => MaterialResistance > 0 ? DemandStress / MaterialResistance : 0;

        #endregion
    }
}
