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
        /// Plain-text value summary: governing interaction formula, key values (2 dp, internal
        /// units), and the resulting utilization ratio.
        /// </summary>
        public override string GetSummary()
        {
            bool compression = CompressionDemandStress > 0f;
            return string.Join(Environment.NewLine, new[]
            {
                compression
                    ? "Formula: (fc/Fc')² + fb1/[Fb1'(1 − fc/FcE1)] + fb2/[…] ≤ 1.0   (NDS Eq. 3.9-3)"
                    : "Formula: ft/Ft' + fb/Fb* ≤ 1.0   and   (fb − ft)/Fb** ≤ 1.0   (NDS 3.9.1)",
                $"Tension demand / capacity: {TensionDemandStress:0.00} / {AdjustedTensionDesignValue:0.00}",
                $"Compression demand / capacity: {CompressionDemandStress:0.00} / {CompressionDemandCapacity:0.00}",
                $"Major bending demand / capacity: {MajorDemandStressBending:0.00} / {BendingDesignValueMajorAxis:0.00}",
                $"Minor bending demand / capacity: {MinorDemandStressBending:0.00} / {BendingDesignValueMinorAxis:0.00}",
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
            // The governing NDS interaction value: the compression+bending equations (3.9.2) for a
            // member with axial compression, otherwise the tension+bending equations (3.9.1). Only
            // finite, positive limits are considered (degenerate cases collapse to ~0).
            double ratio = CompressionDemandStress > 0f
                ? Largest(CombinedCompressionLimit1, CombinedCompressionLimit2)
                : Largest(CombinedTensionLimitMajor1, CombinedTensionLimitMinor1,
                          CombinedTensionLimitMajor2, CombinedTensionLimitMinor2);

            return ratio;
        }

        private static double Largest(params float[] limits)
        {
            double max = 0;
            foreach (float v in limits)
                if (double.IsFinite(v) && v > max) max = v;
            return max;
        }

        #endregion
    }
}
