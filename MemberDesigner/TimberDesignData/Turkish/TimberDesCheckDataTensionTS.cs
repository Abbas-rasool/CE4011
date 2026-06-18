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

        public override string GetTitle() => "";

        public override string GetSummary() => throw new NotImplementedException();

        public override string GetDetailedReportSection() => throw new NotImplementedException();

        public override double GetUtilizationRatio()
            => MaterialResistance > 0 ? DemandStress / MaterialResistance : 0;

        #endregion
    }
}
