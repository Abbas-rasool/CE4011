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

        public override string GetTitle() => "";

        public override string GetSummary() => throw new NotImplementedException();

        public override string GetDetailedReportSection() => throw new NotImplementedException();

        public override double GetUtilizationRatio() => throw new NotImplementedException();

        #endregion
    }
}
