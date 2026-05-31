using System;
using MemberDesigner.TimberDesignData.BaseClasses;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.TimberDesignData.Turkish
{
    public class TimberDesCheckDataCombinedBendingAxialTS : TimberDesignCheckData
    {
        #region CTOR

        public TimberDesCheckDataCombinedBendingAxialTS()
        {
        }

        #endregion

        #region Public Properties

        public float C_E { get; set; }
        public float CriticalCompressionCapacity { get; set; }
        public float FirstTerm { get; set; }
        public float SecondTerm { get; set; }
        public float ThirdTerm { get; set; }
        public float MajorRatio { get; set; }
        public float MinorRatio { get; set; }

        #endregion

        #region Overrides

        public override eTimberDesignCheckType CheckType => eTimberDesignCheckType.CombinedBendingAxial;

        public override string GetTitle() => "";

        public override string GetSummary() => throw new NotImplementedException();

        public override string GetDetailedReportSection() => throw new NotImplementedException();

        public override double GetUtilizationRatio() => throw new NotImplementedException();

        #endregion
    }
}
