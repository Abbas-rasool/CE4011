using System;
using System.Collections.Generic;
using System.Text;
using MemberDesigner.TimberDesignData.BaseClasses;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.TimberDesignData.American
{
    public class TimberDesCheckDataServiceabilityUS : TimberDesignCheckDataAmerican
    {
        #region CTOR

        public TimberDesCheckDataServiceabilityUS()
        {
        }

        #endregion

        #region Overrides

        public override eTimberDesignCheckType CheckType => eTimberDesignCheckType.Serviceability;

        public override string GetTitle()
        {
            return "Serviceability Design (US)";
        }

        /// <summary>
        /// Detached summary returning a plain string.
        /// </summary>
        public override string GetSummary()
        {
            return "Serviceability (deflection) check — not evaluated in this version.";
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
            throw new NotImplementedException();
        }

        #endregion
    }
}
