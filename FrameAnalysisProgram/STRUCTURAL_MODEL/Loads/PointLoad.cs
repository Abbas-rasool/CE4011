using System;

namespace FrameAnalysisProgram.STRUCTURAL_MODEL
{
    /// <summary>
    /// Represents a point load at a specific location on a member.
    /// Converts to equivalent nodal loads using beam theory.
    /// </summary>
    public class PointLoad : IMemberLoad
    {
        /// <summary>
        /// The member (element) where the load is applied.
        /// </summary>
        public FrameElement2D Element { get; }

        /// <summary>
        /// Distance from the start node along the member axis.
        /// Units: length
        /// </summary>
        public double DistanceFromStart { get; }

        /// <summary>
        /// Magnitude of the point load.
        /// Units: force
        /// </summary>
        public double Magnitude { get; }

        /// <summary>
        /// Direction of the load in global coordinates.
        /// </summary>
        public LoadDirection Direction { get; }

        public PointLoad(
            FrameElement2D element,
            double distanceFromStart,
            double magnitude,
            LoadDirection direction)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
            
            if (distanceFromStart < 0)
                throw new ArgumentException("Distance from start must be non-negative.", nameof(distanceFromStart));
            
            if (distanceFromStart > element.Length)
                throw new ArgumentException(
                    $"Distance from start ({distanceFromStart}) exceeds member length ({element.Length}).", 
                    nameof(distanceFromStart));

            DistanceFromStart = distanceFromStart;
            Magnitude = magnitude;
            Direction = direction;
        }

        public void AssembleIntoVector(CustomVector globalLoadVector, DofMap dofMap, StructureModel model)
        {
            if (globalLoadVector == null)
                throw new ArgumentNullException(nameof(globalLoadVector));
            if (dofMap == null)
                throw new ArgumentNullException(nameof(dofMap));

            // Standard beam formulas for concentrated load P at distance a from start
            // Force at start node: F_start = P * b / L  (where b = L - a)
            // Force at end node:   F_end = P * a / L
            
            double length = Element.Length;
            double a = DistanceFromStart;
            double b = length - a;

            double forceAtStart = Magnitude * b / length;
            double forceAtEnd = Magnitude * a / length;

            AssembleAtNode(globalLoadVector, dofMap, Element.StartNode, forceAtStart);
            AssembleAtNode(globalLoadVector, dofMap, Element.EndNode, forceAtEnd);
        }

        private void AssembleAtNode(
            CustomVector globalLoadVector,
            DofMap dofMap,
            Node node,
            double force)
        {
            int eq = Direction switch
            {
                LoadDirection.X => dofMap.GetEquation(node.Id, 0),
                LoadDirection.Y => dofMap.GetEquation(node.Id, 1),
                LoadDirection.Z => dofMap.GetEquation(node.Id, 2),
                _ => 0
            };

            if (eq != 0)
                globalLoadVector.AddToEntry(eq - 1, force);
        }
    }
}
