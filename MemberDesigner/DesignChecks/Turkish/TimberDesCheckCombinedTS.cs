using MemberDesigner.DesignInputs.Turkish;
using MemberDesigner.DesignChecks;
using MemberDesigner.DesignChecks.Helpers;
using MemberDesigner.TimberDesignData.BaseClasses;
using MemberDesigner.TimberDesignData.Turkish;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignChecks.Turkish
{
    public class TimberDesCheckCombinedTS : ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData>
    {

        #region CTOR

        public TimberDesCheckCombinedTS()
        {
            _dependencies = new List<eTimberDesignCheckType>
            {
                eTimberDesignCheckType.Bending,
                eTimberDesignCheckType.Compression,
                eTimberDesignCheckType.Parameters
            };
        }

        #endregion

        #region Private Fields
        private List<eTimberDesignCheckType> _dependencies;
        #endregion

        #region Public Fields
        public List<eTimberDesignCheckType> Dependencies { get => _dependencies; }
        public eTimberDesignCheckType CheckType => eTimberDesignCheckType.CombinedBendingAxial;

        #endregion

        /// <summary>
        /// This method checks the combined axial and bending capacity of a member.
        /// </summary>
        public TimberDesignCheckData PerformCheck(ITimberDesignCheckInput input, params TimberDesignCheckData[] dependencies)
        {
            if (!(input is TimberCombinedCheckInputTS castedInput))
                throw new ArgumentException("Input argument is not of the correct type");

            for (int i = 0; i < _dependencies.Count; i++)
            {
                if (!(dependencies.Any(x => x.CheckType == _dependencies[i])))
                    throw new ArgumentException("Dependency not provided");
            }

            var bendingCheckData = (TimberDesCheckDataBendingTS)dependencies.FirstOrDefault(x => x.CheckType == eTimberDesignCheckType.Bending);
            var compressionCheckData = (TimberDesCheckDataCompressionTS)dependencies.FirstOrDefault(x => x.CheckType == eTimberDesignCheckType.Compression);

            var checkData = new TimberDesCheckDataCombinedBendingAxialTS();
            TimberDesignHelperTS timberDesignHelper = TimberDesignHelperTS.GetInstance();
            checkData.DesignStatus = eDesignStatus.Pass;

            double C_E = 1.0;
            if (castedInput.material == eTimberMaterialType.SolidTimber)
                C_E = 0.70;

            checkData.C_E = (float)C_E;

            double criticalCompressionCapacity = Math.Min(compressionCheckData.MajorCapacity, compressionCheckData.MinorCapacity);
            checkData.CriticalCompressionCapacity = (float)criticalCompressionCapacity;

            double firstTerm = compressionCheckData.DemandStressParallel / criticalCompressionCapacity;
            double secondTerm = bendingCheckData.MajorDemandStress / bendingCheckData.MaterialStrength;
            double thirdTerm = bendingCheckData.MinorDemandStress / bendingCheckData.MaterialStrength;

            checkData.FirstTerm = (float)firstTerm;
            checkData.SecondTerm = (float)secondTerm;
            checkData.ThirdTerm = (float)thirdTerm;

            double majorRatio;
            double minorRatio;

            if (compressionCheckData.C_P_Major < 0.99)
            {
                majorRatio = firstTerm + secondTerm + C_E * thirdTerm;
                minorRatio = firstTerm + C_E * secondTerm + thirdTerm;
            }
            else
            {
                majorRatio = Math.Pow(firstTerm, 2) + secondTerm + C_E * thirdTerm;
                minorRatio = Math.Pow(firstTerm, 2) + C_E * secondTerm + thirdTerm;
            }

            checkData.MajorRatio = (float)majorRatio;
            checkData.MinorRatio = (float)minorRatio;

            if (majorRatio > 1 || minorRatio > 1)
                checkData.DesignStatus = eDesignStatus.Fail;

            return checkData;
        }
    }
}
