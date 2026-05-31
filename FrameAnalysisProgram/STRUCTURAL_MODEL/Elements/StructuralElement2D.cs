using System;
using FrameAnalysisProgram.ANALYSIS_CORE;

namespace FrameAnalysisProgram.STRUCTURAL_MODEL
{
public abstract class StructuralElement2D
{
    public int Id { get; }
    public Node StartNode { get; }
    public Node EndNode { get; }
    public Material Material { get; }
    public SectionProperty Section { get; }

    public double Length
    {
        get
        {
            double dx = EndNode.X - StartNode.X;
            double dy = EndNode.Y - StartNode.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }

    public double CosX
    {
        get
        {
            double length = Length;
            if (length <= 0.0)
                throw new InvalidOperationException($"Element {Id} has zero length.");
            return (EndNode.X - StartNode.X) / length;
        }
    }

    public double SinX
    {
        get
        {
            double length = Length;
            if (length <= 0.0)
                throw new InvalidOperationException($"Element {Id} has zero length.");
            return (EndNode.Y - StartNode.Y) / length;
        }
    }

    protected StructuralElement2D(
        int id,
        Node startNode,
        Node endNode,
        Material material,
        SectionProperty section)
    {
        Id = id;
        StartNode = startNode ?? throw new ArgumentNullException(nameof(startNode));
        EndNode = endNode ?? throw new ArgumentNullException(nameof(endNode));
        Material = material ?? throw new ArgumentNullException(nameof(material));
        Section = section ?? throw new ArgumentNullException(nameof(section));
    }

    // -------------------------------------------------------------------------
    // Abstract interface — each element type must implement these
    // -------------------------------------------------------------------------

    /// <summary>
    /// Element stiffness matrix in local coordinates.
    /// Size depends on element type (e.g. 6x6 for frame, 4x4 for truss).
    /// </summary>
    public abstract double[,] GetLocalStiffnessMatrix();

    /// <summary>
    /// Element stiffness matrix in global coordinates.
    /// Relation: [k_global] = [R]^T [k_local] [R]
    /// </summary>
    public abstract double[,] GetGlobalStiffnessMatrix();

    /// <summary>
    /// Global equation numbers for each DOF of this element,
    /// ordered to match the local stiffness matrix rows/columns.
    /// </summary>
    public abstract int[] GetGlobalDofIndices(DofMap dofMap);

    /// <summary>
    /// Transforms a global element displacement vector to local coordinates.
    /// Relation: {d_local} = [R] {d_global}
    /// Implemented per element type since rotation matrix size differs.
    /// </summary>
    public abstract double[] GetLocalDisplacementVector(double[] globalDisplacements);

    // -------------------------------------------------------------------------
    // Concrete shared methods — same logic for all element types
    // -------------------------------------------------------------------------

    /// <summary>
    /// Extracts this element's DOF values from the full solution vector
    /// using the DOF map to locate the correct equation numbers.
    /// </summary>
    public double[] GetGlobalDisplacementVector(double[] solutionVector, DofMap dofMap)
    {
        if (solutionVector == null)
            throw new ArgumentNullException(nameof(solutionVector));

        int[] indices = GetGlobalDofIndices(dofMap);
        double[] displacements = new double[indices.Length];

        for (int i = 0; i < indices.Length; i++)
        {
            int equation = indices[i];                       // 1-based; 0 = restrained
            displacements[i] = equation == 0 ? 0.0 : solutionVector[equation - 1];
        }

        return displacements;
    }

    /// <summary>
    /// Calculates local element end forces from global displacements.
    /// Relation: {f_local} = [k_local] {d_local}
    /// </summary>
    public double[] GetLocalEndForceVector(double[] globalDisplacements)
    {
        if (globalDisplacements == null)
            throw new ArgumentNullException(nameof(globalDisplacements));

        double[] localDisplacements = GetLocalDisplacementVector(globalDisplacements);
        double[,] localStiffness = GetLocalStiffnessMatrix();

        return MultiplyMatrixVector(localStiffness, localDisplacements);
    }

    // -------------------------------------------------------------------------
    // Protected matrix helpers — available to all subclasses
    // -------------------------------------------------------------------------

    protected static double[,] Transpose(double[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        double[,] result = new double[cols, rows];

        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                result[j, i] = matrix[i, j];

        return result;
    }

    protected static double[,] MultiplyMatrixMatrix(double[,] left, double[,] right)
    {
        int leftRows = left.GetLength(0);
        int leftCols = left.GetLength(1);
        int rightCols = right.GetLength(1);

        if (leftCols != right.GetLength(0))
            throw new InvalidOperationException("Matrix dimensions are not compatible for multiplication.");

        double[,] result = new double[leftRows, rightCols];

        for (int i = 0; i < leftRows; i++)
            for (int j = 0; j < rightCols; j++)
            {
                double sum = 0.0;
                for (int k = 0; k < leftCols; k++)
                    sum += left[i, k] * right[k, j];
                result[i, j] = sum;
            }

        return result;
    }

    protected static double[] MultiplyMatrixVector(double[,] matrix, double[] vector)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        if (cols != vector.Length)
            throw new InvalidOperationException("Matrix and vector dimensions are not compatible for multiplication.");

        double[] result = new double[rows];

        for (int i = 0; i < rows; i++)
        {
            double sum = 0.0;
            for (int j = 0; j < cols; j++)
                sum += matrix[i, j] * vector[j];
            result[i] = sum;
        }

        return result;
    }
}
}