using System;
using System.Collections.Generic;
using System.Text;

namespace Matrix_Library.MAIN_TYPES
{
    public class CustomVector
    {
        private double[] _values;

        public int Length => _values.Length;

        public CustomVector(int size)
        {
            _values = new double[size];
        }

        public double Get(int index) => _values[index];

        public void Set(int index, double value)
        {
            _values[index] = value;
        }

        public void AddToEntry(int i, double value)
        {
            _values[i] += value;
        }

        public double this[int index]
        {
            get => _values[index];
            set => _values[index] = value;
        }
    }
}
