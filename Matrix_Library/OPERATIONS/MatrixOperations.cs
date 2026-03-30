using System;
using Matrix_Library.MAIN_TYPES;

namespace Matrix_Library.OPERATIONS
{
    public static class MatrixOperations
    {
        public static SparseMatrix Add(SparseMatrix a, SparseMatrix b)
        {
            ValidateSameSize(a, b);

            SparseMatrix result = Copy(a);

            for (int i = 0; i < b.Count; i++)
            {
                foreach (var entry in b.GetRowEntries(i))
                {
                    result.AddToEntrySymmetric(i, entry.Key, entry.Value);
                }
            }

            return result;
        }

        public static SparseMatrix Subtract(SparseMatrix a, SparseMatrix b)
        {
            ValidateSameSize(a, b);

            SparseMatrix result = Copy(a);

            for (int i = 0; i < b.Count; i++)
            {
                foreach (var entry in b.GetRowEntries(i))
                {
                    result.AddToEntrySymmetric(i, entry.Key, -entry.Value);
                }
            }

            return result;
        }

        public static SparseMatrix Scale(SparseMatrix a, double scalar)
        {
            SparseMatrix result = new SparseMatrix(a.Count);

            for (int i = 0; i < a.Count; i++)
            {
                foreach (var entry in a.GetRowEntries(i))
                {
                    result.Set(i, entry.Key, entry.Value * scalar);
                }
            }

            return result;
        }

        private static void ValidateSameSize(SparseMatrix a, SparseMatrix b)
        {
            if (a.Count != b.Count || a.Count != b.Count)
                throw new ArgumentException("Matrices must have the same dimensions.");
        }

        private static SparseMatrix Copy(SparseMatrix source)
        {
            SparseMatrix copy = new SparseMatrix(source.Count);

            for (int i = 0; i < source.Count; i++)
            {
                foreach (var entry in source.GetRowEntries(i))
                {
                    copy.Set(i, entry.Key, entry.Value);
                }
            }

            return copy;
        }
    }
}