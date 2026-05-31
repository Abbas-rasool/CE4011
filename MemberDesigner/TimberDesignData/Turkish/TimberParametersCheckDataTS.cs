using System;
using MemberDesigner.TimberDesignData.BaseClasses;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.TimberDesignData.Turkish
{
    public class TimberParametersCheckDataTS : TimberDesignCheckData
    {
        #region CTOR

        public TimberParametersCheckDataTS()
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Size effect coefficient.
        /// </summary>
        public float S { get; set; }
        public float C_N { get; set; }
        public float C_Y { get; set; }
        public float C_B { get; set; }
        public float Omega { get; set; }

        #endregion

        #region Overrides

        public override eTimberDesignCheckType CheckType => eTimberDesignCheckType.Parameters;

        public override string GetTitle() => "Parameters";

        public override string GetSummary()
        {
            return string.Join(Environment.NewLine, new[]
            {
                $"Size Factor: {S}",
                $"C_N: {C_N}",
                $"C_Y: {C_Y}",
                $"C_B: {C_B}",
                $"Omega: {Omega}"
            });
        }

        public override string GetDetailedReportSection() => throw new NotImplementedException();

        public override double GetUtilizationRatio() => throw new NotImplementedException();

        #endregion
    }
}
