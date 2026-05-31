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

        public override string GetSummary()
        {
            return string.Join(Environment.NewLine, new[]
            {
                $"Design Material Strength (Shear) (f_v,d): {ShearStrengthMaterial}",
                $"Design Material Strength (Rolling Shear) (f_v,r,d): {ShearStrengthRollingMaterial}",
                $"Reduction Factor (K_cr): {K_cr}",
                $"Effective Width (b_ef): {EffectiveWidth}",
                $"Shear Area (A_v): {ShearArea}",
                $"Applied Shear Force (F_v): {MaxShearForce}",
                $"Major Demand Shear Stress (σ_v): {MaxShearStress}",
                $"Rolling Shear Area (A_v,r): {ShearAreaRolling}",
                $"Applied Rolling Shear Force (F_v,r): {MaxRollingShearForce}",
                $"Major Demand Rolling Shear Stress (σ_v,r): {MaxRollingShearStress}",
                $"Shape Reduction Factor (K_shape): {K_Shape}",
                $"Design Material Strength (Torsion) (f_tor,d): {TorsionShearCapacity}",
                $"Applied Torsion Stress (σ_tor,d): {MaxTorsionStressDemand}"
            });
        }

        public override string GetDetailedReportSection() => throw new NotImplementedException();

        public override double GetUtilizationRatio() => throw new NotImplementedException();

        #endregion
    }
}
