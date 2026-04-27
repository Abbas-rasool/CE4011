using System;

namespace FrameAnalysisProgram.STRUCTURAL_MODEL
{
    /// <summary>
    /// Represents a uniformly distributed load on a member.
    /// Converts to equivalent nodal forces at both ends.
    /// </summary>
    public class UniformDistributedLoad : IMemberLoad
    {
        /// <summary>
        /// The member (element) where the load is applied.
        /// </summary>
        public FrameElement2D Element { get; }

        /// <summary>
        /// Load magnitude per unit length.
        /// Units: force / length
        /// </summary>
        public double MagnitudePerLength { get; }

        /// <summary>
        /// Direction of the load in global coordinates.
        /// </summary>
        public LoadDirection Direction { get; }

        public UniformDistributedLoad(
            FrameElement2D element,
            double magnitudePerLength,
            LoadDirection direction)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
            MagnitudePerLength = magnitudePerLength;
            Direction = direction;
        }

        public void AssembleIntoVector(CustomVector globalLoadVector, DofMap dofMap, StructureModel model)
        {
            if (globalLoadVector == null)
                throw new ArgumentNullException(nameof(globalLoadVector));
            if (dofMap == null)
                throw new ArgumentNullException(nameof(dofMap));

            // For a uniformly distributed load w over length L:
            // Total load = w * L
            // Split equally: half at each node = (w * L) / 2
            
            double length = Element.Length;
            double totalLoad = MagnitudePerLength * length;
            double forceAtEachNode = totalLoad / 2.0;

            AssembleAtNode(globalLoadVector, dofMap, Element.StartNode, forceAtEachNode);
            AssembleAtNode(globalLoadVector, dofMap, Element.EndNode, forceAtEachNode);
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
