
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
        /// Columns: [StartNodeId, EndNodeId, MaterialId, SectionId, ElementType, MomentRelease]
        /// Row index + 1 = Element ID
        /// ElementType: 0 = Frame, 1 = Truss
        /// MomentRelease (frame only): 0 = None, 1 = Start, 2 = End, 3 = Both
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
        /// Columns: [Width, Length, MomentOfInertia]
        /// Row index + 1 = Section ID
        /// Units: Width = length, Length(depth) = length, I = length^4
        /// Area is derived as Width × Length.
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
        /// Columns: [NodeId, Fx, Fy, Mz, (Nature)]
        /// Units: force, force, force*length
        /// Nature (optional trailing column) is an <see cref="StructuralLoads.eLoadNature"/> code;
        /// when omitted the load defaults to <see cref="StructuralLoads.eLoadNature.Dead"/>.
        /// </summary>
        public required double[,] LoadTable { get; set; }

        /// <summary>
        /// Uniformly distributed member load table (optional).
        /// Columns: [ElementId, MagnitudePerLength, Direction, (Nature)]
        /// Direction: 0 = global X, 1 = global Y
        /// Nature (optional trailing column) is an <see cref="StructuralLoads.eLoadNature"/> code;
        /// when omitted the load defaults to <see cref="StructuralLoads.eLoadNature.Dead"/>.
        /// Frame elements only.
        /// </summary>
        public double[,]? DistributedLoadTable { get; set; }

        /// <summary>
        /// Concentrated member load table (optional).
        /// Columns: [ElementId, DistanceFromStart, Magnitude, Direction, (Nature)]
        /// Direction: 0 = global X, 1 = global Y
        /// Nature (optional trailing column) is an <see cref="StructuralLoads.eLoadNature"/> code;
        /// when omitted the load defaults to <see cref="StructuralLoads.eLoadNature.Dead"/>.
        /// Frame elements only.
        /// </summary>
        public double[,]? PointLoadTable { get; set; }

        /// <summary>
        /// Support settlement (prescribed displacement) table (optional).
        /// Columns: [NodeId, dUx, dUy, dRz]
        /// Units: length, length, radians. A value is only applied at a DOF that
        /// is restrained in the support table.
        /// </summary>
        public double[,]? SettlementTable { get; set; }

        /// <summary>
        /// Thermal (temperature) load table (optional).
        /// Columns: [ElementId, UniformTemperatureChange, TemperatureGradient,
        ///           ThermalExpansionCoefficient, MemberDepth]
        /// Frame elements only.
        /// </summary>
        public double[,]? TemperatureLoadTable { get; set; }
    }
}