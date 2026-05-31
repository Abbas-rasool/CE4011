using System;
using FrameAnalysisProgram.STRUCTURAL_MODEL;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Elements;

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
        public StructuralElement2D Element { get; }

        public double[] LocalEndForces { get; }

        public ElementEndForceResult(StructuralElement2D element, double[] localEndForces)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
            LocalEndForces = localEndForces ?? throw new ArgumentNullException(nameof(localEndForces));

            // 6 for a frame element ([Fx1,Fy1,Mz1,Fx2,Fy2,Mz2]),
            // 4 for a truss element ([Fx1,Fy1,Fx2,Fy2]).
            if (localEndForces.Length != 6 && localEndForces.Length != 4)
                throw new ArgumentException("Local end force vector must have length 4 (truss) or 6 (frame).", nameof(localEndForces));
        }
    }
}