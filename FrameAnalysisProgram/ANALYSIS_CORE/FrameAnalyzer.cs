using System;
using System.Collections.Generic;
using FrameAnalysisProgram.STRUCTURAL_MODEL;
using Matrix_Library.ABSTRACTIONS;
using Matrix_Library.MAIN_TYPES;

namespace FrameAnalysisProgram.ANALYSIS_CORE
{
    /// <summary>
    /// Performs the complete structural analysis workflow for a 2D frame model.
    /// </summary>
    public class FrameAnalyzer
    {
        private readonly DofNumberingService _dofNumberingService;
        private readonly GlobalStiffnessAssembler _globalStiffnessAssembler;
        private readonly LoadVectorBuilder _loadVectorBuilder;
        private readonly ILinearSolver _linearSolver;
        private readonly DisplacementMapper _displacementMapper;
        private readonly ElementForceRecovery _elementForceRecovery;

        public FrameAnalyzer(
            DofNumberingService dofNumberingService,
            GlobalStiffnessAssembler globalStiffnessAssembler,
            LoadVectorBuilder loadVectorBuilder,
            ILinearSolver linearSolver,
            DisplacementMapper displacementMapper,
            ElementForceRecovery elementForceRecovery)
        {
            _dofNumberingService = dofNumberingService ?? throw new ArgumentNullException(nameof(dofNumberingService));
            _globalStiffnessAssembler = globalStiffnessAssembler ?? throw new ArgumentNullException(nameof(globalStiffnessAssembler));
            _loadVectorBuilder = loadVectorBuilder ?? throw new ArgumentNullException(nameof(loadVectorBuilder));
            _linearSolver = linearSolver ?? throw new ArgumentNullException(nameof(linearSolver));
            _displacementMapper = displacementMapper ?? throw new ArgumentNullException(nameof(displacementMapper));
            _elementForceRecovery = elementForceRecovery ?? throw new ArgumentNullException(nameof(elementForceRecovery));
        }

        public FrameAnalysisResult Analyze(StructureModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            DofMap dofMap = _dofNumberingService.BuildEquationNumbers(model);

            SparseMatrix globalStiffnessMatrix = _globalStiffnessAssembler.Assemble(model, dofMap);

            CustomVector globalLoadVector = _loadVectorBuilder.Build(model, dofMap);

            _linearSolver.Factorize(globalStiffnessMatrix);
            CustomVector globalDisplacementVector = _linearSolver.Solve(globalLoadVector);

            double[,] nodalDisplacements = _displacementMapper.BuildNodalDisplacementMatrix(
                dofMap,
                globalDisplacementVector,
                model.Nodes.Count);

            List<ElementEndForceResult> elementEndForces = _elementForceRecovery.ComputeEndForces(
                model,
                dofMap,
                globalDisplacementVector);

            return new FrameAnalysisResult(
                dofMap,
                globalStiffnessMatrix,
                globalLoadVector,
                globalDisplacementVector,
                nodalDisplacements,
                elementEndForces);
        }
    }
}