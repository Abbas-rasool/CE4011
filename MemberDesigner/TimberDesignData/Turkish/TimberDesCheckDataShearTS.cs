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

        public override string GetTitle() => "";

        public override string GetSummary() => throw new NotImplementedException();

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
