using NRough.Math;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Tests.Math
{
    [TestFixture]
    class MathHelperTest
    {
        [Test]
        public void MatrixDecomposeTest()
        {
            double[][] matrix = new double[3][];            

            matrix[0] = new double[] {  2.0, -1.0,  2.0 };
            matrix[1] = new double[] { -4.0,  6.0,  3.0 };
            matrix[2] = new double[] { -4.0, -2.0,  8.0 };

            int[] perm;
            int toggle;


            Console.WriteLine("Matrix decomposition");
            double[][] decomposedMatrix = MatrixHelper.Decompose(matrix, out perm, out toggle);
            MatrixHelper.Show(decomposedMatrix, 1, true);
            MatrixHelper.ShowVector(perm.Select(Convert.ToDouble).ToArray(), 1);
            Console.WriteLine("Toggle: {0} \n", toggle);
            
            Console.WriteLine("Matrix inversion");
            double[][] inversedMatrix = MatrixHelper.Inverse(matrix);
            MatrixHelper.Show(inversedMatrix, 1, true);



        }
    }
}
