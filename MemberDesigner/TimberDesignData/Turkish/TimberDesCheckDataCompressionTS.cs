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

        public override string GetTitle() => "";

        public override string GetSummary() => throw new NotImplementedException();

        public override string GetDetailedReportSection() => throw new NotImplementedException();

        public override double GetUtilizationRatio() => throw new NotImplementedException();

        #endregion
    }
}
