using FrameAnalysisProgram.ANALYSIS_CORE;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Geometry;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Properties;

namespace FrameAnalysisProgram.STRUCTURAL_MODEL.Elements
{
/// <summary>
/// 2D truss element with 2 DOFs per node: Ux, Uy (no rotation).
/// Local DOF order: [u1, v1, u2, v2]
///
/// Assumptions:
/// - Axial force only, no bending
/// - Linear elastic, small displacements
/// - Prismatic member, pin-connected at both ends
/// </summary>
public class TrussElement2D : StructuralElement2D
{
    public TrussElement2D(
        int id,
        Node startNode,
        Node endNode,
        Material material,
        SectionProperty section)
        : base(id, startNode, endNode, material, section)
    {
    }

    // -------------------------------------------------------------------------
    // Truss-specific: 4x4 rotation matrix (no rotational DOF rows/cols)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Forms the 4x4 rotation matrix.
    /// Relation: {d_local} = [R] {d_global}
    /// </summary>
    public override double[,] GetRotationMatrix()
    {
        double c = CosX;
        double s = SinX;

        double[,] R = new double[4, 4];

        R[0, 0] = c; R[0, 1] = s;
        R[1, 0] = -s; R[1, 1] = c;

        R[2, 2] = c; R[2, 3] = s;
        R[3, 2] = -s; R[3, 3] = c;

        return R;
    }

    // -------------------------------------------------------------------------
    // Abstract overrides
    // -------------------------------------------------------------------------

    /// <summary>
    /// Forms the 4x4 element stiffness matrix in local coordinates.
    /// Truss carries axial force only — transverse rows and columns are zero.
    ///
    /// DOF order: [u1, v1, u2, v2]
    /// </summary>
    public override double[,] GetLocalStiffnessMatrix()
    {
        double L = Length;
        if (L <= 0.0)
            throw new InvalidOperationException($"Element {Id} has zero length.");

        double eaL = Material.ElasticModulus * Section.Area / L;

        double[,] k = new double[4, 4];

        // Axial terms only — v1 and v2 rows/cols remain zero (pin ends, no transverse stiffness)
        k[0, 0] = eaL; k[0, 2] = -eaL;
        k[2, 0] = -eaL; k[2, 2] = eaL;

        return k;
    }

    public override double[,] GetGlobalStiffnessMatrix()
    {
        double[,] R = GetRotationMatrix();
        double[,] RT = Transpose(R);
        double[,] kL = GetLocalStiffnessMatrix();

        return MultiplyMatrixMatrix(MultiplyMatrixMatrix(RT, kL), R);
    }

    public override (int NodeId, DofType Dof)[] GetDofAddresses()
    {
        return new (int, DofType)[]
        {
            (StartNode.Id, DofType.Ux),
            (StartNode.Id, DofType.Uy),
            (EndNode.Id,   DofType.Ux),
            (EndNode.Id,   DofType.Uy)
        };
    }

    public override double[] GetLocalDisplacementVector(double[] globalDisplacements)
    {
        if (globalDisplacements == null)
            throw new ArgumentNullException(nameof(globalDisplacements));

        if (globalDisplacements.Length != 4)
            throw new ArgumentException("Truss element global displacement vector must have length 4.");

        return MultiplyMatrixVector(GetRotationMatrix(), globalDisplacements);
    }
}
}