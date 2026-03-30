#region Matrix Testing Program

//using System;
//using Matrix_Library.ABSTRACTIONS;
//using Matrix_Library.MAIN_TYPES;
//using Matrix_Library.SOLVERS;

//namespace Matrix_Library_Validation
//{
//    internal class Program
//    {
//        static void Main(string[] args)
//        {
//            Console.WriteLine("=== MATRIX LIBRARY VALIDATION ===\n");

//            RunCase1_KnownExactSolution();
//            RunCase2_StructuralStyleSPD();
//            RunCase3_TwoByTwo();
//            RunCase4_TridiagonalStiffness();

//            RunCase5_SpringChain();
//            RunCase6_CrossBracedSystem();

//            Console.WriteLine("\nValidation finished.");
//            Console.ReadKey();
//        }

//        private static void RunCase1_KnownExactSolution()
//        {
//            Console.WriteLine("====================================");
//            Console.WriteLine("CASE 1: Small symmetric system");
//            Console.WriteLine("Expected exact solution: x = [5, 5, 5]");
//            Console.WriteLine("====================================");

//            SparseMatrix A = new SparseMatrix(3);

//            // Symmetric matrix:
//            // [ 4 -1  0 ]
//            // [ -1 4 -1 ]
//            // [ 0 -1 3 ]

//            A.SetSymmetric(0, 0, 4.0);
//            A.SetSymmetric(0, 1, -1.0);
//            A.SetSymmetric(0, 2, 0.0);

//            A.SetSymmetric(1, 1, 4.0);
//            A.SetSymmetric(1, 2, -1.0);

//            A.SetSymmetric(2, 2, 3.0);

//            CustomVector expected = new CustomVector(3);
//            expected[0] = 5.0;
//            expected[1] = 5.0;
//            expected[2] = 5.0;

//            // IMPORTANT:
//            // Since only one triangle is stored, use symmetric multiply for validation.
//            CustomVector b = A.Multiply(expected);

//            ILinearSolver solver = new SparseLDLtSolver();
//            solver.Factorize(A);

//            CustomVector x = solver.Solve(b);

//            Console.WriteLine("\nMatrix A:");
//            PrintSymmetricMatrix(A);

//            Console.WriteLine("\nVector b:");
//            PrintVector(b, "b");

//            Console.WriteLine("\nComputed solution x:");
//            PrintVector(x, "x");

//            Console.WriteLine("\nExpected solution:");
//            PrintVector(expected, "x_expected");

//            Console.WriteLine("\nError compared to expected:");
//            PrintVector(VectorDifference(x, expected), "error");

//            CustomVector Ax = A.Multiply(x);
//            Console.WriteLine("\nCheck A*x:");
//            PrintVector(Ax, "A*x");

//            Console.WriteLine($"\nResidual norm ||A*x - b|| = {ComputeResidualNorm(Ax, b):E6}");
//            Console.WriteLine();
//        }

//        private static void RunCase2_StructuralStyleSPD()
//        {
//            Console.WriteLine("====================================");
//            Console.WriteLine("CASE 2: Structural-style SPD matrix");
//            Console.WriteLine("Verify A*x ≈ b");
//            Console.WriteLine("====================================");

//            SparseMatrix K = new SparseMatrix(4);

//            // Symmetric positive definite matrix:
//            // [ 12 -3  0 -2 ]
//            // [ -3 10 -2  0 ]
//            // [  0 -2  8 -1 ]
//            // [ -2  0 -1  6 ]

//            K.SetSymmetric(0, 0, 12.0);
//            K.SetSymmetric(0, 1, -3.0);
//            K.SetSymmetric(0, 2, 0.0);
//            K.SetSymmetric(0, 3, -2.0);

//            K.SetSymmetric(1, 1, 10.0);
//            K.SetSymmetric(1, 2, -2.0);
//            K.SetSymmetric(1, 3, 0.0);

