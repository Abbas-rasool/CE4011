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
                SectionTable = new double[,] { { 0.1, 0.1, 0.001 } }, // [Width, Length, I] → A = 0.01

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

                // Section: [Width, Length, I] → A = 0.01, I = 0.0001
                SectionTable = new double[,] { { 0.1, 0.1, 0.0001 } },

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
                SectionTable = new double[,] { { 0.2, 0.1, 0.08 }, { 0.1, 0.1, 0.01 } }, // [Width, Length, I]
                ElementTable = new int[,] { { 1, 2, 1, 1 }, { 2, 3, 1, 1 }, { 4, 3, 1, 1 }, { 1, 3, 1, 2 } },
                SupportTable = new int[,] { { 1, 1, 1, 0 }, { 4, 0, 1, 0 } },
                LoadTable = new double[,] { { 2, 10.0, -10.0, 0.0 }, { 3, 10.0, -10.0, 0.0 } }
            };
        }

        #endregion
    }
}