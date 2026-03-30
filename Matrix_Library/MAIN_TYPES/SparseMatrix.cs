using System;
using System.Collections.Generic;
using System.Text;
using Matrix_Library.ABSTRACTIONS;

namespace Matrix_Library.MAIN_TYPES
{
    // Stores only lower triangular part (i >= j) of a symmetric matrix.
    public class SparseMatrix : IMatrix
    {
        private readonly List<Dictionary<int, double>> _rows;

        public int Count { get; }

        public int RowCount => Count;
        public int ColumnCount => Count;

        public SparseMatrix(int size)
        {
            Count = size;

            _rows = new List<Dictionary<int, double>>(size);

            for (int i = 0; i < size; i++)
                _rows.Add(new Dictionary<int, double>());
        }

        public double Get(int i, int j)
        {
            if (i >= j)
            {
                if (_rows[i].TryGetValue(j, out double value))
                    return value;
            }
            else
            {
                if (_rows[j].TryGetValue(i, out double value))
                    return value;
            }

            return 0.0;
        }

        public void Set(int i, int j, double value)
        {
            if (i < j)
                throw new InvalidOperationException("Only lower triangle storage is allowed.");

            if (Math.Abs(value) < 1e-14)
                _rows[i].Remove(j);
            else
                _rows[i][j] = value;
        }

        public void SetSymmetric(int i, int j, double value)
        {
            if (i >= j)
                Set(i, j, value);
            else
                Set(j, i, value);

        }
        public IReadOnlyDictionary<int, double> GetRowEntries(int row)
        {
            return _rows[row];
        }

        public void AddToEntrySymmetric(int i, int j, double value)
        {
            if (i >= j)
                AddToEntry(i, j, value);
            else
                AddToEntry(j, i, value);
        }

        public void AddToEntry(int row, int col, double value)
        {
            double current = Get(row, col);
            double updated = current + value;

            if (Math.Abs(updated) < 1e-14)
                _rows[row].Remove(col);
            else
                _rows[row][col] = updated;
        }

        public IMatrix Transpose()
        {
            SparseMatrix transposed = new SparseMatrix(Count);

            for (int i = 0; i < Count; i++)
            {
                foreach (var entry in _rows[i])
                {
                    transposed.Set(i, entry.Key, entry.Value);
                }
            }

            return transposed;
        }

        public CustomVector Multiply(CustomVector vector)
        {
            if (vector.Length != Count)
                throw new ArgumentException("Vector size must match matrix column count.");

            CustomVector result = new CustomVector(Count);

            for (int i = 0; i < Count; i++)
            {
                foreach (var entry in _rows[i])
                {
                    int j = entry.Key;
                    double value = entry.Value;

                    result[i] += value * vector[j];

                    if (i != j)
                    {
                        result[j] += value * vector[i];
                    }
                }
            }

            return result;
        }
    }
}
