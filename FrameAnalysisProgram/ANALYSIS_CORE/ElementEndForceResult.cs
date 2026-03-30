using System;
using FrameAnalysisProgram.STRUCTURAL_MODEL;

namespace FrameAnalysisProgram.ANALYSIS_CORE
{
    /// <summary>
    /// Stores the local end force result of a frame element.
    ///
    /// Local force vector order:
    /// [Fx1, Fy1, Mz1, Fx2, Fy2, Mz2]
    /// </summary>
    public class ElementEndForceResult
    {
        public FrameElement2D Element { get; }

        public double[] LocalEndForces { get; }

        public ElementEndForceResult(FrameElement2D element, double[] localEndForces)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
            LocalEndForces = localEndForces ?? throw new ArgumentNullException(nameof(localEndForces));

            if (localEndForces.Length != 6)
                throw new ArgumentException("Local end force vector must have length 6.", nameof(localEndForces));
        }
    }
}