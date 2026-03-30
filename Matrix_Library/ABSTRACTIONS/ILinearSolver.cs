using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Matrix_Library.MAIN_TYPES;

namespace Matrix_Library.ABSTRACTIONS
{
    public interface ILinearSolver
    {
        void Factorize(IMatrix a);
        CustomVector Solve(CustomVector b);
    }
}
