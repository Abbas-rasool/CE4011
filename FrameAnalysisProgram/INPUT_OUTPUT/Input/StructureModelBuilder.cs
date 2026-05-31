using System;
using System.Collections.Generic;
using FrameAnalysisProgram.STRUCTURAL_MODEL;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Elements;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Geometry;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Loads;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Loads.Members;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Properties;

namespace FrameAnalysisProgram.INPUT_OUTPUT
{
    /// <summary>
    /// Builds an object-oriented StructureModel from raw tabular input data.
    /// 
    /// Purpose:
    /// Converts array-based structural input into strongly typed model objects.
    /// 
    /// Inputs:
    /// - StructureInputData containing node, element, material, section,
    ///   support, and load tables.
    /// 
    /// Output:
    /// - StructureModel ready for structural analysis.
    /// 
    /// Assumptions:
    /// - All IDs are 1-based and consistent.
    /// - Referenced node/material/section IDs exist in their respective tables.
    /// - Input tables use the required column formats.
    /// </summary>
    public class StructureModelBuilder
    {
        public StructureModel Build(StructureInputData input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            ValidateInputTables(input);

            StructureModel model = new StructureModel();

            Dictionary<int, Node> nodeMap = BuildNodes(input, model);
            Dictionary<int, Material> materialMap = BuildMaterials(input, model);
            Dictionary<int, SectionProperty> sectionMap = BuildSections(input, model);

            Dictionary<int, StructuralElement2D> elementMap =
                BuildElements(input, model, nodeMap, materialMap, sectionMap);

            BuildSupports(input, model, nodeMap);
            BuildLoads(input, model, nodeMap);
            BuildMemberLoads(input, model, elementMap);

            return model;
        }

        private void ValidateInputTables(StructureInputData input)
        {
            if (input.NodeTable == null)
                throw new InvalidOperationException("NodeTable is required.");

            if (input.ElementTable == null)
                throw new InvalidOperationException("ElementTable is required.");

            if (input.MaterialTable == null)
                throw new InvalidOperationException("MaterialTable is required.");

            if (input.SectionTable == null)
                throw new InvalidOperationException("SectionTable is required.");

            if (input.SupportTable == null)
                throw new InvalidOperationException("SupportTable is required.");

            if (input.LoadTable == null)
                throw new InvalidOperationException("LoadTable is required.");

            if (input.NodeTable.GetLength(1) != 2)
                throw new InvalidOperationException("NodeTable must have 2 columns: [X, Y].");

            if (input.ElementTable.GetLength(1) != 6)
                throw new InvalidOperationException("ElementTable must have 6 columns: [StartNodeId, EndNodeId, MaterialId, SectionId, ElementType, MomentRelease].");

            if (input.DistributedLoadTable != null && input.DistributedLoadTable.GetLength(1) != 3)
                throw new InvalidOperationException("DistributedLoadTable must have 3 columns: [ElementId, MagnitudePerLength, Direction].");

            if (input.PointLoadTable != null && input.PointLoadTable.GetLength(1) != 4)
                throw new InvalidOperationException("PointLoadTable must have 4 columns: [ElementId, DistanceFromStart, Magnitude, Direction].");

            if (input.MaterialTable.GetLength(1) != 1)
                throw new InvalidOperationException("MaterialTable must have 1 column: [ElasticModulus].");

            if (input.SectionTable.GetLength(1) != 3)
                throw new InvalidOperationException("SectionTable must have 3 columns: [Width, Length, MomentOfInertia].");

            if (input.SupportTable.GetLength(1) != 4)
                throw new InvalidOperationException("SupportTable must have 4 columns: [NodeId, Rx, Ry, Rz].");

            if (input.LoadTable.GetLength(1) != 4)
                throw new InvalidOperationException("LoadTable must have 4 columns: [NodeId, Fx, Fy, Mz].");
        }

        private Dictionary<int, Node> BuildNodes(StructureInputData input, StructureModel model)
        {
            Dictionary<int, Node> nodeMap = new Dictionary<int, Node>();

            int nodeCount = input.NodeTable.GetLength(0);

            for (int i = 0; i < nodeCount; i++)
            {
                int nodeId = i + 1;
                double x = input.NodeTable[i, 0];
                double y = input.NodeTable[i, 1];

                Node node = new Node(nodeId, x, y);

                model.Nodes.Add(node);
                nodeMap.Add(nodeId, node);
            }

            return nodeMap;
        }

        private Dictionary<int, Material> BuildMaterials(StructureInputData input, StructureModel model)
        {
            Dictionary<int, Material> materialMap = new Dictionary<int, Material>();

            int materialCount = input.MaterialTable.GetLength(0);

            for (int i = 0; i < materialCount; i++)
            {
                int materialId = i + 1;
                double elasticModulus = input.MaterialTable[i, 0];

                Material material = new Material(materialId, elasticModulus);

                model.Materials.Add(material);
                materialMap.Add(materialId, material);
            }

            return materialMap;
        }

        private Dictionary<int, SectionProperty> BuildSections(StructureInputData input, StructureModel model)
        {
            Dictionary<int, SectionProperty> sectionMap = new Dictionary<int, SectionProperty>();

            int sectionCount = input.SectionTable.GetLength(0);

            for (int i = 0; i < sectionCount; i++)
            {
                int sectionId = i + 1;
                double width = input.SectionTable[i, 0];
                double length = input.SectionTable[i, 1];
                double momentOfInertia = input.SectionTable[i, 2];

                SectionProperty section = new SectionProperty(sectionId, width, length, momentOfInertia);

                model.Sections.Add(section);
                sectionMap.Add(sectionId, section);
            }

            return sectionMap;
        }

