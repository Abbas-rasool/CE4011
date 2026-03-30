
namespace FrameAnalysisProgram.INPUT_OUTPUT
{
    /// <summary>
    /// Stores raw structural input data in table form.
    /// This class is used as an intermediate input container before
    /// building the object-oriented structural model.
    /// 
    /// Assumptions:
    /// - Node IDs, element IDs, material IDs, and section IDs are 1-based.
    /// - Coordinates are given in the global coordinate system.
    /// - Supports use 0 = free, 1 = restrained.
    /// - Loads are given in global coordinates.
    /// </summary>
    public class StructureInputData
    {
        /// <summary>
        /// Node coordinate table.
        /// Columns: [X, Y]
        /// Row index + 1 = Node ID
        /// Units: length
        /// </summary>
        public required double[,] NodeTable { get; set; }

        /// <summary>
        /// Element connectivity table.
        /// Columns: [StartNodeId, EndNodeId, MaterialId, SectionId]
        /// Row index + 1 = Element ID
        /// </summary>
        public required int[,] ElementTable { get; set; }


        /// <summary>
        /// Material property table.
        /// Columns: [ElasticModulus]
        /// Row index + 1 = Material ID
        /// Units: force / length^2
        /// </summary>
        public required double[,] MaterialTable { get; set; }

        /// <summary>
        /// Section property table.
        /// Columns: [Area, MomentOfInertia]
        /// Row index + 1 = Section ID
        /// Units: Area = length^2, I = length^4
        /// </summary>
        public required double[,] SectionTable { get; set; }

        /// <summary>
        /// Support condition table.
        /// Columns: [NodeId, Rx, Ry, Rz]
        /// 0 = free, 1 = restrained
        /// </summary>
        public required int[,] SupportTable { get; set; }

        /// <summary>
        /// Joint load table.
        /// Columns: [NodeId, Fx, Fy, Mz]
        /// Units: force, force, force*length
        /// </summary>
        public required double[,] LoadTable { get; set; }

    }
}