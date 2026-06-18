using System;
using System.Collections.Generic;

namespace FrameAnalysisProgram.ANALYSIS_CORE
{
    /// <summary>
    /// Post-processing result for a single member: the section forces (N, V, M) and the
    /// deflected shape sampled at a series of station points along the member axis.
    ///
    /// Conventions (local member axes, x measured from the start node):
    ///   - <see cref="Axial"/>  N(x): tension positive.
    ///   - <see cref="Shear"/>  V(x): consistent with dM/dx = V.
    ///   - <see cref="Moment"/> M(x): sagging positive.
    ///   - <see cref="DeflectionAxial"/> / <see cref="DeflectionTransverse"/>: the local
    ///     axial (u) and transverse (v) deflection, in solver length units and *unscaled*
    ///     (presentation applies its own exaggeration factor).
    ///
    /// A point load on the member introduces a step in the shear, so two coincident
    /// stations (same X, left then right limit) are emitted at its location.
    /// </summary>
    public sealed class MemberStationResult
    {
        public int ElementId { get; }
        public double Length { get; }

        public IReadOnlyList<double> X { get; }
        public IReadOnlyList<double> Axial { get; }
        public IReadOnlyList<double> Shear { get; }
        public IReadOnlyList<double> Moment { get; }
        public IReadOnlyList<double> DeflectionAxial { get; }
        public IReadOnlyList<double> DeflectionTransverse { get; }

        /// <summary>Largest absolute section forces over all stations (exact extrema —
        /// the shear-zero point and any point-load steps are sampled — for design demands).</summary>
        public double MaxAbsAxial { get; }
        public double MaxAbsShear { get; }
        public double MaxAbsMoment { get; }

        public MemberStationResult(
            int elementId,
            double length,
            double[] x,
            double[] axial,
            double[] shear,
            double[] moment,
            double[] deflectionAxial,
            double[] deflectionTransverse)
        {
            ElementId = elementId;
            Length = length;
            X = x ?? throw new ArgumentNullException(nameof(x));
            Axial = axial ?? throw new ArgumentNullException(nameof(axial));
            Shear = shear ?? throw new ArgumentNullException(nameof(shear));
            Moment = moment ?? throw new ArgumentNullException(nameof(moment));
            DeflectionAxial = deflectionAxial ?? throw new ArgumentNullException(nameof(deflectionAxial));
            DeflectionTransverse = deflectionTransverse ?? throw new ArgumentNullException(nameof(deflectionTransverse));

            MaxAbsAxial = MaxAbs(axial);
            MaxAbsShear = MaxAbs(shear);
            MaxAbsMoment = MaxAbs(moment);
        }

        private static double MaxAbs(double[] values)
        {
            double m = 0.0;
            foreach (double v in values)
            {
                double a = Math.Abs(v);
                if (a > m) m = a;
            }
            return m;
        }
    }
}
