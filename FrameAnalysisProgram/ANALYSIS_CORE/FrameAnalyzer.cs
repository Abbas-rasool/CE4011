using System;
using System.Collections.Generic;
using System.Linq;
using FrameAnalysisProgram.ANALYSIS_CORE.Validation;
using FrameAnalysisProgram.STRUCTURAL_MODEL;
using Matrix_Library.ABSTRACTIONS;
using Matrix_Library.MAIN_TYPES;

namespace FrameAnalysisProgram.ANALYSIS_CORE
{
    /// <summary>
    /// Performs the complete structural analysis workflow for a 2D frame model.
    ///
    /// Stability handling (the program never crashes — it explains the cause):
    ///   1. Model-level validation rules (topology, restraints, determinacy screen).
    ///   2. Authoritative LDL^T singularity check on Kff, localized to a node/DOF.
    ///   3. A guarded solve that translates any solver failure into a clear message.
    ///   4. A post-solve equilibrium residual sanity check.
    /// Fatal problems are raised as a StructuralAnalysisException carrying clear
    /// messages; non-fatal findings travel on the result.
    /// </summary>
    public class FrameAnalyzer
    {
        private readonly DofNumberingService _dofNumberingService;
        private readonly GlobalStiffnessAssembler _globalStiffnessAssembler;
        private readonly LoadVectorBuilder _loadVectorBuilder;
        private readonly SettlementLoadBuilder _settlementLoadBuilder;
        private readonly ILinearSolver _linearSolver;
        private readonly DisplacementMapper _displacementMapper;
        private readonly ElementForceRecovery _elementForceRecovery;
        private readonly ReactionRecovery _reactionRecovery;
        private readonly ModelValidator _modelValidator;
        private readonly StiffnessSingularityDetector _singularityDetector;

        public FrameAnalyzer(
            DofNumberingService dofNumberingService,
            GlobalStiffnessAssembler globalStiffnessAssembler,
            LoadVectorBuilder loadVectorBuilder,
            SettlementLoadBuilder settlementLoadBuilder,
            ILinearSolver linearSolver,
            DisplacementMapper displacementMapper,
            ElementForceRecovery elementForceRecovery,
            ReactionRecovery reactionRecovery,
            ModelValidator modelValidator,
            StiffnessSingularityDetector singularityDetector)
        {
            _dofNumberingService = dofNumberingService ?? throw new ArgumentNullException(nameof(dofNumberingService));
            _globalStiffnessAssembler = globalStiffnessAssembler ?? throw new ArgumentNullException(nameof(globalStiffnessAssembler));
            _loadVectorBuilder = loadVectorBuilder ?? throw new ArgumentNullException(nameof(loadVectorBuilder));
            _settlementLoadBuilder = settlementLoadBuilder ?? throw new ArgumentNullException(nameof(settlementLoadBuilder));
            _linearSolver = linearSolver ?? throw new ArgumentNullException(nameof(linearSolver));
            _displacementMapper = displacementMapper ?? throw new ArgumentNullException(nameof(displacementMapper));
            _elementForceRecovery = elementForceRecovery ?? throw new ArgumentNullException(nameof(elementForceRecovery));
            _reactionRecovery = reactionRecovery ?? throw new ArgumentNullException(nameof(reactionRecovery));
            _modelValidator = modelValidator ?? throw new ArgumentNullException(nameof(modelValidator));
            _singularityDetector = singularityDetector ?? throw new ArgumentNullException(nameof(singularityDetector));
        }

        public FrameAnalysisResult Analyze(StructureModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            // 1. Cheap, physical model-level checks. Errors abort with a clear message.
            ModelValidationResult validation = _modelValidator.Validate(model);
            if (validation.HasErrors)
                throw new StructuralAnalysisException(validation.Messages);

            // Non-fatal findings (warnings / info) accompany the result.
            var messages = new List<ValidationMessage>(validation.Messages);

            DofMap dofMap = _dofNumberingService.BuildEquationNumbers(model);
            SparseMatrix globalStiffnessMatrix = _globalStiffnessAssembler.Assemble(model, dofMap);

            CustomVector globalLoadVector = _loadVectorBuilder.Build(model, dofMap);

            // Support settlements enter as equivalent free-DOF loads: Ff - Kfr*Ur.
            _settlementLoadBuilder.Apply(model, dofMap, globalLoadVector);

            CustomVector globalDisplacementVector;
            try
            {
                _linearSolver.Factorize(globalStiffnessMatrix);
                globalDisplacementVector = _linearSolver.Solve(globalLoadVector);
            }
            catch (Exception ex)
            {
                List<ValidationMessage> singularity = _singularityDetector.Check(globalStiffnessMatrix, dofMap).ToList();

                if (singularity.Count > 0)
                {
                    var fatal = new List<ValidationMessage>(validation.Messages);
                    fatal.AddRange(singularity);
                    throw new StructuralAnalysisException(fatal);
                }

                // Fallback fallback: If the detector somehow didn't catch a specific DOF, 
                // still throw a clear error instead of crashing.
                var generalFatal = new List<ValidationMessage>(validation.Messages)
                {
                    ValidationMessage.Error(
                        "The stiffness solve failed (" + ex.Message + "). " +
                        "The structure is likely unstable or improperly constrained.")
                };

                throw new StructuralAnalysisException(generalFatal);
            }

            double[,] nodalDisplacements = _displacementMapper.BuildNodalDisplacementMatrix(
                dofMap,
                globalDisplacementVector,
                model);

            List<ElementEndForceResult> elementEndForces = _elementForceRecovery.ComputeEndForces(
                model,
                dofMap,
                globalDisplacementVector);

            List<NodalReaction> reactions = _reactionRecovery.Compute(model, elementEndForces);

            // 4. Post-solve equilibrium residual sanity check.
            AddEquilibriumResidualWarning(globalStiffnessMatrix, globalDisplacementVector, globalLoadVector, messages);

            return new FrameAnalysisResult(
                dofMap,
                globalStiffnessMatrix,
                globalLoadVector,
                globalDisplacementVector,
                nodalDisplacements,
                elementEndForces,
                reactions,
                messages);
        }

        private static void AddEquilibriumResidualWarning(
            SparseMatrix k,
            CustomVector u,
            CustomVector f,
            List<ValidationMessage> messages)
        {
            if (u.Length == 0)
                return;

            CustomVector ku = k.Multiply(u);

            double residual = 0.0;
            double loadNorm = 0.0;
            for (int i = 0; i < f.Length; i++)
            {
                double diff = ku[i] - f[i];
                residual += diff * diff;
                loadNorm += f[i] * f[i];
            }

            residual = Math.Sqrt(residual);
            loadNorm = Math.Sqrt(loadNorm);
            double scale = loadNorm > 0.0 ? loadNorm : 1.0;

            if (residual > 1e-6 * scale)
            {
                messages.Add(ValidationMessage.Warning(
                    $"Equilibrium residual ||K*U - F|| = {residual:G3} is large relative to the applied load " +
                    $"({loadNorm:G3}); the solution may be unreliable (near-mechanism or ill-conditioned)."));
            }
        }
    }
}
