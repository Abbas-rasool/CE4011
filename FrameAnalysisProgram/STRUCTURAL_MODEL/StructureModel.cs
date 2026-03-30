using System;
using System.Collections.Generic;
using System.Text;

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

        public StructureModel()
        {
            Nodes = new List<Node>();
            Elements = new List<FrameElement2D>();
            Materials = new List<Material>();
            Sections = new List<SectionProperty>();
            Supports = new List<SupportCondition>();
            Loads = new List<JointLoad>();
        }
    }
}