//            K.SetSymmetric(2, 2, 8.0);
//            K.SetSymmetric(2, 3, -1.0);

//            K.SetSymmetric(3, 3, 6.0);

//            CustomVector xExact = new CustomVector(4);
//            xExact[0] = 1.0;
//            xExact[1] = 2.0;
//            xExact[2] = 3.0;
//            xExact[3] = 4.0;

//            // IMPORTANT:
//            // Use symmetric multiply here too.
//            CustomVector b = K.Multiply(xExact);

//            ILinearSolver solver = new SparseLDLtSolver();
//            solver.Factorize(K);

//            CustomVector xComputed = solver.Solve(b);

//            Console.WriteLine("\nMatrix K:");
//            PrintSymmetricMatrix(K);

//            Console.WriteLine("\nExact x used to generate b:");
//            PrintVector(xExact, "x_exact");

//            Console.WriteLine("\nGenerated load vector b = K*x_exact:");
//            PrintVector(b, "b");

//            Console.WriteLine("\nComputed solution x:");
//            PrintVector(xComputed, "x");

//            Console.WriteLine("\nError compared to x_exact:");
//            PrintVector(VectorDifference(xComputed, xExact), "error");

//            CustomVector Kx = K.Multiply(xComputed);
//            Console.WriteLine("\nCheck K*x:");
//            PrintVector(Kx, "K*x");

//            Console.WriteLine($"\nResidual norm ||K*x - b|| = {ComputeResidualNorm(Kx, b):E6}");
//            Console.WriteLine();
//        }

//        private static void RunCase3_TwoByTwo()
//        {
//            Console.WriteLine("====================================");
//            Console.WriteLine("CASE 3: Simple 2x2 SPD system");
//            Console.WriteLine("Expected exact solution: x = [1, 2]");
//            Console.WriteLine("====================================");

//            SparseMatrix A = new SparseMatrix(2);

//            A.SetSymmetric(0, 0, 6.0);
//            A.SetSymmetric(0, 1, 2.0);
//            A.SetSymmetric(1, 1, 5.0);

//            CustomVector xExact = new CustomVector(2);
//            xExact[0] = 1.0;
//            xExact[1] = 2.0;

//            CustomVector b = A.Multiply(xExact);

//            ILinearSolver solver = new SparseLDLtSolver();
//            solver.Factorize(A);
//            CustomVector x = solver.Solve(b);

//            Console.WriteLine("\nMatrix A:");
//            PrintSymmetricMatrix(A);

//            Console.WriteLine("\nVector b:");
//            PrintVector(b, "b");

//            Console.WriteLine("\nComputed solution x:");
//            PrintVector(x, "x");

//            Console.WriteLine("\nExpected solution:");
//            PrintVector(xExact, "x_exact");

//            Console.WriteLine("\nError compared to expected:");
//            PrintVector(VectorDifference(x, xExact), "error");

//            CustomVector Ax = A.Multiply(x);
//            Console.WriteLine($"\nResidual norm ||A*x - b|| = {ComputeResidualNorm(Ax, b):E6}");
//            Console.WriteLine();
//        }

//        private static void RunCase4_TridiagonalStiffness()
//        {
//            Console.WriteLine("====================================");
//            Console.WriteLine("CASE 4: 5x5 tridiagonal stiffness matrix");
//            Console.WriteLine("Expected exact solution: x = [1, 2, 3, 4, 5]");
//            Console.WriteLine("====================================");

//            SparseMatrix K = new SparseMatrix(5);

//            K.SetSymmetric(0, 0, 2.0);
//            K.SetSymmetric(0, 1, -1.0);

//            K.SetSymmetric(1, 1, 2.0);
//            K.SetSymmetric(1, 2, -1.0);

//            K.SetSymmetric(2, 2, 2.0);
//            K.SetSymmetric(2, 3, -1.0);

