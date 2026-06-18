using System;
using MemberDesigner.TimberDesignData.BaseClasses;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.TimberDesignData.Eurocode
{
    public class TimberDesCheckDataTensionEU : TimberDesignCheckData
    {
        #region CTOR

        public TimberDesCheckDataTensionEU()
        {
        }

        #endregion

        #region Public Properties

        // Strength and resistance
        public float DesignMatStrengthValue { get; set; }

        // Section property
        public float NetSectionArea { get; set; }

        // Demands
        public float MaxTensionForce { get; set; }
        public float MaxTensionDemand { get; set; }
        public float MaxTensionDemand90 { get; set; }

        #endregion

        #region Overrides

        public override eTimberDesignCheckType CheckType => eTimberDesignCheckType.Tension;

        public override string GetTitle() => "Design Tension Check";

        /// <summary>
        /// Plain-text value summary: governing formula, key values (2 dp, internal units),
        /// and the resulting utilization ratio.
        /// </summary>
        public override string GetSummary()
        {
            return string.Join(Environment.NewLine, new[]
            {
                "Formula: σt,0,d / ft,0,d ≤ 1.0   (ft,0,d = kmod·ft,0,k / γM)",
                $"Design tensile strength (ft,0,d): {DesignMatStrengthValue:0.00}",
                $"Effective area (A_ef): {NetSectionArea:0.00}",
                $"Design tensile force (F): {MaxTensionForce:0.00}",
                $"Design tensile stress (σt,0,d): {MaxTensionDemand:0.00}",
                $"Design tensile stress perpendicular (σt,90,d): {MaxTensionDemand90:0.00}",
                $"Utilization (D/C): {GetUtilizationRatio():0.00}"
            });
        }

        public override string GetDetailedReportSection() => throw new NotImplementedException();

        public override double GetUtilizationRatio()
            => DesignMatStrengthValue > 0 ? MaxTensionDemand / DesignMatStrengthValue : 0;

        #endregion
    }
}
