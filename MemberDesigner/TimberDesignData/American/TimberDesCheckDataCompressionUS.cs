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
        /// Plain-text value summary: governing formula, key values (2 dp, internal units),
        /// and the resulting utilization ratio.
        /// </summary>
        public override string GetSummary()
        {
            return string.Join(Environment.NewLine, new[]
            {
                "Formula: fc / Fc' ≤ 1.0   (Fc' includes the column-stability factor C_P)",
                $"Parallel demand stress (gross): {ParallelDemandGross:0.00}",
                $"Gross compression capacity (Fc'): {GrossCompressionCapacity:0.00}",
                $"Net compression capacity: {NetCompressionCapacity:0.00}",
                $"Column-stability factor (C_P): {ColumnStabilityFactor:0.00}",
                $"Slenderness (major / minor): {SlendernessRatioMajor:0.00} / {SlendernessRatioMinor:0.00}",
                $"Perpendicular demand / capacity: {MaxPerpendicularDemand:0.00} / {NetCompressionCapacityPerpendicular:0.00}",
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
            // Governing of: parallel-to-grain on gross (with C_P) and net sections, and
            // perpendicular-to-grain bearing. Guard against zero/non-finite capacities.
            double gross = GrossCompressionCapacity > 0 ? ParallelDemandGross / GrossCompressionCapacity : 0;
            double net = NetCompressionCapacity > 0 ? ParallelDemandNet / NetCompressionCapacity : 0;
            double perp = NetCompressionCapacityPerpendicular > 0 ? MaxPerpendicularDemand / NetCompressionCapacityPerpendicular : 0;

            double ratio = Math.Max(gross, Math.Max(net, perp));
            return double.IsFinite(ratio) && ratio > 0 ? ratio : 0;
        }

        #endregion
    }
}