//            K.SetSymmetric(3, 3, 2.0);
//            K.SetSymmetric(3, 4, -1.0);

//            K.SetSymmetric(4, 4, 2.0);

//            CustomVector xExact = new CustomVector(5);
//            xExact[0] = 1.0;
//            xExact[1] = 2.0;
//            xExact[2] = 3.0;
//            xExact[3] = 4.0;
//            xExact[4] = 5.0;

//            CustomVector b = K.Multiply(xExact);

//            ILinearSolver solver = new SparseLDLtSolver();
//            solver.Factorize(K);
//            CustomVector x = solver.Solve(b);

//            Console.WriteLine("\nMatrix K:");
//            PrintSymmetricMatrix(K);

//            Console.WriteLine("\nVector b:");
//            PrintVector(b, "b");

//            Console.WriteLine("\nComputed solution x:");
//            PrintVector(x, "x");

//            Console.WriteLine("\nExpected solution:");
//            PrintVector(xExact, "x_exact");

//            Console.WriteLine("\nError compared to expected:");
//            PrintVector(VectorDifference(x, xExact), "error");

//            CustomVector Kx = K.Multiply(x);
//            Console.WriteLine($"\nResidual norm ||K*x - b|| = {ComputeResidualNorm(Kx, b):E6}");
//            Console.WriteLine();
//        }

//        private static void RunCase5_SpringChain()
//        {
//            Console.WriteLine("====================================");
//            Console.WriteLine("CASE 5: 6x6 Spring Chain (Tridiagonal)");
//            Console.WriteLine("Expected exact solution: x = [0.5, 1.0, 1.5, 2.0, 2.5, 3.0]");
//            Console.WriteLine("====================================");

//            SparseMatrix K = new SparseMatrix(6);

//            // Set diagonal and off-diagonal values
//            for (int i = 0; i < 5; i++) K.SetSymmetric(i, i, 2.0);
//            K.SetSymmetric(5, 5, 1.0);
//            for (int i = 1; i < 6; i++) K.SetSymmetric(i, i - 1, -1.0);

//            CustomVector xExact = new CustomVector(6);
//            for (int i = 0; i < 6; i++) xExact[i] = (i + 1) * 0.5;

//            ExecuteValidation(K, xExact, "Spring Chain");
//        }

//        private static void RunCase6_CrossBracedSystem()
//        {
//            Console.WriteLine("====================================");
//            Console.WriteLine("CASE 6: 5x5 Cross-Braced System (High Bandwidth)");
//            Console.WriteLine("Expected exact solution: x = [1, -1, 2, -2, 5]");
//            Console.WriteLine("====================================");

//            SparseMatrix K = new SparseMatrix(5);

//            // Diagonal
//            K.SetSymmetric(0, 0, 10.0);
//            K.SetSymmetric(1, 1, 12.0);
//            K.SetSymmetric(2, 2, 15.0);
//            K.SetSymmetric(3, 3, 18.0);
//            K.SetSymmetric(4, 4, 20.0);

//            // Off-Diagonals (Lower Triangle)
//            K.SetSymmetric(1, 0, -2.0);
//            K.SetSymmetric(2, 0, -5.0);
//            K.SetSymmetric(3, 1, -4.0);
//            K.SetSymmetric(3, 2, -3.0);
//            K.SetSymmetric(4, 0, -1.0);
//            K.SetSymmetric(4, 3, -6.0);

//            CustomVector xExact = new CustomVector(5);
//            xExact[0] = 1.0; xExact[1] = -1.0; xExact[2] = 2.0; xExact[3] = -2.0; xExact[4] = 5.0;

//            ExecuteValidation(K, xExact, "Cross-Braced");
//        }



//        #region Private Methods

//        /// <summary>
//        /// Helper to reduce boilerplate code for validation cases
//        /// </summary>
//        private static void ExecuteValidation(SparseMatrix A, CustomVector xExact, string label)
//        {
//            CustomVector b = A.Multiply(xExact);
//            ILinearSolver solver = new SparseLDLtSolver();

