using MemberDesigner.DesignInputs.American;
using System;
using System.Collections.Generic;
using System.Text;
using MemberDesigner.DesignChecks;
using MemberDesigner.DesignInputs.Eurocode;
using MemberDesigner.DesignInputs.Turkish;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.Designers
{
    public class TimberDesignCheckInputFactory
    {
        #region CTOR

        public TimberDesignCheckInputFactory(TimberCheckTypeProvider checkTypeProvider)
        {
            // Later I will have to set it using the data i get from MD!
            _TimberCode = eTimberCode.EC5;

            _checkTypeProvider = checkTypeProvider;
        }

        #endregion

        #region Fields

        protected TimberCheckTypeProvider _checkTypeProvider;
        private eTimberCode _TimberCode;

        #endregion

        #region Private Methods
        protected ITimberDesignCheckInput PrepareCheckInput(eTimberDesignCheckType checkType)
        {
            if (_TimberCode == eTimberCode.US)
            {
                return PrepareCheckInputUS(checkType);
            }
            else if (_TimberCode == eTimberCode.TR)
            {
                return PrepareCheckInputTR(checkType);
            }
            else if (_TimberCode == eTimberCode.EC5)
            {
                return PrepareCheckInputEC(checkType);
            }

            return default;
        }

        private ITimberDesignCheckInput PrepareCheckInputEC(eTimberDesignCheckType checkType)
        {
            switch (checkType)
            {
                case eTimberDesignCheckType.Parameters:
                    var parameters = PrepareParametersCheckInputEC();
                    return parameters;

                case eTimberDesignCheckType.Tension:
                    var tensionInputs = PrepareTensionCheckInputEC();
                    return tensionInputs;

                case eTimberDesignCheckType.Compression:
                    var compressionInputs = PrepareCompressionCheckInputEC();
                    return compressionInputs;

                case eTimberDesignCheckType.Bending:
                    var bendingInputs = PrepareBendingCheckInputEC();
                    return bendingInputs;

                case eTimberDesignCheckType.Shear:
                    var shearInputs = PrepareShearCheckInputEC();
                    return shearInputs;

                case eTimberDesignCheckType.CombinedBendingAxial:
                    var combinedInputs = PrepareCombinedBendingAxialCheckInputEC();
                    return combinedInputs;

                default:
                    break;
            }

            return null;
        }

        #region Input Filling Methods Per Check (EC)!!

        // TO DO!
        // These methods are all filled up with static values for the purpose of testing, the proper UI values should be called later!

        private TimberParametersCheckInputEU PrepareParametersCheckInputEC()
        {
            var checkInput = new TimberParametersCheckInputEU();

            checkInput.material = eTimberMaterialType.SolidTimber;
            checkInput.serviceClass = eServiceClass.ServiceClass1;
            checkInput.loadDurationClass = eLoadDurationClass.PermanentAction;

            checkInput.IsFactorsModified = false;

            return checkInput;
        }


        private TimberTensionCheckInputEU PrepareTensionCheckInputEC()
        {
            var checkInput = new TimberTensionCheckInputEU();

            checkInput.Ft = 14;
            checkInput.NetSectionArea = 134900;
            checkInput.MaxTensionDemand = 30000;
            checkInput.MaxTensionDemand90 = 0f;

            return checkInput;
        }

        private TimberCompressionCheckInputEU PrepareCompressionCheckInputEC()
        {
            var checkInput = new TimberCompressionCheckInputEU();

            checkInput.AppliedAngle = 45;
            checkInput.NetSectionArea = 134900;
            checkInput.EffectiveArea90 = 30000;
            checkInput.SupportType = eSupportTypeEU.Continuous;
            checkInput.Fc90 = 4.9f;
            checkInput.Fc = 21;
            checkInput.MaxCompressionAppliedAngled = 0;
            checkInput.MaxCompressionDemandParallel = 30000;
            checkInput.MaxCompressionDemandPerpendicular = 3000;

            return checkInput;
        }

        private TimberBendingCheckInputEU PrepareBendingCheckInputEC()
        {

            // To Do, These static values should be called from UI!!
            var checkInput = new TimberBendingCheckInputEU();

            checkInput.material = eTimberMaterialType.SolidTimber;

            checkInput.Fm = 24f;
            checkInput.h2 = 300f;
            checkInput.h1 = 450f;
            checkInput.MaxDemandMomentMajor = 1.2e7f;
            checkInput.MaxDemandMomentMinor = 2.0e6f;

            return checkInput;
        }

        private TimberShearCheckInputEU PrepareShearCheckInputEC()
        {
            var checkInput = new TimberShearCheckInputEU();

            checkInput.material = eTimberMaterialType.SolidTimber;

            checkInput.h1 = 450;
            checkInput.h2 = 300;

            checkInput.Ft90 = 0.6f;
            checkInput.Fv = 3.7f;
            checkInput.RollingShearEffectiveArea = 134900;
            checkInput.MaxShearDemand = 30000;
            checkInput.MaxRollingShearDemand = 3000;
            checkInput.MaxTorsionStressDemand = 1;

            return checkInput;
        }

        private TimberCombinedCheckInputEU PrepareCombinedBendingAxialCheckInputEC()
        {
            var checkInput = new TimberCombinedCheckInputEU();

            checkInput.material = eTimberMaterialType.SolidTimber;

            checkInput.h1 = 450;
            checkInput.h2 = 300;

            checkInput.EffectiveBeamLength = 6000;
            checkInput.E_005 = 8400;
            checkInput.Fc = 21;
            checkInput.MajorEffectiveLength = 6000;
            checkInput.MinorEffectiveLength = 2500;

            checkInput.Fm = 24;

            return checkInput;
        }

        #endregion

        private ITimberDesignCheckInput PrepareCheckInputTR(eTimberDesignCheckType checkType)
        {
            switch (checkType)
            {
                case eTimberDesignCheckType.Parameters:
                    var parameters = PrepareParametersCheckInputTS();
                    return parameters;

                case eTimberDesignCheckType.Tension:
                    var tensionInputs = PrepareTensionCheckInputTS();
                    return tensionInputs;

                case eTimberDesignCheckType.Compression:
                    var compressionInputs = PrepareCompressionCheckInputTS();
                    return compressionInputs;

                case eTimberDesignCheckType.Bending:
                    var bendingInputs = PrepareBendingCheckInputTS();
                    return bendingInputs;

                case eTimberDesignCheckType.Shear:
                    var shearInputs = PrepareShearCheckInputTS();
                    return shearInputs;

                case eTimberDesignCheckType.CombinedBendingAxial:
                    var combinedInputs = PrepareCombinedBendingAxialCheckInputTS();
                    return combinedInputs;

                default:
                    break;
            }

            return null;
        }


        #region Input Filling Methods Per Check (TS)!!

        // TO DO!
        // These methods are all filled up with static values for the purpose of testing, the proper UI values should be called later!

        private TimberParametersCheckInputTS PrepareParametersCheckInputTS()
        {
            var checkInput = new TimberParametersCheckInputTS();

            checkInput.material = eTimberMaterialType.SolidTimber;
            checkInput.serviceClass = eServiceClass.ServiceClass1;
            checkInput.loadDurationClass = eLoadDurationClass.PermanentAction;

            checkInput.IsFactorsModified = false;

            return checkInput;
        }


        private TimberTensionCheckInputTS PrepareTensionCheckInputTS()
        {
            var checkInput = new TimberTensionCheckInputTS();

            return checkInput;
        }

        private TimberCompressionCheckInputTS PrepareCompressionCheckInputTS()
        {
            var checkInput = new TimberCompressionCheckInputTS();


            return checkInput;
        }

        private TimberBendingCheckInputTS PrepareBendingCheckInputTS()
        {

            // To Do, These static values should be called from UI!!
            var checkInput = new TimberBendingCheckInputTS();

            checkInput.material = eTimberMaterialType.SolidTimber;

            checkInput.Fm = 24f;
            checkInput.h2 = 300f;
            checkInput.h1 = 450f;
            checkInput.MaxDemandMomentMajor = 1.2e7f;
            checkInput.MaxDemandMomentMinor = 2.0e6f;

            return checkInput;
        }

        private TimberShearCheckInputTS PrepareShearCheckInputTS()
        {
            var checkInput = new TimberShearCheckInputTS();


            checkInput.h1 = 450;
            checkInput.h2 = 300;

            checkInput.Ft90 = 0.6f;
            checkInput.Fv = 3.7f;
            checkInput.RollingShearEffectiveArea = 134900;
            checkInput.MaxShearDemand = 30000;
            checkInput.MaxRollingShearDemand = 3000;
            checkInput.MaxTorsionStressDemand = 1;

            return checkInput;
        }

        private TimberCombinedCheckInputTS PrepareCombinedBendingAxialCheckInputTS()
        {
            var checkInput = new TimberCombinedCheckInputTS();

            checkInput.material = eTimberMaterialType.SolidTimber;

            return checkInput;
        }

        #endregion

        private ITimberDesignCheckInput PrepareCheckInputUS(eTimberDesignCheckType checkType)
        {
            throw new NotImplementedException();
        }

        #endregion

        public List<ITimberDesignCheckInput> PrepareAllCheckInputs()
        {
            var checkInputs = new List<ITimberDesignCheckInput>();

            try
            {
                // Get all required CheckTypes from the provider
                var checkTypes = _checkTypeProvider.GetRequiredCheckTypes();

                for (int i = 0; i < checkTypes.Count; i++)
                {
                    var checkType = checkTypes[i];

                    // Prepare the input for this checkType
                    ITimberDesignCheckInput input = PrepareCheckInput(checkType);

                    checkInputs.Add(input);
                }
            }
            catch
            {
            }

            return checkInputs;
        }

    }
}
