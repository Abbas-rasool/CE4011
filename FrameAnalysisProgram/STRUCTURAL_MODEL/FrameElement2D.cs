using System;

namespace FrameAnalysisProgram.STRUCTURAL_MODEL
{
    /// <summary>
    /// Represents a 2D frame element connecting two nodes.
    /// Each node has three degrees of freedom: Ux, Uy, Rz.
    ///
    /// Local element DOF order:
    /// [u1, v1, r1, u2, v2, r2]
    ///
    /// Assumptions:
    /// - Linear elastic behavior
    /// - Small displacements
    /// - Prismatic member
    /// - Euler-Bernoulli beam theory
    /// - 2D frame behavior in the global XY plane
    /// </summary>
    public class FrameElement2D
    {
        /// <summary>
        /// Unique element identifier.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Start node of the element.
        /// </summary>
        public Node StartNode { get; }

        /// <summary>
        /// End node of the element.
        /// </summary>
        public Node EndNode { get; }

        /// <summary>
        /// Material assigned to the element.
        /// </summary>
        public Material Material { get; }

        /// <summary>
        /// Section properties assigned to the element.
        /// </summary>
        public SectionProperty Section { get; }

        /// <summary>
        /// Element length in global coordinates.
        /// Units: length
        /// </summary>
        public double Length
        {
            get
            {
                double dx = EndNode.X - StartNode.X;
                double dy = EndNode.Y - StartNode.Y;
                return Math.Sqrt(dx * dx + dy * dy);
            }
        }

        /// <summary>
        /// Cosine of the angle between the element axis and global X axis.
        /// </summary>
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

        /// <summary>
        /// Sine of the angle between the element axis and global X axis.
        /// </summary>
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