//            solver.Factorize(A);
//            CustomVector xComputed = solver.Solve(b);

//            Console.WriteLine($"\nComputed solution for {label}:");
//            PrintVector(xComputed, "x");

//            double residual = ComputeResidualNorm(A.Multiply(xComputed), b);
//            Console.WriteLine($"\nResidual norm ||Ax - b|| = {residual:E6}");

//            double error = ComputeResidualNorm(xComputed, xExact);
//            Console.WriteLine($"Error vs Exact: {error:E6}\n");
//        }

//        /// <summary>
//        /// Returns the logical symmetric value, even though only one triangle is stored.
//        /// </summary>
//        private static double GetSymmetricValue(SparseMatrix matrix, int i, int j)
//        {
//            return (i >= j) ? matrix.Get(i, j) : matrix.Get(j, i);
//        }

//        private static CustomVector VectorDifference(CustomVector a, CustomVector b)
//        {
//            if (a.Length != b.Length)
//                throw new ArgumentException("Vector lengths must match.");

//            CustomVector result = new CustomVector(a.Length);

//            for (int i = 0; i < a.Length; i++)
//                result[i] = a[i] - b[i];

//            return result;
//        }

//        private static double ComputeResidualNorm(CustomVector lhs, CustomVector rhs)
//        {
//            if (lhs.Length != rhs.Length)
//                throw new ArgumentException("Vector lengths must match.");

//            double sum = 0.0;

//            for (int i = 0; i < lhs.Length; i++)
//            {
//                double diff = lhs[i] - rhs[i];
//                sum += diff * diff;
//            }

//            return Math.Sqrt(sum);
//        }

//        private static void PrintSymmetricMatrix(SparseMatrix matrix)
//        {
//            for (int i = 0; i < matrix.Count; i++)
//            {
//                for (int j = 0; j < matrix.Count; j++)
//                {
//                    Console.Write($"{GetSymmetricValue(matrix, i, j),12:F4}");
//                }
//                Console.WriteLine();
//            }
//        }

//        private static void PrintVector(CustomVector vector, string name)
//        {
//            for (int i = 0; i < vector.Length; i++)
//            {
//                Console.WriteLine($"{name}[{i}] = {vector[i]:F8}");
//            }
//        }

//        #endregion

//    }
//}

#endregion

#region Main Program Entry

using System;
using System.IO;
using System.Text.Json;
using FrameAnalysisProgram.ANALYSIS_CORE;
using FrameAnalysisProgram.INPUT_OUTPUT;
using FrameAnalysisProgram.STRUCTURAL_MODEL;
using Matrix_Library.SOLVERS;

