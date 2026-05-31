using System;
using FrameAnalysisProgram.ANALYSIS_CORE;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Elements;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Loads.Interfaces;
using Matrix_Library.MAIN_TYPES;

namespace FrameAnalysisProgram.STRUCTURAL_MODEL.Loads.Members
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

            double[] equivalentLoads = Element.GetEquivalentGlobalNodalLoads(GetLocalFixedEndForces());
            int[] dofIndices = Element.GetGlobalDofIndices(dofMap);

            for (int i = 0; i < 6; i++)
            {
                int eq = dofIndices[i];
                if (eq != 0)
                    globalLoadVector.AddToEntry(eq - 1, equivalentLoads[i]);
            }
        }

        public double[] GetLocalFixedEndForces()
        {
            double L = Element.Length;

            // Resolve the global intensity into local axial / transverse components.
            (double gx, double gy) = GlobalDirectionComponents(Direction, MagnitudePerLength);
            double c = Element.CosX;
            double s = Element.SinX;
            double qAxial = c * gx + s * gy;        // along local x
            double qTransverse = -s * gx + c * gy;  // along local y

            double[] f = new double[6];

            // Axial: split equally to both ends.
            f[0] = -qAxial * L / 2.0;
            f[3] = -qAxial * L / 2.0;

            // Transverse: fixed-fixed shear and moment reactions.
            f[1] = -qTransverse * L / 2.0;
            f[2] = -qTransverse * L * L / 12.0;
            f[4] = -qTransverse * L / 2.0;
            f[5] = qTransverse * L * L / 12.0;

            return f;
        }

        /// <summary>
        /// Global (X, Y) components of a load of the given magnitude acting in the
        /// specified global direction. A distributed moment (Z) is not supported.
        /// </summary>
        private static (double, double) GlobalDirectionComponents(LoadDirection direction, double magnitude)
        {
            switch (direction)
            {
                case LoadDirection.X: return (magnitude, 0.0);
                case LoadDirection.Y: return (0.0, magnitude);
                default:
                    throw new NotSupportedException(
                        "Distributed moment loads (LoadDirection.Z) are not supported.");
            }
        }
    }
}
