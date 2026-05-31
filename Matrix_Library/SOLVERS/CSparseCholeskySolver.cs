using System;
using CSparse;
using CSparse.Double.Factorization;
using CSparse.Storage;
using Matrix_Library.ABSTRACTIONS;
using Matrix_Library.MAIN_TYPES;

namespace Matrix_Library.SOLVERS
{
    /// <summary>
    /// Sparse symmetric positive-definite linear solver backed by CSparse.NET
    /// (a C# port of Tim Davis's CSparse / SuiteSparse).
    ///
    /// Performs a sparse Cholesky factorization with an approximate-minimum-degree
    /// (AMD) fill-reducing ordering. This is the mature, library-backed equivalent
    /// of <see cref="SparseLDLtSolver"/> and is a drop-in replacement behind
    /// <see cref="ILinearSolver"/>.
    /// </summary>
    public class CSparseCholeskySolver : ILinearSolver
    {
        private SparseCholesky? _cholesky;
        private int _size;

        public void Factorize(IMatrix a)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));

            if (a.RowCount != a.ColumnCount)
                throw new ArgumentException("Matrix must be square.");

            _size = a.RowCount;

            // The caller stores only the lower triangle of a symmetric matrix.
            // CSparse expects the full matrix, so mirror the off-diagonal entries.
            var coordinates = new CoordinateStorage<double>(_size, _size, EstimateNonZeros(a));

            if (a is SparseMatrix sparse)
            {
                for (int i = 0; i < _size; i++)
                {
                    foreach (var entry in sparse.GetRowEntries(i))
                    {
                        int j = entry.Key;          // j <= i (lower triangle)
                        double value = entry.Value;

                        coordinates.At(i, j, value);
                        if (i != j)
                            coordinates.At(j, i, value);
                    }
                }
            }
            else
            {
                // Generic fallback for any IMatrix implementation.
                for (int i = 0; i < _size; i++)
                {
                    for (int j = 0; j <= i; j++)
                    {
                        double value = a.Get(i, j);
                        if (value == 0.0)
                            continue;

                        coordinates.At(i, j, value);
                        if (i != j)
                            coordinates.At(j, i, value);
                    }
                }
            }

            var matrix = CompressedColumnStorage<double>.OfIndexed(coordinates, true);

            try
            {
                _cholesky = SparseCholesky.Create(matrix, ColumnOrdering.MinimumDegreeAtPlusA);
            }
            catch (Exception ex)
            {
                // CSparse throws when a non-positive pivot is encountered, i.e. the
                // global stiffness matrix is not positive definite.
                throw new InvalidOperationException(
                    "Cholesky factorization failed. The structure may be unstable or improperly constrained.",
                    ex);
            }
        }

        public CustomVector Solve(CustomVector b)
        {
            if (_cholesky == null)
                throw new InvalidOperationException("Call Factorize() first.");

            if (b == null)
                throw new ArgumentNullException(nameof(b));

            if (b.Length != _size)
                throw new ArgumentException("Vector size mismatch.");

            double[] rhs = new double[_size];
            for (int i = 0; i < _size; i++)
                rhs[i] = b[i];

            double[] solution = new double[_size];
            _cholesky.Solve(rhs, solution);

            CustomVector result = new CustomVector(_size);
            for (int i = 0; i < _size; i++)
                result[i] = solution[i];

            return result;
        }

        private static int EstimateNonZeros(IMatrix a)
        {
            if (a is SparseMatrix sparse)
            {
                int lowerTriangleCount = 0;
                for (int i = 0; i < sparse.Count; i++)
                    lowerTriangleCount += sparse.GetRowEntries(i).Count;

                // Roughly two entries per off-diagonal once mirrored.
                return Math.Max(1, 2 * lowerTriangleCount);
            }

            return Math.Max(1, a.RowCount);
        }
    }
}
