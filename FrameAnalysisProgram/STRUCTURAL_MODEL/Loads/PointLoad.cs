using System;
using FrameAnalysisProgram.ANALYSIS_CORE;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Loads.Interfaces;
using Matrix_Library.MAIN_TYPES;

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
            double a = DistanceFromStart;
            double b = L - a;

            // Resolve the global load into local axial / transverse components.
            (double gx, double gy) = GlobalDirectionComponents(Direction, Magnitude);
            double c = Element.CosX;
            double s = Element.SinX;
            double pAxial = c * gx + s * gy;        // along local x
            double pTransverse = -s * gx + c * gy;  // along local y

            double l2 = L * L;
            double l3 = l2 * L;

            double[] f = new double[6];

            // Axial: lever-arm split.
            f[0] = -pAxial * b / L;
            f[3] = -pAxial * a / L;

            // Transverse: fixed-fixed shear and moment reactions for P at distance a.
            f[1] = -pTransverse * b * b * (3.0 * a + b) / l3;
            f[2] = -pTransverse * a * b * b / l2;
            f[4] = -pTransverse * a * a * (a + 3.0 * b) / l3;
            f[5] = pTransverse * a * a * b / l2;

            return f;
        }

        /// <summary>
        /// Global (X, Y) components of a load of the given magnitude acting in the
        /// specified global direction. A concentrated moment (Z) is not supported.
        /// </summary>
        private static (double, double) GlobalDirectionComponents(LoadDirection direction, double magnitude)
        {
            switch (direction)
            {
                case LoadDirection.X: return (magnitude, 0.0);
                case LoadDirection.Y: return (0.0, magnitude);
                default:
                    throw new NotSupportedException(
                        "Concentrated moment loads (LoadDirection.Z) are not supported.");
            }
        }
    }
}
