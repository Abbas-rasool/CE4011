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

        public override string GetSummary()
        {
            return string.Join(Environment.NewLine, new[]
            {
                $"Design Bending Strength (f_m,d): {DesignMatStrength}",
                $"Reduction Factor (k_m): {KmInteractionFactor}",
                $"Design Bending Moment (Major) (M_maj): {MajorDemandMoment}",
                $"Design Bending Moment (Minor) (M_min): {MinorDemandMoment}",
                $"Design Bending Stress (Major) (σ_m,Major): {MajorDemandStress}",
                $"Design Bending Stress (Minor) (σ_m,Minor): {MinorDemandStress}",
                $"Check Ratio (Major Axis) (R_Major): {MajorCheckFactor}",
                $"Check Ratio (Minor Axis) (R_Minor): {MinorCheckFactor}"
            });
        }

        public override string GetDetailedReportSection() => throw new NotImplementedException();

        public override double GetUtilizationRatio() => Math.Max(MajorCheckFactor, MinorCheckFactor);

        #endregion
    }
}
