using System;
using MemberDesigner.TimberDesignData.BaseClasses;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.TimberDesignData.Turkish
{
    public class TimberDesCheckDataCombinedBendingAxialTS : TimberDesignCheckData
    {
        #region CTOR

        public TimberDesCheckDataCombinedBendingAxialTS()
        {
        }

        #endregion

        #region Public Properties

        public float C_E { get; set; }
        public float CriticalCompressionCapacity { get; set; }
        public float FirstTerm { get; set; }
        public float SecondTerm { get; set; }
        public float ThirdTerm { get; set; }
        public float MajorRatio { get; set; }
        public float MinorRatio { get; set; }

        #endregion

        #region Overrides

        public override eTimberDesignCheckType CheckType => eTimberDesignCheckType.CombinedBendingAxial;

        public override string GetTitle() => "Combined Bending and Axial (TR)";

        /// <summary>
        /// Plain-text value summary: governing interaction formula, key terms (2 dp), and the
        /// resulting utilization ratio.
        /// </summary>
        public override string GetSummary()
        {
            return string.Join(Environment.NewLine, new[]
            {
                "Formula: σc/fc,d + σm,major/fm,d + σm,minor/fm,d ≤ 1.0   (stability factor C_E)",
                $"Axial term: {FirstTerm:0.00}",
                $"Major bending term: {SecondTerm:0.00}",
                $"Minor bending term: {ThirdTerm:0.00}",
                $"Stability factor (C_E): {C_E:0.00}",
                $"Interaction ratio major / minor: {MajorRatio:0.00} / {MinorRatio:0.00}",
                $"Utilization (D/C): {GetUtilizationRatio():0.00}"
            });
        }

        public override string GetDetailedReportSection() => throw new NotImplementedException();

        public override double GetUtilizationRatio() => Math.Max(MajorRatio, MinorRatio);

        #endregion
    }
}
