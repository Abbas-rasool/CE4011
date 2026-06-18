using System;
using MemberDesigner.TimberDesignData.BaseClasses;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.TimberDesignData.Turkish
{
    public class TimberDesCheckDataCompressionTS : TimberDesignCheckData
    {
        #region CTOR

        public TimberDesCheckDataCompressionTS()
        {
        }

        #endregion

        #region Public Properties

        // Input-related properties
        public float BucklingLengthMajor { get; set; }
        public float BucklingLengthMinor { get; set; }
        public float SlendernessMajor { get; set; }
        public float SlendernessMinor { get; set; }

        // Calculated factors
        public float C_B { get; set; }
        public float C_P_Major { get; set; }
        public float C_P_Minor { get; set; }
        public float C_P_90 { get; set; }
        public float C { get; set; }

        public float F_E_Major { get; set; }
        public float F_E_Minor { get; set; }

        // Material strengths
        public float MaterialStrengthParallel { get; set; }
        public float MaterialStrengthPerp { get; set; } // Perpendicular strength

        // Calculated capacities
        public float MajorCapacity { get; set; }
        public float MinorCapacity { get; set; }
        public float AngledCapacity { get; set; }

        // Demand and stress values
        public float DemandStressParallel { get; set; }
        public float DemandStressPerpendicular { get; set; }
        public float AngledDemandStress { get; set; }
        public float AngledStressAppliedArea { get; set; }

        #endregion

        #region Overrides

        public override eTimberDesignCheckType CheckType => eTimberDesignCheckType.Compression;

        public override string GetTitle() => "Compression Design (TR)";

        /// <summary>
        /// Plain-text value summary: governing formula, key values (2 dp, internal units),
        /// and the resulting utilization ratio.
        /// </summary>
        public override string GetSummary()
        {
            return string.Join(Environment.NewLine, new[]
            {
                "Formula: σc / fc,d ≤ 1.0   (fc,d includes the stability factor C_P)",
                $"Demand stress parallel (σc): {DemandStressParallel:0.00}",
                $"Major capacity (fc,d major): {MajorCapacity:0.00}",
                $"Minor capacity (fc,d minor): {MinorCapacity:0.00}",
                $"Stability factor (C_P major / minor): {C_P_Major:0.00} / {C_P_Minor:0.00}",
                $"Slenderness (major / minor): {SlendernessMajor:0.00} / {SlendernessMinor:0.00}",
                $"Perpendicular demand / strength: {DemandStressPerpendicular:0.00} / {MaterialStrengthPerp:0.00}",
                $"Utilization (D/C): {GetUtilizationRatio():0.00}"
            });
        }

        public override string GetDetailedReportSection() => throw new NotImplementedException();

        public override double GetUtilizationRatio()
        {
            double major = MajorCapacity > 0 ? DemandStressParallel / MajorCapacity : 0;
            double minor = MinorCapacity > 0 ? DemandStressParallel / MinorCapacity : 0;
            double perp = MaterialStrengthPerp > 0 ? DemandStressPerpendicular / MaterialStrengthPerp : 0;
            double angled = AngledCapacity > 0 ? AngledDemandStress / AngledCapacity : 0;
            return Math.Max(Math.Max(major, minor), Math.Max(perp, angled));
        }

        #endregion
    }
}
