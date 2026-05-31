using System;
using MemberDesigner.TimberDesignData.BaseClasses;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.TimberDesignData.Eurocode
{
    public class TimberParametersCheckDataEU : TimberDesignCheckData
    {
        #region CTOR

        public TimberParametersCheckDataEU()
        {
        }

        #endregion

        #region Public Properties

        public float PartialFactor { get; set; }
        public float ModificationFactor { get; set; }

        #endregion

        #region Overrides

        public override eTimberDesignCheckType CheckType => eTimberDesignCheckType.Parameters;

        public override string GetTitle() => "EC5 Design Parameters";

        public override string GetSummary()
        {
            return string.Join(Environment.NewLine, new[]
            {
                $"Material Properties Partial Factor (y_M): {PartialFactor}",
                $"Modification Factor (k_mod): {ModificationFactor}"
            });
        }

        public override string GetDetailedReportSection() => throw new NotImplementedException();

        public override double GetUtilizationRatio() => throw new NotImplementedException();

        #endregion
    }
}
