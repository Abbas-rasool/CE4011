using System;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Elements;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Geometry;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Properties;
using Xunit;

namespace FEMTestProject
{
    public class FrameElementTests
    {
        private const int Precision = 4;

        private (Material mat, SectionProperty sec) GetMemberProperties()
        {
            var mat = new Material(1, 200e9);
            var sec = new SectionProperty(1, 0.2, 0.1, 0.08);

            return (mat, sec);
        }

        // --- UNIT TEST 1: LOCAL STIFFNESS MATRIX ---
        [Fact]
        public void GetLocalStiffnessMatrix_RigidHorizontalElement_MatchesAnalyticalValues()
        {
            var (mat, sec) = GetMemberProperties();

            var node1 = new Node(1, 0.0, 0.0);
            var node2 = new Node(2, 4.0, 0.0); // Length = 4.0m

            var element = new FrameElement2D(1, node1, node2, mat, sec, MomentRelease.None);

            double[,] expectedK = new double[6, 6]
            {
                {  1000000000,            0,            0, -1000000000,            0,            0 },
                {           0,   3000000000,   6000000000,           0,  -3000000000,   6000000000 },
                {           0,   6000000000,  16000000000,           0,  -6000000000,   8000000000 },
                { -1000000000,            0,            0,  1000000000,            0,            0 },
                {           0,  -3000000000,  -6000000000,           0,   3000000000,  -6000000000 },
                {           0,   6000000000,   8000000000,           0,  -6000000000,  16000000000 }
            };

            // Act
            double[,] actualK = element.GetLocalStiffnessMatrix();

            // Assert
            AssertMatricesEqual(expectedK, actualK, Precision);
        }

        // --- UNIT TEST 2: ROTATION MATRIX ---
        [Fact]
        public void GetRotationMatrix_45DegreeElement_CalculatesCorrectTrigTransform()
        {
            // Arrange
            var (mat, sec) = GetMemberProperties();
            var node1 = new Node(1, 0.0, 0.0);
            var node2 = new Node(2, 4.0, 4.0);

            var element = new FrameElement2D(1, node1, node2, mat, sec);

            double c = 1.0 / Math.Sqrt(2);
            double s = 1.0 / Math.Sqrt(2);

            double[,] expectedR = new double[6, 6]
            {
                {  c,  s,  0,  0,  0,  0 },
                { -s,  c,  0,  0,  0,  0 },
                {  0,  0,  1,  0,  0,  0 },
                {  0,  0,  0,  c,  s,  0 },
                {  0,  0,  0, -s,  c,  0 },
                {  0,  0,  0,  0,  0,  1 }
            };

            // Act
            double[,] actualR = element.GetRotationMatrix();

            // Assert
            AssertMatricesEqual(expectedR, actualR, Precision);
        }

        // --- UNIT TEST 3: STATIC CONDENSATION (MOMENT RELEASE) ---
        [Fact]
        public void GetLocalStiffnessMatrix_StartHingeRelease_ZeroesRotationalDofAndModifiesBending()
        {
            // Arrange
            var (mat, sec) = GetMemberProperties();
            var node1 = new Node(1, 0.0, 0.0);
            var node2 = new Node(2, 4.0, 0.0);

            var element = new FrameElement2D(1, node1, node2, mat, sec, MomentRelease.Start);
            double[,] expectedCondensedK = new double[6, 6]
            {
                {  1000000000,            0,   0, -1000000000,            0,            0 },
                {           0,    750000000,   0,           0,   -750000000,   3000000000 },
                {           0,            0,   0,           0,            0,            0 },
                { -1000000000,            0,   0,  1000000000,            0,            0 },
                {           0,   -750000000,   0,           0,    750000000,  -3000000000 },
                {           0,   3000000000,   0,           0,  -3000000000,  12000000000 }
            };

            // Act
            double[,] actualCondensedK = element.GetLocalStiffnessMatrix();

            // Assert
            AssertMatricesEqual(expectedCondensedK, actualCondensedK, Precision);
        }

        private void AssertMatricesEqual(double[,] expected, double[,] actual, int precision)
        {
            Assert.Equal(expected.GetLength(0), actual.GetLength(0));
            Assert.Equal(expected.GetLength(1), actual.GetLength(1));

            for (int i = 0; i < expected.GetLength(0); i++)
            {
                for (int j = 0; j < expected.GetLength(1); j++)
                {
                    Assert.Equal(expected[i, j], actual[i, j], precision);
                }
            }
        }
    }
}