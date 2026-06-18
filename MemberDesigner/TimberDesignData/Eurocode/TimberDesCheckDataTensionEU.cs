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

        public override string GetSummary()
        {
            return string.Join(Environment.NewLine, new[]
            {
                $"Design Tensile Strength (f_t,0,d): {DesignMatStrengthValue}",
                $"Effective Area (A_ef): {NetSectionArea}",
                $"Design Tensile Force (F): {MaxTensionForce}",
                $"Design Tensile Stress (σ_t,0,d): {MaxTensionDemand}",
                $"Design Tensile Stress Perpendicular (σ_t,90,d): {MaxTensionDemand90}"
            });
        }

        public override string GetDetailedReportSection() => throw new NotImplementedException();

        public override double GetUtilizationRatio()
            => DesignMatStrengthValue > 0 ? MaxTensionDemand / DesignMatStrengthValue : 0;

        #endregion
    }
}
