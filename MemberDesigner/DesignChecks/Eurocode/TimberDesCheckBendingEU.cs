using MemberDesigner.DesignInputs.Eurocode;
using MemberDesigner.TimberDesignData.BaseClasses;
using MemberDesigner.TimberDesignData.Eurocode;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignChecks.Eurocode
{
    public class TimberDesCheckBendingEU : ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData>
    {

        #region CTOR

        public TimberDesCheckBendingEU()
        {
            _dependencies = new List<eTimberDesignCheckType>() { eTimberDesignCheckType.Parameters };
        }

        #endregion

        #region Private Fields
        private List<eTimberDesignCheckType> _dependencies;
        #endregion

        #region Public Fields
        public List<eTimberDesignCheckType> Dependencies { get => _dependencies; }
        public eTimberDesignCheckType CheckType => eTimberDesignCheckType.Bending;

        #endregion

        /// <summary>
        /// This method checks the bending capacity of a member.
        /// </summary>
        public TimberDesignCheckData PerformCheck(ITimberDesignCheckInput input, params TimberDesignCheckData[] dependencies)
        {
            if (!(input is TimberBendingCheckInputEU castedInput))
                throw new ArgumentException("Input argument is not of the correct type");

            var checkData = new TimberDesCheckDataBendingEU();
            checkData.DesignStatus = eDesignStatus.Pass;

            var parameters = (TimberParametersCheckDataEU)dependencies.FirstOrDefault(x => x.CheckType == eTimberDesignCheckType.Parameters);

            float partialFactor = parameters.PartialFactor;
            float modificationFactor = parameters.ModificationFactor;

            double materialStrength = (castedInput.Fm * modificationFactor) / partialFactor;
            checkData.DesignMatStrength = (float)materialStrength;

            // Demand stresses (rectangular section assumption)
            checkData.MajorDemandMoment = castedInput.MaxDemandMomentMajor;
            checkData.MinorDemandMoment = castedInput.MaxDemandMomentMinor;

            double MajorDemandStress = (6 * castedInput.MaxDemandMomentMajor) /
                                       (castedInput.h2 * Math.Pow(castedInput.h1, 2));

            double MinorDemandStress = (6 * castedInput.MaxDemandMomentMinor) /
                                       (castedInput.h1 * Math.Pow(castedInput.h2, 2));

            checkData.MajorDemandStress = (float)MajorDemandStress;
            checkData.MinorDemandStress = (float)MinorDemandStress;

            // (rectangular section assumption)
            double k_m = 1;
            if (castedInput.material == eTimberMaterialType.SolidTimber || castedInput.material == eTimberMaterialType.GluedLaminatedTimber)
            {
                k_m = 0.7;
            }
            checkData.KmInteractionFactor = (float)k_m;

            double MajorCheck = (MajorDemandStress / materialStrength) + k_m * (MinorDemandStress / materialStrength);
            double MinorCheck = (MinorDemandStress / materialStrength) + k_m * (MajorDemandStress / materialStrength);

            checkData.MajorCheckFactor = (float)MajorCheck;
            checkData.MinorCheckFactor = (float)MinorCheck;

            if (MajorCheck > 1 || MinorCheck > 1)
            {
                checkData.DesignStatus = eDesignStatus.Fail;
            }

            return checkData;
        }

    }
}
