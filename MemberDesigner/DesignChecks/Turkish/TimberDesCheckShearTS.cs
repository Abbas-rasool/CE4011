using MemberDesigner.DesignInputs.Turkish;
using MemberDesigner.DesignChecks;
using MemberDesigner.TimberDesignData.BaseClasses;
using MemberDesigner.TimberDesignData.Turkish;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignChecks.Turkish
{
    public class TimberDesCheckShearTS : ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData>
    {
        #region CTOR

        public TimberDesCheckShearTS()
        {
            _dependencies = new List<eTimberDesignCheckType>() { eTimberDesignCheckType.Parameters };
        }

        #endregion

        #region Private Fields
        private List<eTimberDesignCheckType> _dependencies;
        #endregion

        #region Public Fields
        public List<eTimberDesignCheckType> Dependencies { get => _dependencies; }
        public eTimberDesignCheckType CheckType => eTimberDesignCheckType.Shear;

        #endregion

        /// <summary>
        /// This method checks the shear capacity of a member.
        /// </summary>
        public TimberDesignCheckData PerformCheck(ITimberDesignCheckInput input, params TimberDesignCheckData[] dependencies)
        {
            if (!(input is TimberShearCheckInputTS castedInput))
                throw new ArgumentException("Input argument is not of the correct type");

            var checkData = new TimberDesCheckDataShearTS();
            checkData.DesignStatus = eDesignStatus.Pass;

            double thickness = Math.Min(castedInput.h1, castedInput.h2);
            double width = Math.Max(castedInput.h1, castedInput.h2);

            var parameters = (TimberParametersCheckDataTS)dependencies.FirstOrDefault(x => x.CheckType == eTimberDesignCheckType.Parameters);

            float C_N = parameters.C_N;
            float C_B = parameters.C_B;
            float C_Y = parameters.C_Y;
            float omega = parameters.Omega;

            double shearStrengthMaterial = (castedInput.Fv * C_N * C_Y) / omega;
            double shearStrengthRollingMaterial = 2 * (castedInput.Ft90 * C_N * C_Y) / omega;

            checkData.ShearStrengthMaterial = (float)shearStrengthMaterial;
            checkData.ShearStrengthRollingMaterial = (float)shearStrengthRollingMaterial;

            double k_cr = 1;
            if (castedInput.Material == eTimberMaterialType.SolidTimber || castedInput.Material == eTimberMaterialType.GluedLaminatedTimber)
                k_cr = 0.67;

            double effectiveWidth = width * k_cr;

            // for rectangular section
            double shearDemandStress = (3.0 * castedInput.MaxShearDemand) / (2.0 * thickness * effectiveWidth);
            checkData.MaxShearDemand = (float)shearDemandStress;

            // for simplified and conservative approach.
            double rollingShearDemand = castedInput.MaxRollingShearDemand / castedInput.RollingShearEffectiveArea;
            checkData.MaxRollingShearDemand = (float)rollingShearDemand;

            // Torsional check for the member (it could be easily implemented here so no new class)
            double k_shape = Math.Min((1 + 0.05 * width / thickness), 1.3);

            double torsionCapacity = k_shape * shearStrengthMaterial;

            checkData.K_cr = (float)k_cr;
            checkData.K_Shape = (float)k_shape;
            checkData.EffectiveWidth = (float)effectiveWidth;
            checkData.TorsionalShearCapacity = (float)(torsionCapacity);

            checkData.MaxTorsionStressDemand = (float)castedInput.MaxTorsionStressDemand;

            if (shearDemandStress > shearStrengthMaterial)
                checkData.DesignStatus = eDesignStatus.Fail;

            if (rollingShearDemand > shearStrengthRollingMaterial)
                checkData.DesignStatus = eDesignStatus.Fail;

            if (castedInput.MaxTorsionStressDemand > torsionCapacity)
                checkData.DesignStatus = eDesignStatus.Fail;

            return checkData;
        }
    }
}
