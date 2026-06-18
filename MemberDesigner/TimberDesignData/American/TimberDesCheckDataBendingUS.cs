using System;
using System.Collections.Generic;
using System.Text;
using MemberDesigner.TimberDesignData.BaseClasses;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.TimberDesignData.American
{
    public class TimberDesCheckDataBendingUS : TimberDesignCheckDataAmerican
    {
        #region CTOR

        public TimberDesCheckDataBendingUS()
        {
        }

        #endregion

        #region Public Properties - Bending Design Values

        public float FlatUseFactor { get; set; }
        public float FlatUseFactorEmin { get; set; }
        public float RepetitiveMemberFactor { get; set; }
        public float SlendernessRatioMajor { get; set; }
        public float SlendernessRatioMinor { get; set; }
        public bool SlendernessPassed { get; set; } = true;

        public float C_L_Major { get; set; }
        public float C_L_Minor { get; set; }

        // Full reference bending design values (all applicable factors included)
        public float BendingDesignValueMajorAxis { get; set; }
        public float BendingDesignValueMinorAxis { get; set; }

        // Reference bending values excluding CL (lateral stability factor)
        public float BendingDesignValueMajorAxis_ExclCL { get; set; }
        public float BendingDesignValueMinorAxis_ExclCL { get; set; }

        // Reference bending values excluding Cv (volume factor)
        public float BendingDesignValueMajorAxis_ExclCV { get; set; }
        public float BendingDesignValueMinorAxis_ExclCV { get; set; }

        public float F_BE_Major { get; set; }
        public float F_BE_Minor { get; set; }
        public float AdjustedMinModulus { get; set; }
        public float MajorDemandStress { get; set; }
        public float MinorDemandStress { get; set; }

        #endregion

        #region Overrides

        public override eTimberDesignCheckType CheckType => eTimberDesignCheckType.Bending;

        public override string GetTitle()
        {
            return "Bending Design (US)";
        }

        /// <summary>
        /// Plain-text value summary: governing formula, key values (2 dp, internal units),
        /// and the resulting utilization ratio.
        /// </summary>
        public override string GetSummary()
        {
            return string.Join(Environment.NewLine, new[]
            {
                "Formula: fb / Fb' ≤ 1.0   (checked on both axes; Fb' = Fb·CD·CM·Ct·CL·CF·Cr)",
                $"Major demand stress (fb,major): {MajorDemandStress:0.00}",
                $"Major bending capacity (Fb',major): {BendingDesignValueMajorAxis:0.00}",
                $"Minor demand stress (fb,minor): {MinorDemandStress:0.00}",
                $"Minor bending capacity (Fb',minor): {BendingDesignValueMinorAxis:0.00}",
                $"Beam stability factor (C_L major / minor): {C_L_Major:0.00} / {C_L_Minor:0.00}",
                $"Slenderness OK: {(SlendernessPassed ? "yes" : "no")}",
                $"Utilization (D/C): {GetUtilizationRatio():0.00}"
            });
        }

        /// <summary>
        /// Fully detached detailed report returning raw text
        /// </summary>
        public override string GetDetailedReportSection()
        {
            throw new NotImplementedException();
        }

        public override double GetUtilizationRatio()
        {
            double majorRatio = BendingDesignValueMajorAxis > 0 ? (MajorDemandStress / BendingDesignValueMajorAxis) : 0;
            double minorRatio = BendingDesignValueMinorAxis > 0 ? (MinorDemandStress / BendingDesignValueMinorAxis) : 0;

            return Math.Max(majorRatio, minorRatio);
        }

        #endregion
    }
}
