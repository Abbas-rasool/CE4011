using System;
using System.Collections.Generic;
using Matrix_Library.MAIN_TYPES;

namespace FrameAnalysisProgram.ANALYSIS_CORE.Validation
{
    /// <summary>
    /// Authoritative stability check on the reduced stiffness matrix Kff. Performs
    /// an LDL^T factorization (the symmetric factorization the solver itself uses)
    /// and reports the first non-positive pivot. A zero pivot means the matrix is
    /// singular (a mechanism / rigid-body mode); a negative pivot means it is not
    /// positive-definite. The pivot index maps directly back to a node and DOF, so
    /// the message can localize the instability — catching cases that count-based
    /// screens miss (e.g. a free segment beyond an internal hinge).
    /// </summary>
    public class StiffnessSingularityDetector
    {
        private const double RelativeTolerance = 1e-9;

        public IEnumerable<ValidationMessage> Check(SparseMatrix kff, DofMap dofMap)
        {
            int n = kff.Count;
            if (n == 0)
                yield break;

            double maxDiagonal = 0.0;
            for (int i = 0; i < n; i++)
                maxDiagonal = Math.Max(maxDiagonal, Math.Abs(kff.Get(i, i)));

            double tolerance = RelativeTolerance * (maxDiagonal > 0.0 ? maxDiagonal : 1.0);

            // Dense LDL^T factorization, scanning for the first non-positive pivot.
            double[] d = new double[n];
            double[,] l = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                l[i, i] = 1.0;

                for (int j = 0; j < i; j++)
                {
                    double sum = kff.Get(i, j);
                    for (int k = 0; k < j; k++)
                        sum -= l[i, k] * d[k] * l[j, k];
                    l[i, j] = sum / d[j];
                }

                double pivot = kff.Get(i, i);
                for (int k = 0; k < i; k++)
                    pivot -= l[i, k] * l[i, k] * d[k];

                if (pivot <= tolerance)
                {
                    yield return ValidationMessage.Error(DescribeSingularity(i + 1, dofMap, pivot, tolerance));
                    yield break; // first failing pivot is enough to explain the instability
                }

                d[i] = pivot;
            }
        }

        private static string DescribeSingularity(int equationNumber, DofMap dofMap, double pivot, double tolerance)
        {
            string location = "an unidentified degree of freedom";
            if (dofMap.TryGetLocation(equationNumber, out int nodeId, out int localDof))
                location = $"node {nodeId} ({DofName(localDof)})";

            string kind = pivot < -tolerance
                ? "is not positive-definite"
                : "is singular";

            return $"Stiffness matrix {kind} at {location}: the structure is unstable. " +
                   "Likely a mechanism, a free segment beyond an internal hinge, or insufficient / " +
                   "parallel-or-concurrent supports at that location.";
        }

        private static string DofName(int localDof)
        {
            switch (localDof)
            {
                case 0: return "Ux";
                case 1: return "Uy";
                case 2: return "Rz";
                default: return "?";
            }
        }
    }
}