namespace FAP
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                StructureInputData input = null;

                Console.WriteLine("FRAME ANALYSIS PROGRAM - INPUT SELECTION");
                Console.WriteLine("1. Run Built-in Homework Sample");
                Console.WriteLine("2. Manual Console Entry (Quick Case)");
                Console.WriteLine("3. Build Portal Frame Input");

                Console.Write("\nSelect option: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        input = BuildHomeworkSampleInput();
                        break;

                    case "2":
                        input = BuildManualInput();
                        break;

                    case "3":
                        input = BuildPortalFrameInput();
                        break;

                    default:
                        Console.WriteLine("Invalid selection. Defaulting to Homework Sample.");
                        input = BuildHomeworkSampleInput();
                        break;
                }

                // --- Analysis Execution ---
                RunAnalysis(input);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nCRITICAL ERROR: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        private static void RunAnalysis(StructureInputData input)
        {
            StructureModelBuilder modelBuilder = new StructureModelBuilder();
            StructureModel model = modelBuilder.Build(input);

            DisplacementMapper displacementMapper = new DisplacementMapper();
            ElementForceRecovery elementForceRecovery = new ElementForceRecovery(displacementMapper);

            FrameAnalyzer analyzer = new FrameAnalyzer(
                new DofNumberingService(),
                new GlobalStiffnessAssembler(),
                new LoadVectorBuilder(),
                new SparseLDLtSolver(),
                displacementMapper,
                elementForceRecovery);

            FrameAnalysisResult result = analyzer.Analyze(model);

            ResultPrinter printer = new ResultPrinter();
            printer.PrintModel(model);
            printer.PrintAnalysisResult(result);
        }

        #region Input Methods


        // METHOD 2: Manual Interactive Input (Simplistic)
        private static StructureInputData BuildManualInput()
        {
            Console.WriteLine("\n--- QUICK MANUAL INPUT (2 Nodes, 1 Element) ---");
            Console.Write("Enter E (Modulus): ");
            double e = double.Parse(Console.ReadLine());

            return new StructureInputData
            {
                NodeTable = new double[,] { { 0, 0 }, { 5, 0 } },
                MaterialTable = new double[,] { { e } },
                SectionTable = new double[,] { { 0.01, 0.001 } },
                ElementTable = new int[,] { { 1, 2, 1, 1 } },
                SupportTable = new int[,] { { 1, 1, 1, 1 } }, // Fixed at N1
                LoadTable = new double[,] { { 2, 100, 0, 0 } } // 100kN at N2
            };
        }

        private static StructureInputData BuildPortalFrameInput()
        {
            return new StructureInputData
            {
                // Nodes: (X, Y)
                NodeTable = new double[,]
                {
            { 0, 0 },   // Node 1: Base of left column
            { 0, 3 },   // Node 2: Top of left column
            { 4, 3 },   // Node 3: Top of right column
            { 4, 0 }    // Node 4: Base of right column
                },

                // Material: E = 200,000 (e.g., MPa or kN/m^2 depending on your units)
                MaterialTable = new double[,] { { 200000.0 } },

                // Section: A = 0.01, I = 0.0001
                SectionTable = new double[,] { { 0.01, 0.0001 } },

                // Elements: [StartNode, EndNode, MatId, SecId]
                ElementTable = new int[,]
                {
            { 1, 2, 1, 1 }, // Element 1: Left Column (Vertical)
            { 2, 3, 1, 1 }, // Element 2: Top Beam (Horizontal)
            { 4, 3, 1, 1 }  // Element 3: Right Column (Vertical - N4 to N3)
                },

                // Supports: [Node, Ux, Uy, Rz] (1 = Fixed, 0 = Free)
                SupportTable = new int[,]
                {
            { 1, 1, 1, 1 }, // Node 1: Fixed Base
            { 4, 1, 1, 1 }  // Node 4: Fixed Base
                },

                // Loads: [Node, Fx, Fy, Mz]
                LoadTable = new double[,]
                {
            { 2, 50.0, 0, 0 } // 50 units horizontal force at Node 2
                }
            };
        }


        // Existing Method
        private static StructureInputData BuildHomeworkSampleInput()
        {
            return new StructureInputData
            {
                NodeTable = new double[,] { { 0, 0 }, { 0, 3 }, { 4, 3 }, { 4, 0 } },
                MaterialTable = new double[,] { { 200000.0 } },
                SectionTable = new double[,] { { 0.02, 0.08 }, { 0.01, 0.01 } },
                ElementTable = new int[,] { { 1, 2, 1, 1 }, { 2, 3, 1, 1 }, { 4, 3, 1, 1 }, { 1, 3, 1, 2 } },
                SupportTable = new int[,] { { 1, 1, 1, 0 }, { 4, 0, 1, 0 } },
                LoadTable = new double[,] { { 2, 10.0, -10.0, 0.0 }, { 3, 10.0, -10.0, 0.0 } }
            };
        }

        #endregion
    }
}

#endregion