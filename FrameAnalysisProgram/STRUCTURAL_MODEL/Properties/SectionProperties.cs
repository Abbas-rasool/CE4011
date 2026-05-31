using System;
using System.Collections.Generic;
using System.Text;

namespace FrameAnalysisProgram.STRUCTURAL_MODEL.Properties
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
        /// Section width.
        /// Units: length
        /// </summary>
        public double Width { get; }

        /// <summary>
        /// Section height/depth.
        /// Units: length
        /// </summary>
        public double Length { get; }

        /// <summary>
        /// Cross-sectional area.
        /// Computed automatically as Width × Length.
        /// Units: length^2
        /// </summary>
        public double Area => Width * Length;

        /// <summary>
        /// Second moment of area about the out-of-plane axis.
        /// Units: length^4
        /// </summary>
        public double MomentOfInertia { get; }

        public SectionProperty(
            int id,
            double width,
            double length,
            double momentOfInertia)
        {
            Id = id;
            Width = width;
            Length = length;
            MomentOfInertia = momentOfInertia;
        }
    }
}