using System;
using System.Collections.Generic;
using FrameAnalysisProgram.STRUCTURAL_MODEL;

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

            BuildElements(input, model, nodeMap, materialMap, sectionMap);
            BuildSupports(input, model, nodeMap);
            BuildLoads(input, model, nodeMap);

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

            if (input.ElementTable.GetLength(1) != 4)
                throw new InvalidOperationException("ElementTable must have 4 columns: [StartNodeId, EndNodeId, MaterialId, SectionId].");

            if (input.MaterialTable.GetLength(1) != 1)
                throw new InvalidOperationException("MaterialTable must have 1 column: [ElasticModulus].");

            if (input.SectionTable.GetLength(1) != 2)
                throw new InvalidOperationException("SectionTable must have 2 columns: [Area, MomentOfInertia].");

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
                double area = input.SectionTable[i, 0];
                double momentOfInertia = input.SectionTable[i, 1];

                SectionProperty section = new SectionProperty(sectionId, area, momentOfInertia);

                model.Sections.Add(section);
                sectionMap.Add(sectionId, section);
            }

            return sectionMap;
        }

        private void BuildElements(
            StructureInputData input,
            StructureModel model,
            Dictionary<int, Node> nodeMap,
            Dictionary<int, Material> materialMap,
            Dictionary<int, SectionProperty> sectionMap)
        {
            int elementCount = input.ElementTable.GetLength(0);

            for (int i = 0; i < elementCount; i++)
            {
                int elementId = i + 1;

                int startNodeId = input.ElementTable[i, 0];
                int endNodeId = input.ElementTable[i, 1];
                int materialId = input.ElementTable[i, 2];
                int sectionId = input.ElementTable[i, 3];

                if (!nodeMap.TryGetValue(startNodeId, out Node startNode))
                    throw new InvalidOperationException($"Element {elementId} references undefined start node ID {startNodeId}.");

                if (!nodeMap.TryGetValue(endNodeId, out Node endNode))
                    throw new InvalidOperationException($"Element {elementId} references undefined end node ID {endNodeId}.");

                if (!materialMap.TryGetValue(materialId, out Material material))
                    throw new InvalidOperationException($"Element {elementId} references undefined material ID {materialId}.");

                if (!sectionMap.TryGetValue(sectionId, out SectionProperty section))
                    throw new InvalidOperationException($"Element {elementId} references undefined section ID {sectionId}.");

                FrameElement2D element = new FrameElement2D(
                    elementId,
                    startNode,
                    endNode,
                    material,
                    section);

                model.Elements.Add(element);
            }
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

                JointLoad load = new JointLoad(node, fx, fy, mz);

                model.Loads.Add(load);
            }
        }
    }
}