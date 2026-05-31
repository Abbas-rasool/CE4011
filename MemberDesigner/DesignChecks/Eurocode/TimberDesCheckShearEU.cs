using MemberDesigner.DesignInputs.Eurocode;
using MemberDesigner.TimberDesignData.BaseClasses;
using MemberDesigner.TimberDesignData.Eurocode;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignChecks.Eurocode
{
    public class TimberDesCheckShearEU : ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData>
    {

        #region CTOR

        public TimberDesCheckShearEU()
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
            if (!(input is TimberShearCheckInputEU castedInput))
                throw new ArgumentException("Input argument is not of the correct type");

            var checkData = new TimberDesCheckDataShearEU();
            checkData.DesignStatus = eDesignStatus.Pass;

            var parameters = (TimberParametersCheckDataEU)dependencies.FirstOrDefault(x => x.CheckType == eTimberDesignCheckType.Parameters);

            float partialFactor = parameters.PartialFactor;
            float modificationFactor = parameters.ModificationFactor;

            double shearStrengthMaterial = (castedInput.Fv * modificationFactor) / partialFactor;
            double shearStrengthRollingMaterial = 2 * (castedInput.Ft90 * modificationFactor) / partialFactor;

            checkData.ShearStrengthMaterial = (float)shearStrengthMaterial;
            checkData.ShearStrengthRollingMaterial = (float)shearStrengthRollingMaterial;

            double k_cr = 1;
            if (castedInput.material == eTimberMaterialType.SolidTimber || castedInput.material == eTimberMaterialType.GluedLaminatedTimber)
                k_cr = 0.67;

            double largerDimension = Math.Max(castedInput.h1, castedInput.h2);
            double smallerDimension  = Math.Min(castedInput.h1, castedInput.h2);

            double effectiveWidth = largerDimension * k_cr;
            double shearArea = effectiveWidth * smallerDimension;

            checkData.ShearArea = (float)shearArea;
            checkData.ShearAreaRolling = castedInput.RollingShearEffectiveArea;

            // rectangular section

            checkData.MaxShearForce = castedInput.MaxShearDemand;
            checkData.MaxRollingShearForce = castedInput.MaxRollingShearDemand;

            double shearDemand = (3.0 / 2.0) * (castedInput.MaxShearDemand / shearArea);
            double rollingShearDemand = (3.0 / 2.0) * castedInput.MaxRollingShearDemand / castedInput.RollingShearEffectiveArea;

            checkData.MaxShearStress = (float)shearDemand;
            checkData.MaxRollingShearStress = (float)rollingShearDemand;

            // Torsional check for the member (it could be easily implemented here so no new class)
            double firstTerm = (1 + 0.15 * (largerDimension / smallerDimension));
            double k_shape = Math.Min(2, firstTerm);

            double torsionCapacity = k_shape * shearStrengthMaterial;
            checkData.TorsionShearCapacity = (float)torsionCapacity;

            checkData.K_cr = (float)k_cr;
            checkData.K_Shape = (float)k_shape;
            checkData.EffectiveWidth = (float)effectiveWidth;
            
            checkData.MaxTorsionStressDemand = (float)castedInput.MaxTorsionStressDemand;

            if (shearDemand > shearStrengthMaterial)
                checkData.DesignStatus = eDesignStatus.Fail;

            if (rollingShearDemand > shearStrengthRollingMaterial)
                checkData.DesignStatus = eDesignStatus.Fail;

            if (castedInput.MaxTorsionStressDemand > torsionCapacity)
                checkData.DesignStatus = eDesignStatus.Fail;

            return checkData;
        }
    }
}
