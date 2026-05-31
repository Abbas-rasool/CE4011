using MemberDesigner.DesignInputs.American;
using MemberDesigner.DesignChecks;
using MemberDesigner.TimberDesignData.American;
using MemberDesigner.TimberDesignData.BaseClasses;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignChecks.American
{
    public class TimberDesCheckCombinedBendingAxialUS : ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData>
    {

        #region CTOR
        public TimberDesCheckCombinedBendingAxialUS()
        {
            _dependencies = new List<eTimberDesignCheckType>
        {
            eTimberDesignCheckType.Bending,
            eTimberDesignCheckType.Tension,
            eTimberDesignCheckType.Compression
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
        /// This check control the combined bending and axial loading.
        /// </summary>
        public TimberDesignCheckData PerformCheck(ITimberDesignCheckInput input, params TimberDesignCheckData[] dependencies)
        {
            if (!(input is TimberCombinedBendingAxialCheckInputUS castedInput))
                throw new ArgumentException("Input argument is not of the correct type");

            for (int i = 0; i < _dependencies.Count; i++)
            {
                if (!(dependencies.Any(x => x.CheckType == _dependencies[i])))
                    throw new ArgumentException("Dependency not provided");
            }

            var bendingCheckData = (TimberDesCheckDataBendingUS)dependencies.FirstOrDefault(x => x.CheckType == eTimberDesignCheckType.Bending);
            var tensionCheckData = (TimberDesCheckDataTensionUS)dependencies.FirstOrDefault(x => x.CheckType == eTimberDesignCheckType.Tension);
            var compressionCheckData = (TimberDesCheckDataCompressionUS)dependencies.FirstOrDefault(x => x.CheckType == eTimberDesignCheckType.Compression);

            var checkData = new TimberDesCheckDataCombinedBendingAxialUS();
            checkData.DesignStatus = eDesignStatus.Pass;
            bool isFailed = false;

            // if one of the major checks fail, there's no point to check the member for interaction failure.
            if (bendingCheckData.DesignStatus == eDesignStatus.Fail || tensionCheckData.DesignStatus == eDesignStatus.Fail || compressionCheckData.DesignStatus == eDesignStatus.Fail)
            {
                checkData.DesignStatus = eDesignStatus.Fail;
                return checkData;
            }


            checkData.AdjustedTensionDesignValue = tensionCheckData.AdjustedTensionDesignValue;
            checkData.TensionDemandStress = tensionCheckData.TensionDemandStress;

            checkData.CompressionDemandStress = compressionCheckData.ParallelDemandGross;
            checkData.CompressionDemandCapacity = compressionCheckData.GrossCompressionCapacity;

            checkData.BucklingDesignStressMajor = compressionCheckData.BucklingDesignStressMajor;
            checkData.BucklingDesignStressMinor = compressionCheckData.BucklingDesignStressMinor;

            checkData.MajorDemandStressBending = bendingCheckData.MajorDemandStress;
            checkData.MinorDemandStressBending = bendingCheckData.MinorDemandStress;

            checkData.BendingDesignValueMajorAxis = bendingCheckData.BendingDesignValueMajorAxis;
            checkData.BendingDesignValueMinorAxis = bendingCheckData.BendingDesignValueMinorAxis;

            checkData.BendingDesignValueMajorAxis_ExclCL = bendingCheckData.BendingDesignValueMajorAxis_ExclCL;
            checkData.BendingDesignValueMinorAxis_ExclCL = bendingCheckData.BendingDesignValueMinorAxis_ExclCL;

            checkData.BendingDesignValueMajorAxis_ExclCV = bendingCheckData.BendingDesignValueMajorAxis_ExclCV;
            checkData.BendingDesignValueMinorAxis_ExclCV = bendingCheckData.BendingDesignValueMinorAxis_ExclCV;

            checkData.F_BE_Major = bendingCheckData.F_BE_Major;
            checkData.F_BE_Minor = bendingCheckData.F_BE_Minor;

            // Configuration type dispatch
            switch (castedInput.memberConfigurationType)
            {
                case eMemberConfigurationType.SolidMembers:
                case eMemberConfigurationType.NotSpacedCombinedColumns:
                    isFailed = PerformSolidMemberChecks(castedInput, checkData);
                    break;

                case eMemberConfigurationType.SpacedColumns:
                    isFailed = PerformSpacedColumnChecks(castedInput, checkData);
                    break;

                default:

                    // Unknown configuration
                    checkData.DesignStatus = eDesignStatus.Fail;
                    break;
            }

            if (isFailed)
            {
                checkData.DesignStatus = eDesignStatus.Fail;
            }

            return checkData;
        }

        #region Private Methods

        /// <summary>
        /// Checks the multi loading cases for Solid members.
        /// </summary>
        private bool PerformSolidMemberChecks(TimberCombinedBendingAxialCheckInputUS castedInput, TimberDesCheckDataCombinedBendingAxialUS checkData)
        {
            bool isFailed = false;

            // Combined Tension and Bending Checks

            double combinedTensionLimitMajor1 = (checkData.TensionDemandStress / checkData.AdjustedTensionDesignValue) + (checkData.MajorDemandStressBending / checkData.BendingDesignValueMajorAxis_ExclCL);
            double combinedTensionLimitMinor1 = (checkData.TensionDemandStress / checkData.AdjustedTensionDesignValue) + (checkData.MinorDemandStressBending / checkData.BendingDesignValueMinorAxis_ExclCL);

            checkData.CombinedTensionLimitMajor1 = (float)combinedTensionLimitMajor1;
            checkData.CombinedTensionLimitMinor1 = (float)combinedTensionLimitMinor1;

            if (combinedTensionLimitMajor1 > 1 || combinedTensionLimitMinor1 > 1)
            {
                isFailed = true;
            }

            double combinedTensionLimitMajor2 = (checkData.MajorDemandStressBending - checkData.TensionDemandStress) / checkData.BendingDesignValueMajorAxis_ExclCV;
            double combinedTensionLimitMinor2 = (checkData.MinorDemandStressBending - checkData.TensionDemandStress) / checkData.BendingDesignValueMinorAxis_ExclCV;

            checkData.CombinedTensionLimitMajor2 = (float)combinedTensionLimitMajor2;
            checkData.CombinedTensionLimitMinor2 = (float)combinedTensionLimitMinor2;

            if (combinedTensionLimitMajor2 > 1 || combinedTensionLimitMinor2 > 1)
            {
                isFailed = true;
            }

            // Combined Compression and Bending Checks

            double term1 = Math.Pow((checkData.CompressionDemandStress / checkData.CompressionDemandCapacity), 2);
            double term2 = checkData.MajorDemandStressBending / (checkData.BendingDesignValueMajorAxis * (1 - (checkData.CompressionDemandStress / checkData.BucklingDesignStressMajor)));
            double term3 = checkData.MinorDemandStressBending / (checkData.BendingDesignValueMinorAxis * (1 - (checkData.CompressionDemandStress / checkData.BucklingDesignStressMinor) - Math.Pow((checkData.MajorDemandStressBending / checkData.F_BE_Major), 2)));

            double combinedCompressionLimit1 = term1 + term2 + term3;
            double combinedcompressionLimit2 = (checkData.CompressionDemandStress / checkData.BucklingDesignStressMinor) + Math.Pow((checkData.MajorDemandStressBending / checkData.F_BE_Major), 2);

            checkData.CombinedCompressionLimit1 = (float)combinedCompressionLimit1;
            checkData.CombinedCompressionLimit2 = (float)combinedcompressionLimit2;

            if (castedInput.MaxDemandMomentMajor != 0 && castedInput.MaxDemandMomentMinor != 0)
            {
                if (checkData.MajorDemandStressBending > checkData.F_BE_Major || checkData.MinorDemandStressBending > checkData.F_BE_Minor)
                {
                    isFailed = true;
                }
            }
            else if (castedInput.MaxDemandMomentMinor != 0)
            {
                if (checkData.CompressionDemandStress > checkData.BucklingDesignStressMinor)
                {
                    isFailed = true;
                }
            }
            else if (castedInput.MaxDemandMomentMajor != 0)
            {
                if (checkData.CompressionDemandStress > checkData.BucklingDesignStressMajor)
                {
                    isFailed = true;
                }
            }

            if (combinedCompressionLimit1 > 1 || combinedcompressionLimit2 > 1)
            {
                isFailed = true;
            }

            return isFailed;
        }

        /// <summary>
        /// Checks the multi loading cases for spaced columns.
        /// </summary>
        private bool PerformSpacedColumnChecks(TimberCombinedBendingAxialCheckInputUS castedInput, TimberDesCheckDataCombinedBendingAxialUS checkData)
        {
            bool isFailed = false;

            double combinedTensionLimitMajor1 = (checkData.TensionDemandStress / checkData.AdjustedTensionDesignValue) + (checkData.MajorDemandStressBending / checkData.BendingDesignValueMajorAxis_ExclCL);
            checkData.CombinedTensionLimitMajor1 = (float)combinedTensionLimitMajor1;

            if (combinedTensionLimitMajor1 > 1)
            {
                isFailed = true;
            }

            double combinedTensionLimitMajor2 = (checkData.MajorDemandStressBending - checkData.TensionDemandStress) / checkData.BendingDesignValueMajorAxis_ExclCV;
            checkData.CombinedTensionLimitMajor2 = (float)combinedTensionLimitMajor2;

            if (combinedTensionLimitMajor2 > 1)
            {
                isFailed = true;
            }

            // Combined Compression and Bending Checks

            double term1 = Math.Pow((checkData.CompressionDemandStress / checkData.CompressionDemandCapacity), 2);
            double term2 = checkData.MajorDemandStressBending / (checkData.BendingDesignValueMajorAxis * (1 - (checkData.CompressionDemandStress / checkData.BucklingDesignStressMajor)));
            double term3 = checkData.MinorDemandStressBending / (checkData.BendingDesignValueMinorAxis * (1 - (checkData.CompressionDemandStress / checkData.BucklingDesignStressMinor) - Math.Pow((checkData.MajorDemandStressBending / checkData.F_BE_Major), 2)));

            double combinedCompressionLimit1 = term1 + term2 + term3;
            double combinedcompressionLimit2 = (checkData.CompressionDemandStress / checkData.BucklingDesignStressMinor) + Math.Pow((checkData.MajorDemandStressBending / checkData.F_BE_Major), 2);

            checkData.CombinedCompressionLimit1 = (float)combinedCompressionLimit1;
            checkData.CombinedCompressionLimit2 = (float)combinedcompressionLimit2;

            if (castedInput.MaxDemandMomentMajor != 0)
            {
                if (checkData.CompressionDemandStress > checkData.BucklingDesignStressMajor)
                {
                    isFailed = true;
                }
            }

            if (combinedCompressionLimit1 > 1 || combinedcompressionLimit2 > 1)
            {
                isFailed = true;
            }

            return isFailed;
        }

        #endregion

    }
}
