using System;
using System.Collections.Generic;
using System.Text;

namespace FrameAnalysisProgram.STRUCTURAL_MODEL
{
    /// <summary>
    /// Represents cross-sectional properties of a frame member.
    /// </summary>
    public class SectionProperty
    {
        /// <summary>
        /// Unique section property identifier.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Cross-sectional area.
        /// Units: length^2
        /// </summary>
        public double Area { get; }

        /// <summary>
        /// Second moment of area about the out-of-plane axis.
        /// Units: length^4
        /// </summary>
        public double MomentOfInertia { get; }

        public SectionProperty(int id, double area, double momentOfInertia)
        {
            Id = id;
            Area = area;
            MomentOfInertia = momentOfInertia;
        }
    }
}
