using System;
using System.Collections.Generic;
using System.Text;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Loads.Interfaces;

namespace FrameAnalysisProgram.STRUCTURAL_MODEL
{
    /// <summary>
    /// Represents the full structural model for 2D frame analysis.
    /// </summary>
    public class StructureModel
    {
        public List<Node> Nodes { get; }
        public List<FrameElement2D> Elements { get; }
        public List<Material> Materials { get; }
        public List<SectionProperty> Sections { get; }
        public List<SupportCondition> Supports { get; }
        public List<JointLoad> Loads { get; }

        /// <summary>
        /// Loads applied along members (distributed, point-on-span, temperature).
        /// Assembled into the load vector as equivalent nodal loads and added back
        /// during element end-force recovery.
        /// </summary>
        public List<IMemberLoad> MemberLoads { get; }

        public StructureModel()
        {
            Nodes = new List<Node>();
            Elements = new List<FrameElement2D>();
            Materials = new List<Material>();
            Sections = new List<SectionProperty>();
            Supports = new List<SupportCondition>();
            Loads = new List<JointLoad>();
            MemberLoads = new List<IMemberLoad>();
        }
    }
}
