using System;
using MemberDesigner.TimberDesignData.BaseClasses;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.TimberDesignData.Turkish
{
    public class TimberDesCheckDataShearTS : TimberDesignCheckData
    {
        #region CTOR

        public TimberDesCheckDataShearTS()
        {
        }

        #endregion

        #region Public Properties

        public float MaxShearDemand { get; set; }
        public float MaxRollingShearDemand { get; set; }
        public float ShearStrengthMaterial { get; set; }
        public float ShearStrengthRollingMaterial { get; set; }
        public float K_cr { get; set; }
        public float K_Shape { get; set; }
        public float EffectiveWidth { get; set; }
        public float TorsionalShearCapacity { get; set; }
        public float MaxTorsionStressDemand { get; set; }

        #endregion

        #region Overrides

        public override eTimberDesignCheckType CheckType => eTimberDesignCheckType.Shear;

        public override string GetTitle() => "Shear Design (TR)";

        /// <summary>
        /// Plain-text value summary: governing formula, key values (2 dp, internal units),
        /// and the resulting utilization ratio.
        /// </summary>
        public override string GetSummary()
        {
            return string.Join(Environment.NewLine, new[]
            {
                "Formula: τ / fv,d ≤ 1.0",
                $"Demand shear stress (τ): {MaxShearDemand:0.00}",
                $"Shear strength (fv,d): {ShearStrengthMaterial:0.00}",
                $"Crack factor (K_cr): {K_cr:0.00}",
                $"Rolling shear demand / strength: {MaxRollingShearDemand:0.00} / {ShearStrengthRollingMaterial:0.00}",
                $"Torsion demand / capacity: {MaxTorsionStressDemand:0.00} / {TorsionalShearCapacity:0.00}",
                $"Utilization (D/C): {GetUtilizationRatio():0.00}"
            });
        }

        public override string GetDetailedReportSection() => throw new NotImplementedException();

        public override double GetUtilizationRatio()
        {
            double shear = ShearStrengthMaterial > 0 ? MaxShearDemand / ShearStrengthMaterial : 0;
            double rolling = ShearStrengthRollingMaterial > 0 ? MaxRollingShearDemand / ShearStrengthRollingMaterial : 0;
            double torsion = TorsionalShearCapacity > 0 ? MaxTorsionStressDemand / TorsionalShearCapacity : 0;
            return Math.Max(shear, Math.Max(rolling, torsion));
        }

        #endregion
    }
}
