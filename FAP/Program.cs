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
                Console.WriteLine("3. Build Portal Frame Input (with member load)");
                Console.WriteLine("4. Two-Bar Truss Example");

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

                    case "4":
                        input = BuildTrussExampleInput();
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
                new CSparseCholeskySolver(),
                displacementMapper,
                elementForceRecovery,
                new ReactionRecovery());

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

                ElementTable = new int[,] { { 1, 2, 1, 1, 0, 0 } }, // [Start,End,Mat,Sec,Type(0=Frame),Release(0=None)]
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

                // Elements: [StartNode, EndNode, MatId, SecId, Type(0=Frame,1=Truss), Release(0=None,1=Start,2=End,3=Both)]
                ElementTable = new int[,]
                {
            { 1, 2, 1, 1, 0, 0 }, // Element 1: Left Column (Vertical)
            { 2, 3, 1, 1, 0, 0 }, // Element 2: Top Beam (Horizontal)
            { 4, 3, 1, 1, 0, 0 }  // Element 3: Right Column (Vertical - N4 to N3)
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
                },

                // Member loads: a downward UDL of 10/length on the top beam (Element 2).
                // [ElementId, MagnitudePerLength, Direction(0=X,1=Y)]
                DistributedLoadTable = new double[,]
                {
            { 2, -10.0, 1 }
                }
            };
        }


        // Statically determinate two-bar truss (apex point load).
        private static StructureInputData BuildTrussExampleInput()
        {
            return new StructureInputData
            {
                // Node 1 and 2 at the base, Node 3 at the apex.
                NodeTable = new double[,] { { 0, 0 }, { 4, 0 }, { 2, 3 } },
                MaterialTable = new double[,] { { 200000.0 } },
                SectionTable = new double[,] { { 0.1, 0.1, 0.0001 } }, // [Width, Length, I] → A = 0.01

                // Both members are trusses (Type = 1); release is ignored for trusses.
                ElementTable = new int[,]
                {
                    { 1, 3, 1, 1, 1, 0 }, // Element 1: bottom-left bar
                    { 2, 3, 1, 1, 1, 0 }  // Element 2: bottom-right bar
                },

                // Pin both bases. Rz is auto-restrained at truss-only nodes.
                SupportTable = new int[,] { { 1, 1, 1, 0 }, { 2, 1, 1, 0 } },

                // 20-unit downward load at the apex.
                LoadTable = new double[,] { { 3, 0.0, -20.0, 0.0 } }
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
                ElementTable = new int[,] { { 1, 2, 1, 1, 0, 0 }, { 2, 3, 1, 1, 0, 0 }, { 4, 3, 1, 1, 0, 0 }, { 1, 3, 1, 2, 0, 0 } }, // [Start,End,Mat,Sec,Type,Release]
                SupportTable = new int[,] { { 1, 1, 1, 0 }, { 4, 0, 1, 0 } },
                LoadTable = new double[,] { { 2, 10.0, -10.0, 0.0 }, { 3, 10.0, -10.0, 0.0 } }
            };
        }

        #endregion
    }
}