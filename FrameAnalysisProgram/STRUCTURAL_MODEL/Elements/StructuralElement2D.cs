using FrameAnalysisProgram.ANALYSIS_CORE;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Geometry;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Properties;

namespace FrameAnalysisProgram.STRUCTURAL_MODEL.Elements
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
    /// Rotation matrix transforming global to local coordinates.
    /// Relation: {d_local} = [R] {d_global}. Size depends on element type
    /// (6x6 for frame, 4x4 for truss).
    /// </summary>
    public abstract double[,] GetRotationMatrix();

    /// <summary>
    /// The (node, DOF-type) address of each local DOF, in the same order as
    /// the rows/columns of the local stiffness matrix. This is the single
    /// element-specific definition of connectivity; the size-agnostic helpers
    /// below are derived from it.
    /// </summary>
    public abstract (int NodeId, DofType Dof)[] GetDofAddresses();

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
    /// Global equation numbers for each DOF of this element, ordered to match
    /// the local stiffness matrix. Equation 0 means a restrained / inactive DOF.
    /// </summary>
    public int[] GetGlobalDofIndices(DofMap dofMap)
    {
        if (dofMap == null)
            throw new ArgumentNullException(nameof(dofMap));

        (int NodeId, DofType Dof)[] addresses = GetDofAddresses();
        int[] indices = new int[addresses.Length];

        for (int i = 0; i < addresses.Length; i++)
            indices[i] = dofMap.GetEquation(addresses[i].NodeId, addresses[i].Dof);

        return indices;
    }

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
    /// Extracts this element's global DOF values from the nodal displacement
    /// matrix (rows = Node ID - 1; columns = [Ux, Uy, Rz]).
    /// </summary>
    public double[] GetGlobalDisplacementVector(double[,] nodalDisplacements)
    {
        if (nodalDisplacements == null)
            throw new ArgumentNullException(nameof(nodalDisplacements));

        (int NodeId, DofType Dof)[] addresses = GetDofAddresses();
        double[] displacements = new double[addresses.Length];

        for (int i = 0; i < addresses.Length; i++)
            displacements[i] = nodalDisplacements[addresses[i].NodeId - 1, (int)addresses[i].Dof];

        return displacements;
    }

    /// <summary>
    /// Transforms a local end-force vector to global coordinates.
    /// Relation: {f_global} = [R]^T {f_local}
    /// </summary>
    public double[] GetGlobalEndForces(double[] localEndForces)
    {
        if (localEndForces == null)
            throw new ArgumentNullException(nameof(localEndForces));

        return MultiplyMatrixVector(Transpose(GetRotationMatrix()), localEndForces);
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