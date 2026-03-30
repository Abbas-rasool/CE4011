using System;
using System.Collections.Generic;
using System.Text;

namespace FrameAnalysisProgram.STRUCTURAL_MODEL
{
    /// <summary>
    /// Represents material properties used by frame elements.
    /// </summary>
    public class Material
    {
        /// <summary>
        /// Unique material identifier.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Modulus of elasticity.
        /// Units: force / length^2
        /// </summary>
        public double ElasticModulus { get; }

        public Material(int id, double elasticModulus)
        {
            Id = id;
            ElasticModulus = elasticModulus;
        }
    }
}
