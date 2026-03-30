using System;
using FrameAnalysisProgram.ANALYSIS_CORE;
using FrameAnalysisProgram.STRUCTURAL_MODEL;
using Matrix_Library.MAIN_TYPES;

namespace FrameAnalysisProgram.INPUT_OUTPUT
{
    /// <summary>
    /// Prints structural model data and analysis results to the console.
    /// 
    /// Purpose:
    /// - Display input model information
    /// - Display equation numbering
    /// - Display global analysis results
    /// - Display nodal displacements
    /// - Display element end forces
    /// </summary>
    public class ResultPrinter
    {
        public void PrintModel(StructureModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            PrintNodes(model);
            PrintMaterials(model);
            PrintSections(model);
            PrintElements(model);
            PrintSupports(model);
            PrintLoads(model);
        }

        public void PrintDofMap(DofMap dofMap)
        {
            if (dofMap == null)
                throw new ArgumentNullException(nameof(dofMap));

            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("DEGREE OF FREEDOM MAP");
            Console.WriteLine("========================================");
            Console.WriteLine("Node\tUx\tUy\tRz");

            int nodeCount = dofMap.EquationNumbers.GetLength(0);

            for (int i = 0; i < nodeCount; i++)
            {
                Console.WriteLine(
                    $"{i + 1}\t{dofMap.EquationNumbers[i, 0]}\t{dofMap.EquationNumbers[i, 1]}\t{dofMap.EquationNumbers[i, 2]}");
            }

            Console.WriteLine();
            Console.WriteLine($"Total number of active equations = {dofMap.NumberOfEquations}");
        }

        public void PrintAnalysisResult(FrameAnalysisResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            PrintDofMap(result.DofMap);
            PrintGlobalLoadVector(result.GlobalLoadVector);
            PrintGlobalDisplacementVector(result.GlobalDisplacementVector);
            PrintNodalDisplacements(result.NodalDisplacements);
            PrintElementEndForces(result);
        }

        public void PrintGlobalLoadVector(CustomVector loadVector)
        {
            if (loadVector == null)
                throw new ArgumentNullException(nameof(loadVector));

            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("GLOBAL LOAD VECTOR");
            Console.WriteLine("========================================");

            for (int i = 0; i < loadVector.Length; i++)
            {
                Console.WriteLine($"F[{i + 1}] = {loadVector.Get(i):G10}");
            }
        }

        public void PrintGlobalDisplacementVector(CustomVector displacementVector)
        {
            if (displacementVector == null)
                throw new ArgumentNullException(nameof(displacementVector));

            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("GLOBAL DISPLACEMENT VECTOR");
            Console.WriteLine("========================================");

            for (int i = 0; i < displacementVector.Length; i++)
            {
                Console.WriteLine($"D[{i + 1}] = {displacementVector.Get(i):G10}");
            }
        }

        public void PrintNodalDisplacements(double[,] nodalDisplacements)
        {
            if (nodalDisplacements == null)
                throw new ArgumentNullException(nameof(nodalDisplacements));

            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("NODAL DISPLACEMENTS");
            Console.WriteLine("========================================");
            Console.WriteLine("Node\tUx\tUy\tRz");

            int nodeCount = nodalDisplacements.GetLength(0);

            for (int i = 0; i < nodeCount; i++)
            {
                Console.WriteLine(
                    $"{i + 1}\t{nodalDisplacements[i, 0]:G10}\t{nodalDisplacements[i, 1]:G10}\t{nodalDisplacements[i, 2]:G10}");
            }
        }

        public void PrintElementEndForces(FrameAnalysisResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("ELEMENT END FORCES (LOCAL COORDINATES)");
            Console.WriteLine("========================================");

            foreach (ElementEndForceResult elementResult in result.ElementEndForces)
            {
                double[] f = elementResult.LocalEndForces;

                Console.WriteLine();
                Console.WriteLine($"Element {elementResult.Element.Id}");
                Console.WriteLine($"[Fx1, Fy1, Mz1, Fx2, Fy2, Mz2]");

                for (int i = 0; i < f.Length; i++)
                {
                    Console.WriteLine($"f[{i + 1}] = {f[i]:G10}");
                }
            }
        }

        private void PrintNodes(StructureModel model)
        {
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("NODES");
            Console.WriteLine("========================================");
            Console.WriteLine("Id\tX\tY");

            foreach (Node node in model.Nodes)
            {
                Console.WriteLine($"{node.Id}\t{node.X:G10}\t{node.Y:G10}");
            }
        }

        private void PrintMaterials(StructureModel model)
        {
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("MATERIALS");
            Console.WriteLine("========================================");
            Console.WriteLine("Id\tE");

            foreach (Material material in model.Materials)
            {
                Console.WriteLine($"{material.Id}\t{material.ElasticModulus:G10}");
            }
        }

        private void PrintSections(StructureModel model)
        {
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("SECTIONS");
            Console.WriteLine("========================================");
            Console.WriteLine("Id\tA\tI");

            foreach (SectionProperty section in model.Sections)
            {
                Console.WriteLine($"{section.Id}\t{section.Area:G10}\t{section.MomentOfInertia:G10}");
            }
        }

        private void PrintElements(StructureModel model)
        {
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("ELEMENTS");
            Console.WriteLine("========================================");
            Console.WriteLine("Id\tStart\tEnd\tMat\tSec\tLength");

            foreach (FrameElement2D element in model.Elements)
            {
                Console.WriteLine(
                    $"{element.Id}\t{element.StartNode.Id}\t{element.EndNode.Id}\t{element.Material.Id}\t{element.Section.Id}\t{element.Length:G10}");
            }
        }

        private void PrintSupports(StructureModel model)
        {
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("SUPPORTS");
            Console.WriteLine("========================================");
            Console.WriteLine("Node\tUx\tUy\tRz");

            foreach (SupportCondition support in model.Supports)
            {
                Console.WriteLine(
                    $"{support.Node.Id}\t{BoolToCode(support.RestrainsUx)}\t{BoolToCode(support.RestrainsUy)}\t{BoolToCode(support.RestrainsRz)}");
            }
        }

        private void PrintLoads(StructureModel model)
        {
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("LOADS");
            Console.WriteLine("========================================");
            Console.WriteLine("Node\tFx\tFy\tMz");

            foreach (JointLoad load in model.Loads)
            {
                Console.WriteLine($"{load.Node.Id}\t{load.Fx:G10}\t{load.Fy:G10}\t{load.Mz:G10}");
            }
        }

        public void PrintGlobalStiffnessMatrixCompact(SparseMatrix matrix)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            Console.WriteLine();
            Console.WriteLine("K (compact view)");
            Console.WriteLine("----------------");

            int n = matrix.Count;

            for (int i = 0; i < n; i++)
            {
                Console.Write("[ ");

                for (int j = 0; j < n; j++)
                {
                    double value = matrix.Get(i, j);

                    if (Math.Abs(value) < 1e-10)
                        Console.Write("   0   ");
                    else
                        Console.Write($"{value,7:F2}");
                }

                Console.WriteLine(" ]");
            }
        }

        private int BoolToCode(bool value)
        {
            return value ? 1 : 0;
        }
    }
}