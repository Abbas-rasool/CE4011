using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Matrix_Library.MAIN_TYPES;

namespace Matrix_Library.ABSTRACTIONS
{
    public interface IMatrix
    {
        int RowCount { get; }
        int ColumnCount { get; }

        double Get(int row, int col);
        void Set(int row, int col, double value);
        void AddToEntry(int row, int col, double value);

        IMatrix Transpose();
        CustomVector Multiply(CustomVector vector);
    }
}
