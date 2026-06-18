using System;
using MemberDesigner.TimberDesignData.BaseClasses;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.TimberDesignData.Eurocode
{
    public class TimberDesCheckDataShearEU : TimberDesignCheckData
    {
        #region CTOR

        public TimberDesCheckDataShearEU()
        {
        }

        #endregion

        #region Public Properties

        // Shear strength values
        public float ShearStrengthMaterial { get; set; }
        public float ShearStrengthRollingMaterial { get; set; }

        // Effective dimensions and factors
        public float EffectiveWidth { get; set; }
        public float ShearArea { get; set; }
        public float ShearAreaRolling { get; set; }
        public float K_cr { get; set; }
        public float K_Shape { get; set; }

        // Shear capacities
        public float TorsionShearCapacity { get; set; }

        // Demands
        public float MaxShearForce { get; set; }
        public float MaxRollingShearForce { get; set; }
        public float MaxShearStress { get; set; }
        public float MaxRollingShearStress { get; set; }
        public float MaxTorsionStressDemand { get; set; }

        #endregion

        #region Overrides

        public override eTimberDesignCheckType CheckType => eTimberDesignCheckType.Shear;

        public override string GetTitle() => "Design Shear Check";

        /// <summary>
        /// Plain-text value summary: governing formula, key values (2 dp, internal units),
        /// and the resulting utilization ratio.
        /// </summary>
        public override string GetSummary()
        {
            return string.Join(Environment.NewLine, new[]
            {
                "Formula: τd / fv,d ≤ 1.0   (fv,d = kmod·fv,k / γM; b_ef = kcr·b)",
                $"Design shear strength (fv,d): {ShearStrengthMaterial:0.00}",
                $"Crack factor (kcr): {K_cr:0.00}",
                $"Effective width (b_ef): {EffectiveWidth:0.00}",
                $"Demand shear stress (τd): {MaxShearStress:0.00}",
                $"Rolling shear demand / strength: {MaxRollingShearStress:0.00} / {ShearStrengthRollingMaterial:0.00}",
                $"Torsion demand / capacity: {MaxTorsionStressDemand:0.00} / {TorsionShearCapacity:0.00}",
                $"Utilization (D/C): {GetUtilizationRatio():0.00}"
            });
        }

        public override string GetDetailedReportSection() => throw new NotImplementedException();

        public override double GetUtilizationRatio()
        {
            double shear = ShearStrengthMaterial > 0 ? MaxShearStress / ShearStrengthMaterial : 0;
            double rolling = ShearStrengthRollingMaterial > 0 ? MaxRollingShearStress / ShearStrengthRollingMaterial : 0;
            double torsion = TorsionShearCapacity > 0 ? MaxTorsionStressDemand / TorsionShearCapacity : 0;
            return Math.Max(shear, Math.Max(rolling, torsion));
        }

        #endregion
    }
}
