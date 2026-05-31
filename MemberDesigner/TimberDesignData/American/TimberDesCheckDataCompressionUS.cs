using System;
using System.Collections.Generic;
using System.Text;
using MemberDesigner.TimberDesignData.BaseClasses;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.TimberDesignData.American
{
    public class TimberDesCheckDataCompressionUS : TimberDesignCheckDataAmerican
    {
        #region CTOR

        public TimberDesCheckDataCompressionUS()
        {
        }

        #endregion

        #region Public Properties

        public float FlatUseFactorEmin { get; set; }
        public float RepetitiveMemberFactor { get; set; }
        public bool SlendernessPassed { get; set; } = true;
        public float MaterialAdjustmentFactor { get; set; }
        public float BearingAreaFactor { get; set; }
        public float EffectiveColumnLength { get; set; }
        public float AdjustedEmin { get; set; }
        public float SlendernessRatioMajor { get; set; }
        public float SlendernessRatioMinor { get; set; }
        public float SlendernessRatioConnectingMembers { get; set; }
        public float BucklingDesignStressMajor { get; set; }
        public float BucklingDesignStressMinor { get; set; }
        public float CriticalBucklingStress { get; set; }
        public float ColumnStabilityFactor { get; set; }
        public float GrossCompressionCapacity { get; set; }
        public float NetCompressionCapacity { get; set; }
        public float NetCompressionCapacityPerpendicular { get; set; }
        public float ParallelDemandGross { get; set; }
        public float ParallelDemandNet { get; set; }
        public float MaxPerpendicularDemand { get; set; }
        public float ResistanceFactor90 { get; set; }
        public float FormatConversionFactorF90 { get; set; }

        #endregion

        #region Overrides

        public override eTimberDesignCheckType CheckType => eTimberDesignCheckType.Compression;

        public override string GetTitle()
        {
            return "Compression Design (US)";
        }

        /// <summary>
        /// Detached summary returning a plain string.
        /// </summary>
        public override string GetSummary()
        {
            throw new NotImplementedException();
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
