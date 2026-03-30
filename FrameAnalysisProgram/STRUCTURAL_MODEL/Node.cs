using System;
using System.Collections.Generic;
using System.Text;

namespace FrameAnalysisProgram.STRUCTURAL_MODEL
{
    /// <summary>
    /// Represents a 2D structural node in the global coordinate system.
    /// </summary>
    public class Node
    {
        /// <summary>
        /// Unique node identifier.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Global X coordinate of the node.
        /// Units: length
        /// </summary>
        public double X { get; }

        /// <summary>
        /// Global Y coordinate of the node.
        /// Units: length
        /// </summary>
        public double Y { get; }

        public Node(int id, double x, double y)
        {
            Id = id;
            X = x;
            Y = y;
        }
    }
}