        public FrameElement2D(
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

        /// <summary>
        /// Forms the 6x6 rotation matrix that transforms
        /// global element displacements to local displacements.
        ///
        /// Relation:
        /// {d_local} = [R] {d_global}
        /// </summary>
        public double[,] GetRotationMatrix()
        {
            double c = CosX;
            double s = SinX;

            double[,] rotation = new double[6, 6];

            rotation[0, 0] = c;
            rotation[0, 1] = s;
            rotation[1, 0] = -s;
            rotation[1, 1] = c;
            rotation[2, 2] = 1.0;

            rotation[3, 3] = c;
            rotation[3, 4] = s;
            rotation[4, 3] = -s;
            rotation[4, 4] = c;
            rotation[5, 5] = 1.0;

            return rotation;
        }

        /// <summary>
        /// Forms the 6x6 element stiffness matrix in local coordinates.
        ///
        /// DOF order:
        /// [u1, v1, r1, u2, v2, r2]
        ///
        /// Units:
        /// - axial terms: force / length
        /// - bending translation terms: force / length
        /// - coupling terms: force
        /// - rotational terms: force * length
        /// </summary>
        public double[,] GetLocalStiffnessMatrix()
        {
            double L = Length;
            if (L <= 0.0)
                throw new InvalidOperationException($"Element {Id} has zero length.");

            double A = Section.Area;
            double I = Section.MomentOfInertia;
            double E = Material.ElasticModulus;

            double eaOverL = E * A / L;
            double ei = E * I;
            double twelveEiOverL3 = 12.0 * ei / (L * L * L);
            double sixEiOverL2 = 6.0 * ei / (L * L);
            double fourEiOverL = 4.0 * ei / L;
            double twoEiOverL = 2.0 * ei / L;

            double[,] k = new double[6, 6];

            k[0, 0] = eaOverL;
            k[0, 3] = -eaOverL;

            k[1, 1] = twelveEiOverL3;
            k[1, 2] = sixEiOverL2;
            k[1, 4] = -twelveEiOverL3;
            k[1, 5] = sixEiOverL2;

            k[2, 1] = sixEiOverL2;
            k[2, 2] = fourEiOverL;
            k[2, 4] = -sixEiOverL2;
            k[2, 5] = twoEiOverL;

            k[3, 0] = -eaOverL;
            k[3, 3] = eaOverL;

            k[4, 1] = -twelveEiOverL3;
            k[4, 2] = -sixEiOverL2;
            k[4, 4] = twelveEiOverL3;
            k[4, 5] = -sixEiOverL2;

            k[5, 1] = sixEiOverL2;
            k[5, 2] = twoEiOverL;
            k[5, 4] = -sixEiOverL2;
            k[5, 5] = fourEiOverL;

            return k;
        }

        /// <summary>
        /// Forms the 6x6 element stiffness matrix in global coordinates.
        ///
        /// Relation:
        /// [k_global] = [R]^T [k_local] [R]
        /// </summary>
        public double[,] GetGlobalStiffnessMatrix()
        {
            double[,] rotation = GetRotationMatrix();
            double[,] rotationTranspose = Transpose(rotation);
            double[,] localStiffness = GetLocalStiffnessMatrix();

            double[,] temp = Multiply(rotationTranspose, localStiffness);
            double[,] globalStiffness = Multiply(temp, rotation);

            return globalStiffness;
        }

        /// <summary>
        /// Extracts the 6x1 element displacement vector in global coordinates
        /// from the full nodal displacement matrix.
        ///
        /// nodalDisplacements columns:
        /// [Ux, Uy, Rz]
        /// row index = Node ID - 1
        /// </summary>
        public double[] GetGlobalDisplacementVector(double[,] nodalDisplacements)
        {
            if (nodalDisplacements == null)
                throw new ArgumentNullException(nameof(nodalDisplacements));

            if (nodalDisplacements.GetLength(1) != 3)
                throw new ArgumentException("Nodal displacement matrix must have 3 columns: [Ux, Uy, Rz].");

            int startIndex = StartNode.Id - 1;
            int endIndex = EndNode.Id - 1;

            return new double[]
            {
                nodalDisplacements[startIndex, 0],
                nodalDisplacements[startIndex, 1],
                nodalDisplacements[startIndex, 2],
                nodalDisplacements[endIndex, 0],
                nodalDisplacements[endIndex, 1],
                nodalDisplacements[endIndex, 2]
            };
        }

        /// <summary>
        /// Transforms the 6x1 element displacement vector
        /// from global coordinates to local coordinates.
        ///
        /// Relation:
        /// {d_local} = [R] {d_global}
        /// </summary>
        public double[] GetLocalDisplacementVector(double[] globalDisplacements)
        {
            if (globalDisplacements == null)
                throw new ArgumentNullException(nameof(globalDisplacements));

            if (globalDisplacements.Length != 6)
                throw new ArgumentException("Element global displacement vector must have length 6.");

            double[,] rotation = GetRotationMatrix();
            return Multiply(rotation, globalDisplacements);
        }

        /// <summary>
        /// Calculates the 6x1 local element end force vector.
        ///
        /// Relation:
        /// {f_local} = [k_local] {d_local}
        /// </summary>
        public double[] GetLocalEndForceVector(double[] globalDisplacements)
        {
            if (globalDisplacements == null)
                throw new ArgumentNullException(nameof(globalDisplacements));

            double[] localDisplacements = GetLocalDisplacementVector(globalDisplacements);
            double[,] localStiffness = GetLocalStiffnessMatrix();

            return Multiply(localStiffness, localDisplacements);
        }

        private static double[,] Transpose(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            double[,] transpose = new double[cols, rows];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    transpose[j, i] = matrix[i, j];
                }
            }

            return transpose;
        }

        private static double[,] Multiply(double[,] left, double[,] right)
        {
            int leftRows = left.GetLength(0);
            int leftCols = left.GetLength(1);
            int rightRows = right.GetLength(0);
            int rightCols = right.GetLength(1);

            if (leftCols != rightRows)
                throw new InvalidOperationException("Matrix dimensions are not compatible for multiplication.");

            double[,] result = new double[leftRows, rightCols];

            for (int i = 0; i < leftRows; i++)
            {
                for (int j = 0; j < rightCols; j++)
                {
                    double sum = 0.0;

                    for (int k = 0; k < leftCols; k++)
                    {
                        sum += left[i, k] * right[k, j];
                    }

                    result[i, j] = sum;
                }
            }

            return result;
        }

        private static double[] Multiply(double[,] matrix, double[] vector)
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
                {
                    sum += matrix[i, j] * vector[j];
                }

                result[i] = sum;
            }

            return result;
        }
    }
}