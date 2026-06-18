using System;
using Matrix_Library.ABSTRACTIONS;
using Matrix_Library.MAIN_TYPES;

namespace Matrix_Library.SOLVERS
{
    public class SparseLDLtSolver : ILinearSolver
    {
        // Absolute threshold used only to decide whether a tiny off-diagonal
        // contribution is worth storing/multiplying (a sparsity optimization).
        private const double Tolerance = 1e-12;

        // Relative threshold for judging pivots. A stiffness matrix can have
        // diagonal entries on the order of EA/L (1e8+), so an absolute pivot
        // threshold is meaningless; we scale by the largest diagonal instead.
        // Matches StiffnessSingularityDetector so the solver and the diagnostic
        // agree on what "singular" means.
        private const double RelativeTolerance = 1e-9;

        private SparseMatrix? _L;
        private double[]? _D;

        private int _size;
        private bool _isFactorized;

        public void Factorize(IMatrix a)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));

            if (a.RowCount != a.ColumnCount)
                throw new ArgumentException("Matrix must be square.");

            _size = a.RowCount;
            _L = new SparseMatrix(_size);
            _D = new double[_size];
            _isFactorized = false;

            // Scale-aware pivot tolerance, judged relative to the largest diagonal.
            double maxDiagonal = 0.0;
            for (int i = 0; i < _size; i++)
                maxDiagonal = Math.Max(maxDiagonal, Math.Abs(a.Get(i, i)));

            double pivotTolerance = RelativeTolerance * (maxDiagonal > 0.0 ? maxDiagonal : 1.0);

            for (int i = 0; i < _size; i++)
            {
                // Unit diagonal (implicit, but we can store it for clarity)
                _L.Set(i, i, 1.0);

                for (int j = 0; j < i; j++)
                {
                    double aij = GetSymmetric(a, i, j);

                    //if (Math.Abs(aij) < Tolerance)
                    //    continue;

                    double sum = aij;

                    for (int k = 0; k < j; k++)
                    {
                        double lik = _L.Get(i, k);
                        double ljk = _L.Get(j, k);

                        if (Math.Abs(lik) < Tolerance || Math.Abs(ljk) < Tolerance)
                            continue;

                        sum -= lik * _D[k] * ljk;
                    }

                    if (Math.Abs(_D[j]) < Tolerance)
                        throw new InvalidOperationException(
                            $"Zero pivot at DOF {j}. Structure may be unstable.");

                    double lij = sum / _D[j];

                    if (Math.Abs(lij) >= Tolerance)
                        _L.Set(i, j, lij);
                }

                // --- Compute D(i) ---
                double di = a.Get(i, i);

                for (int k = 0; k < i; k++)
                {
                    double lik = _L.Get(i, k);
                    if (Math.Abs(lik) < Tolerance)
                        continue;

                    di -= lik * lik * _D[k];
                }

                // A zero pivot means the matrix is singular (a mechanism /
                // rigid-body mode); a negative pivot means it is not
                // positive-definite. Both indicate an unstable structure, so
                // reject any non-positive pivot (signed test, not Math.Abs).
                if (di < pivotTolerance)
                    throw new InvalidOperationException(
                        $"Non-positive pivot at DOF {i}. Structure may be unstable or improperly constrained.");

                _D[i] = di;
            }

            _isFactorized = true;
        }

        public CustomVector Solve(CustomVector b)
        {
            if (!_isFactorized)
                throw new InvalidOperationException("Call Factorize() first.");

            if (b == null)
                throw new ArgumentNullException(nameof(b));

            if (b.Length != _size)
                throw new ArgumentException("Vector size mismatch.");

            CustomVector y = ForwardSubstitution(b);
            CustomVector z = DiagonalSolve(y);
            CustomVector x = BackwardSubstitution(z);

            return x;
        }

        private CustomVector ForwardSubstitution(CustomVector b)
        {
            CustomVector y = new CustomVector(_size);

            for (int i = 0; i < _size; i++)
            {
                double sum = b[i];

                foreach (var entry in _L.GetRowEntries(i))
                {
                    int j = entry.Key;
                    if (j >= i) continue;

                    sum -= entry.Value * y[j];
                }

                y[i] = sum;
            }

            return y;
        }

        private CustomVector DiagonalSolve(CustomVector y)
        {
            CustomVector z = new CustomVector(_size);

            for (int i = 0; i < _size; i++)
            {
                if (Math.Abs(_D[i]) < Tolerance)
                    throw new InvalidOperationException($"Zero diagonal at D[{i}].");

                z[i] = y[i] / _D[i];
            }

            return z;
        }

        // L^T x = z
        private CustomVector BackwardSubstitution(CustomVector z)
        {
            CustomVector x = new CustomVector(_size);

            for (int i = _size - 1; i >= 0; i--)
            {
                double sum = z[i];

                for (int j = i + 1; j < _size; j++)
                {
                    double lji = _L.Get(j, i);
                    if (Math.Abs(lji) < Tolerance)
                        continue;

                    sum -= lji * x[j];
                }

                x[i] = sum;
            }

            return x;
        }

        private double GetSymmetric(IMatrix a, int i, int j)
        {
            if (i >= j)
                return a.Get(i, j);
            else
                return a.Get(j, i);
        }
    }
}