        private Dictionary<int, StructuralElement2D> BuildElements(
            StructureInputData input,
            StructureModel model,
            Dictionary<int, Node> nodeMap,
            Dictionary<int, Material> materialMap,
            Dictionary<int, SectionProperty> sectionMap)
        {
            Dictionary<int, StructuralElement2D> elementMap = new Dictionary<int, StructuralElement2D>();

            int elementCount = input.ElementTable.GetLength(0);

            for (int i = 0; i < elementCount; i++)
            {
                int elementId = i + 1;

                int startNodeId = input.ElementTable[i, 0];
                int endNodeId = input.ElementTable[i, 1];
                int materialId = input.ElementTable[i, 2];
                int sectionId = input.ElementTable[i, 3];
                int elementType = input.ElementTable[i, 4];
                int releaseCode = input.ElementTable[i, 5];

                if (!nodeMap.TryGetValue(startNodeId, out Node startNode))
                    throw new InvalidOperationException($"Element {elementId} references undefined start node ID {startNodeId}.");

                if (!nodeMap.TryGetValue(endNodeId, out Node endNode))
                    throw new InvalidOperationException($"Element {elementId} references undefined end node ID {endNodeId}.");

                if (!materialMap.TryGetValue(materialId, out Material material))
                    throw new InvalidOperationException($"Element {elementId} references undefined material ID {materialId}.");

                if (!sectionMap.TryGetValue(sectionId, out SectionProperty section))
                    throw new InvalidOperationException($"Element {elementId} references undefined section ID {sectionId}.");

                StructuralElement2D element;

                if (elementType == 1)
                {
                    element = new TrussElement2D(elementId, startNode, endNode, material, section);
                }
                else
                {
                    if (releaseCode < 0 || releaseCode > 3)
                        throw new InvalidOperationException(
                            $"Element {elementId} has invalid moment release code {releaseCode} (expected 0-3).");

                    element = new FrameElement2D(
                        elementId,
                        startNode,
                        endNode,
                        material,
                        section,
                        (MomentRelease)releaseCode);
                }

                model.Elements.Add(element);
                elementMap.Add(elementId, element);
            }

            return elementMap;
        }

        private void BuildMemberLoads(
            StructureInputData input,
            StructureModel model,
            Dictionary<int, StructuralElement2D> elementMap)
        {
            if (input.DistributedLoadTable != null)
            {
                int rows = input.DistributedLoadTable.GetLength(0);
                for (int i = 0; i < rows; i++)
                {
                    int elementId = (int)input.DistributedLoadTable[i, 0];
                    double magnitude = input.DistributedLoadTable[i, 1];
                    LoadDirection direction = (LoadDirection)(int)input.DistributedLoadTable[i, 2];

                    FrameElement2D frame = ResolveFrameElement(elementMap, elementId, "Distributed");
                    model.MemberLoads.Add(new UniformDistributedLoad(frame, magnitude, direction));
                }
            }

            if (input.PointLoadTable != null)
            {
                int rows = input.PointLoadTable.GetLength(0);
                for (int i = 0; i < rows; i++)
                {
                    int elementId = (int)input.PointLoadTable[i, 0];
                    double distance = input.PointLoadTable[i, 1];
                    double magnitude = input.PointLoadTable[i, 2];
                    LoadDirection direction = (LoadDirection)(int)input.PointLoadTable[i, 3];

                    FrameElement2D frame = ResolveFrameElement(elementMap, elementId, "Point");
                    model.MemberLoads.Add(new PointLoad(frame, distance, magnitude, direction));
                }
            }
        }

        private static FrameElement2D ResolveFrameElement(
            Dictionary<int, StructuralElement2D> elementMap,
            int elementId,
            string loadKind)
        {
            if (!elementMap.TryGetValue(elementId, out StructuralElement2D element))
                throw new InvalidOperationException($"{loadKind} load references undefined element ID {elementId}.");

            if (element is not FrameElement2D frame)
                throw new InvalidOperationException(
                    $"{loadKind} load on element {elementId} is not supported: member loads apply to frame elements only.");

            return frame;
        }

        private void BuildSupports(
            StructureInputData input,
            StructureModel model,
            Dictionary<int, Node> nodeMap)
        {
            int supportCount = input.SupportTable.GetLength(0);

            for (int i = 0; i < supportCount; i++)
            {
                int nodeId = input.SupportTable[i, 0];

                if (!nodeMap.TryGetValue(nodeId, out Node node))
                    throw new InvalidOperationException($"Support row {i + 1} references undefined node ID {nodeId}.");

                bool restrainsUx = input.SupportTable[i, 1] == 1;
                bool restrainsUy = input.SupportTable[i, 2] == 1;
                bool restrainsRz = input.SupportTable[i, 3] == 1;

                SupportCondition support = new SupportCondition(
                    node,
                    restrainsUx,
                    restrainsUy,
                    restrainsRz);

                model.Supports.Add(support);
            }
        }

        private void BuildLoads(
            StructureInputData input,
            StructureModel model,
            Dictionary<int, Node> nodeMap)
        {
            int loadCount = input.LoadTable.GetLength(0);

            for (int i = 0; i < loadCount; i++)
            {
                int nodeId = (int)input.LoadTable[i, 0];

                if (!nodeMap.TryGetValue(nodeId, out Node node))
                    throw new InvalidOperationException($"Load row {i + 1} references undefined node ID {nodeId}.");

                double fx = input.LoadTable[i, 1];
                double fy = input.LoadTable[i, 2];
                double mz = input.LoadTable[i, 3];

                NodalLoad load = new NodalLoad(node, fx, fy, mz);

                model.Loads.Add(load);
            }
        }
    }
}