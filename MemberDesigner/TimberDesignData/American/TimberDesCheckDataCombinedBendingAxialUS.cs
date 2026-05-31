using System;
using System.Collections.Generic;
using System.Text;
using MemberDesigner.TimberDesignData.BaseClasses;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.TimberDesignData.American
{
    public class TimberDesCheckDataCombinedBendingAxialUS : TimberDesignCheckData
    {
        #region CTOR

        public TimberDesCheckDataCombinedBendingAxialUS()
        {
        }

        #endregion

        #region Public Properties

        public float CombinedTensionLimitMajor1 { get; set; }
        public float CombinedTensionLimitMinor1 { get; set; }

        public float CombinedTensionLimitMajor2 { get; set; }
        public float CombinedTensionLimitMinor2 { get; set; }

        public float CombinedCompressionLimit1 { get; set; }
        public float CombinedCompressionLimit2 { get; set; }

        public float TensionDemandStress { get; set; }
        public float AdjustedTensionDesignValue { get; set; }

        public float CompressionDemandStress { get; set; }
        public float CompressionDemandCapacity { get; set; }

        public float MajorDemandStressBending { get; set; }
        public float MinorDemandStressBending { get; set; }

        // Full reference bending design values (all applicable factors included)
        public float BendingDesignValueMajorAxis { get; set; }
        public float BendingDesignValueMinorAxis { get; set; }

        // Reference bending values excluding CL (lateral stability factor)
        public float BendingDesignValueMajorAxis_ExclCL { get; set; }
        public float BendingDesignValueMinorAxis_ExclCL { get; set; }

        // Reference bending values excluding Cv (volume factor)
        public float BendingDesignValueMajorAxis_ExclCV { get; set; }
        public float BendingDesignValueMinorAxis_ExclCV { get; set; }

        public float BucklingDesignStressMajor { get; set; }
        public float BucklingDesignStressMinor { get; set; }

        public float F_BE_Major { get; set; }
        public float F_BE_Minor { get; set; }

        #endregion

        #region Overrides

        public override eTimberDesignCheckType CheckType => eTimberDesignCheckType.CombinedBendingAxial;

        public override string GetTitle()
        {
            return "Combined Bending and Axial (US)";
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
            // Typically returns the maximum limit value calculated from the interaction equations
            throw new NotImplementedException();
        }

        #endregion
    }
}
