using System;
using System.Collections.Generic;
using System.Text;
using MemberDesigner.TimberDesignData.BaseClasses;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.TimberDesignData.American
{
   public class TimberDesCheckDataShearUS : TimberDesignCheckDataAmerican
    {
        #region CTOR

        public TimberDesCheckDataShearUS()
        {
        }

        #endregion

        #region Public Properties

        public float MemberShearCapacityMat { get; set; }
        public float MaxShearDemand { get; set; }

        #endregion

        #region Overrides

        public override eTimberDesignCheckType CheckType => eTimberDesignCheckType.Shear;

        public override string GetTitle()
        {
            return "Shear Design (US)";
        }

        /// <summary>
        /// Detached summary returning a plain string.
        /// </summary>
        public override string GetSummary()
        {
            return string.Join(Environment.NewLine, new[]
            {
                "Formula: fv = 1.5·V/A ≤ Fv'",
                $"Max shear demand stress (fv): {MaxShearDemand:0.00}",
                $"Shear capacity (Fv'): {MemberShearCapacityMat:0.00}",
                $"Utilization (D/C): {GetUtilizationRatio():0.00}"
            });
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
            // Simple utilization logic: Demand over Capacity
            return MemberShearCapacityMat > 0 ? (MaxShearDemand / MemberShearCapacityMat) : 0;
        }

        #endregion
    }
}
