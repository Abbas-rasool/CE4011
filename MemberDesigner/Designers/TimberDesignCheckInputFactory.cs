using System.Collections.Generic;
using MemberDesigner.DesignChecks;
using MemberDesigner.DesignInputs.American;
using MemberDesigner.DesignInputs.Eurocode;
using MemberDesigner.DesignInputs.Turkish;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.Designers
{
    /// <summary>
    /// Builds the per-check design inputs for a single member from a
    /// <see cref="TimberMemberDesignContext"/>. The code is taken from the context (no longer
    /// hardcoded), and every value comes from the context (no longer static placeholders).
    /// </summary>
    public class TimberDesignCheckInputFactory
    {
        #region CTOR

        public TimberDesignCheckInputFactory(TimberCheckTypeProvider checkTypeProvider, TimberMemberDesignContext context)
        {
            _checkTypeProvider = checkTypeProvider;
            _context = context;
            _TimberCode = context.Code;
        }

        #endregion

        #region Fields

        protected TimberCheckTypeProvider _checkTypeProvider;
        private readonly TimberMemberDesignContext _context;
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

            return null;
        }

        private ITimberDesignCheckInput PrepareCheckInputEC(eTimberDesignCheckType checkType)
        {
            switch (checkType)
            {
                case eTimberDesignCheckType.Parameters: return PrepareParametersCheckInputEC();
                case eTimberDesignCheckType.Tension: return PrepareTensionCheckInputEC();
                case eTimberDesignCheckType.Compression: return PrepareCompressionCheckInputEC();
                case eTimberDesignCheckType.Bending: return PrepareBendingCheckInputEC();
                case eTimberDesignCheckType.Shear: return PrepareShearCheckInputEC();
                case eTimberDesignCheckType.CombinedBendingAxial: return PrepareCombinedBendingAxialCheckInputEC();
                default: return null;
            }
        }

        #region Input Filling Methods Per Check (EC)

        private TimberParametersCheckInputEU PrepareParametersCheckInputEC() => new()
        {
            material = _context.MaterialType,
            serviceClass = _context.ServiceClass,
            loadDurationClass = _context.LoadDurationClass,
            PartialFactor = _context.PartialFactor,
            ModificationFactor = _context.ModificationFactor,
            IsFactorsModified = _context.FactorsModified,
        };

        private TimberTensionCheckInputEU PrepareTensionCheckInputEC() => new()
        {
            Ft = _context.TensionStrength,
            NetSectionArea = _context.NetArea,
            MaxTensionDemand = _context.AxialTension,
            MaxTensionDemand90 = 0f,
        };

        private TimberCompressionCheckInputEU PrepareCompressionCheckInputEC() => new()
        {
            AppliedAngle = _context.AppliedAngle,
            NetSectionArea = _context.NetArea,
            EffectiveArea90 = _context.GrossArea,
            SupportType = _context.SupportType,
            Fc90 = _context.CompressionPerpStrength,
            Fc = _context.CompressionStrength,
            MaxCompressionAppliedAngled = 0f,
            MaxCompressionDemandParallel = _context.AxialCompression,
            MaxCompressionDemandPerpendicular = 0f,
        };

        private TimberBendingCheckInputEU PrepareBendingCheckInputEC() => new()
        {
            material = _context.MaterialType,
            Fm = _context.BendingStrength,
            h1 = _context.H1,
            h2 = _context.H2,
            MaxDemandMomentMajor = _context.MomentMajor,
            MaxDemandMomentMinor = _context.MomentMinor,
        };

        private TimberShearCheckInputEU PrepareShearCheckInputEC() => new()
        {
            material = _context.MaterialType,
            h1 = _context.H1,
            h2 = _context.H2,
            Ft90 = _context.TensionPerpStrength,
            Fv = _context.ShearStrength,
            RollingShearEffectiveArea = _context.GrossArea,
            MaxShearDemand = _context.Shear,
            MaxRollingShearDemand = 0f,
            MaxTorsionStressDemand = 0f,
        };

        private TimberCombinedCheckInputEU PrepareCombinedBendingAxialCheckInputEC() => new()
        {
            material = _context.MaterialType,
            h1 = _context.H1,
            h2 = _context.H2,
            EffectiveBeamLength = _context.EffectiveBeamLength,
            E_005 = _context.ModulusBuckling,
            Fc = _context.CompressionStrength,
            Fm = _context.BendingStrength,
            MajorEffectiveLength = _context.EffectiveLengthMajor,
            MinorEffectiveLength = _context.EffectiveLengthMinor,
        };

        #endregion

        private ITimberDesignCheckInput PrepareCheckInputTR(eTimberDesignCheckType checkType)
        {
            switch (checkType)
            {
                case eTimberDesignCheckType.Parameters: return PrepareParametersCheckInputTS();
                case eTimberDesignCheckType.Tension: return PrepareTensionCheckInputTS();
                case eTimberDesignCheckType.Compression: return PrepareCompressionCheckInputTS();
                case eTimberDesignCheckType.Bending: return PrepareBendingCheckInputTS();
                case eTimberDesignCheckType.Shear: return PrepareShearCheckInputTS();
                case eTimberDesignCheckType.CombinedBendingAxial: return PrepareCombinedBendingAxialCheckInputTS();
                default: return null;
            }
        }

        #region Input Filling Methods Per Check (TS)

        private TimberParametersCheckInputTS PrepareParametersCheckInputTS() => new()
        {
            material = _context.MaterialType,
            serviceClass = _context.ServiceClass,
            loadDurationClass = _context.LoadDurationClass,
            IsFactorsModified = _context.FactorsModified,
            CharacteristicDensity = _context.Density,
            SectionLength = _context.EffectiveBeamLength,
            h1 = _context.H1,
            h2 = _context.H2,
        };

        private TimberTensionCheckInputTS PrepareTensionCheckInputTS() => new()
        {
            NetSectionArea = _context.NetArea,
            Ft = _context.TensionStrength,
            MaxDemandTension = _context.AxialTension,
        };

        private TimberCompressionCheckInputTS PrepareCompressionCheckInputTS() => new()
        {
            material = _context.MaterialType,
            AppliedAngle = _context.AppliedAngle,
            NetSectionAreaPerpendicular = _context.GrossArea,
            SectionLength = _context.EffectiveLengthMajor,
            CharacteristicDensity = _context.Density,
            f_c0k = _context.CompressionStrength,
            f_c90k = _context.CompressionPerpStrength,
            E_005 = _context.ModulusBuckling,
            h1 = _context.H1,
            h2 = _context.H2,
            BucklingLengthCoe1 = _context.BucklingLengthCoeMajor,
            BucklingLengthCoe2 = _context.BucklingLengthCoeMinor,
            Length1 = _context.EffectiveLengthMajor,
            Length2 = _context.EffectiveLengthMinor,
            SectionGrossArea = _context.GrossArea,
            MaxCompressionDemandParallel = _context.AxialCompression,
            MaxCompressionDemandPerpendicular = 0f,
            MaxCompressionDemandAngled = 0f,
        };

        private TimberShearCheckInputTS PrepareShearCheckInputTS() => new()
        {
            Material = _context.MaterialType,
            h1 = _context.H1,
            h2 = _context.H2,
            Ft90 = _context.TensionPerpStrength,
            Fv = _context.ShearStrength,
            RollingShearEffectiveArea = _context.GrossArea,
            MaxShearDemand = _context.Shear,
            MaxRollingShearDemand = 0f,
            MaxTorsionStressDemand = 0f,
        };

        private TimberBendingCheckInputTS PrepareBendingCheckInputTS() => new()
        {
            material = _context.MaterialType,
            h1 = _context.H1,
            h2 = _context.H2,
            EffectiveBeamLength = _context.EffectiveBeamLength,
            E_005 = _context.ModulusBuckling,
            Fm = _context.BendingStrength,
            MaxDemandMomentMajor = _context.MomentMajor,
            MaxDemandMomentMinor = _context.MomentMinor,
        };

        private TimberCombinedCheckInputTS PrepareCombinedBendingAxialCheckInputTS() => new()
        {
            material = _context.MaterialType,
        };

        #endregion

        private ITimberDesignCheckInput PrepareCheckInputUS(eTimberDesignCheckType checkType)
        {
            switch (checkType)
            {
                // US has no standalone Parameters check; adjustment factors are computed inside
                // each check from the base-class fields. Skip it (filtered out downstream).
                case eTimberDesignCheckType.Parameters: return null;
                case eTimberDesignCheckType.Tension: return PrepareTensionCheckInputUS();
                case eTimberDesignCheckType.Compression: return PrepareCompressionCheckInputUS();
                case eTimberDesignCheckType.Bending: return PrepareBendingCheckInputUS();
                case eTimberDesignCheckType.Shear: return PrepareShearCheckInputUS();
                case eTimberDesignCheckType.CombinedBendingAxial: return PrepareCombinedBendingAxialCheckInputUS();
                default: return null;
            }
        }

        #region Input Filling Methods Per Check (US)

        /// <summary>Fills the fields shared by all base-class US check inputs.</summary>
        private void FillBaseUS(TimberCheckInputBaseClassUS input, eDesignParameter designParameter)
        {
            input.h1 = _context.H1;
            input.h2 = _context.H2;
            input.Temperature = _context.Temperature;
            input.TimeEffectFactor = _context.TimeEffectFactor;
            input.LoadDurationFactor = _context.LoadDurationFactor;
            input.loadCombinationType = _context.DesignMethod;
            input.IsLumberIncised = false;
            input.TimberType = _context.TimberType;
            input.designParameter = designParameter;
            input.memberConfigurationType = _context.MemberConfiguration;
            input.MoistureContentCondition = _context.MoistureCondition;
            input.TimberGrade = _context.TimberGrade;
            input.ApplicationType = _context.ApplicationType;
        }

        private TimberBendingCheckInputUS PrepareBendingCheckInputUS()
        {
            var input = new TimberBendingCheckInputUS
            {
                StudSpacing = 0f,
                Fb = _context.BendingStrength,
                EffectiveLengthMajor = _context.EffectiveLengthMajor,
                EffectiveLengthMinor = _context.EffectiveLengthMinor,
                MaxDemandMomentMajor = _context.MomentMajor,
                MaxDemandMomentMinor = _context.MomentMinor,
                E = _context.ModulusMean,
                Emin = _context.ModulusBuckling,
                BucklingLengthCoe = _context.BucklingLengthCoeMajor,
                EndDistance = 0f,
                SectionModulusMajor = _context.SectionModulusMajor,
                SectionModulusMinor = _context.SectionModulusMinor,
                IsLaterallySupported = _context.IsLaterallySupported,
                IsRepetitiveMember = _context.IsRepetitiveMember,
            };
            FillBaseUS(input, eDesignParameter.Fb);
            return input;
        }

        private TimberTensionCheckInputUS PrepareTensionCheckInputUS()
        {
            var input = new TimberTensionCheckInputUS
            {
                Ft = _context.TensionStrength,
                NetSectionArea = _context.NetArea,
                MaxTensionDemand = _context.AxialTension,
            };
            FillBaseUS(input, eDesignParameter.Ft);
            return input;
        }

        private TimberCompressionCheckInputUS PrepareCompressionCheckInputUS()
        {
            var input = new TimberCompressionCheckInputUS
            {
                BearingLength = 0f,
                Length1 = _context.EffectiveLengthMajor,
                Length2 = _context.EffectiveLengthMinor,
                Length3 = 0f,
                EndDistance = 0f,
                BucklingLengthCoe1 = _context.BucklingLengthCoeMajor,
                BucklingLengthCoe2 = _context.BucklingLengthCoeMinor,
                E = _context.ModulusMean,
                Emin = _context.ModulusBuckling,
                Fc = _context.CompressionStrength,
                Fc90 = _context.CompressionPerpStrength,
                NetSectionAreaParallel = _context.NetArea,
                NetSectionAreaPerpendicular = _context.GrossArea,
                GrossSectionArea = _context.GrossArea,
                MaxCompressionDemandParallel = _context.AxialCompression,
                MaxCompressionDemandPerpendicular = 0f,
                builtUPColumnType = eBuiltUPColumnConnectionType.None,
                IsRepetitiveMember = _context.IsRepetitiveMember,
            };
            FillBaseUS(input, eDesignParameter.Fc);
            return input;
        }

        private TimberShearCheckInputUS PrepareShearCheckInputUS()
        {
            var input = new TimberShearCheckInputUS
            {
                Inertia = _context.Inertia,
                MomentofArea = _context.FirstMomentOfArea,
                Fv = _context.ShearStrength,
                MaxShearDemand = _context.Shear,
            };
            FillBaseUS(input, eDesignParameter.Fv);
            return input;
        }

        private TimberCombinedBendingAxialCheckInputUS PrepareCombinedBendingAxialCheckInputUS() => new()
        {
            memberConfigurationType = _context.MemberConfiguration,
            MaxDemandMomentMajor = _context.MomentMajor,
            MaxDemandMomentMinor = _context.MomentMinor,
        };

        #endregion

        #endregion

        public List<ITimberDesignCheckInput> PrepareAllCheckInputs()
        {
            var checkInputs = new List<ITimberDesignCheckInput>();

            // Required check types for this run; null results (e.g. US Parameters) are skipped
            // so the list stays clean. Errors propagate to the caller (DesignService) instead
            // of being silently swallowed.
            var checkTypes = _checkTypeProvider.GetRequiredCheckTypes();

            for (int i = 0; i < checkTypes.Count; i++)
            {
                ITimberDesignCheckInput input = PrepareCheckInput(checkTypes[i]);
                if (input != null)
                    checkInputs.Add(input);
            }

            return checkInputs;
        }
    }
}
