using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Math
{
    public static class MatrixHelper
    {       
        public static double[][] Inverse(double[][] matrix)
        {
            int n = matrix.Length;
            double[][] result = Duplicate(matrix);

            int[] perm;
            int toggle;
            double[][] lum = Decompose(matrix, out perm, out toggle);
            if (lum == null)
                throw new Exception("Unable to compute inverse");

            double[] b = new double[n];
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                {
                    if (i == perm[j])
                        b[j] = 1.0;
                    else
                        b[j] = 0.0;
                }

                double[] x = HelperSolve(lum, b); // use decomposition

                for (int j = 0; j < n; ++j)
                    result[j][i] = x[j];
            }
            return result;
        }

        // -------------------------------------------------------------

        public static double[] HelperSolve(double[][] luMatrix, double[] b)
        {
            // before calling this helper, permute b using the perm array
            // from MatrixDecompose that generated luMatrix

            int n = luMatrix.Length;
            double[] x = new double[n];
            b.CopyTo(x, 0);

            for (int i = 1; i < n; ++i)
            {
                double sum = x[i];
                for (int j = 0; j < i; ++j)
                    sum -= luMatrix[i][j] * x[j];
                x[i] = sum;
            }

            x[n - 1] /= luMatrix[n - 1][n - 1];
            for (int i = n - 2; i >= 0; --i)
            {
                double sum = x[i];
                for (int j = i + 1; j < n; ++j)
                    sum -= luMatrix[i][j] * x[j];
                x[i] = sum / luMatrix[i][i];
            }

            return x;
        }

        // -------------------------------------------------------------

        public static double[][] Duplicate(double[][] matrix)
        {
            // allocates/creates a duplicate of a matrix
            double[][] result = MatrixCreate(matrix.Length, matrix[0].Length);
            for (int i = 0; i < matrix.Length; ++i) // copy the values
                for (int j = 0; j < matrix[i].Length; ++j)
                    result[i][j] = matrix[i][j];
            return result;
        }

        // -------------------------------------------------------------

        public static double[][] Decompose(double[][] matrix, out int[] perm,
            out int toggle)
        {
            // Doolittle LUP decomposition with partial pivoting.
            // returns: result is L (with 1s on diagonal) and U;
            // perm holds row permutations; toggle is +1 or -1 (even or odd)
            int rows = matrix.Length;
            int cols = matrix[0].Length;
            if (rows != cols)
                throw new Exception("Non-square mattrix");

            int n = rows; // convenience

            double[][] result = Duplicate(matrix); // 

            perm = new int[n]; // set up row permutation result
            for (int i = 0; i < n; ++i) { perm[i] = i; }

            toggle = 1; // toggle tracks row swaps

            for (int j = 0; j < n - 1; ++j) // each column
            {
                double colMax = System.Math.Abs(result[j][j]);
                int pRow = j;
                
                for (int i = j + 1; i < n; ++i) // reader Matt V needed this:
                {
                    if (System.Math.Abs(result[i][j]) > colMax)
                    {
                        colMax = System.Math.Abs(result[i][j]);
                        pRow = i;
                    }
                }
                // Not sure if this approach is needed always, or not.

                if (pRow != j) // if largest value not on pivot, swap rows
                {
                    double[] rowPtr = result[pRow];
                    result[pRow] = result[j];
                    result[j] = rowPtr;

                    int tmp = perm[pRow]; // and swap perm info
                    perm[pRow] = perm[j];
                    perm[j] = tmp;

                    toggle = -toggle; // adjust the row-swap toggle
                }

                // -------------------------------------------------------------
                // This part added later (not in original code) 
                // and replaces the 'return null' below.
                // if there is a 0 on the diagonal, find a good row 
                // from i = j+1 down that doesn't have
                // a 0 in column j, and swap that good row with row j

                if (result[j][j] == 0.0)
                {
                    // find a good row to swap
                    int goodRow = -1;
                    for (int row = j + 1; row < n; ++row)
                    {
                        if (result[row][j] != 0.0)
                            goodRow = row;
                    }

                    if (goodRow == -1)
                        throw new Exception("Cannot use Doolittle's method");

                    // swap rows so 0.0 no longer on diagonal
                    double[] rowPtr = result[goodRow];
                    result[goodRow] = result[j];
                    result[j] = rowPtr;

                    int tmp = perm[goodRow]; // and swap perm info
                    perm[goodRow] = perm[j];
                    perm[j] = tmp;

                    toggle = -toggle; // adjust the row-swap toggle
                }
                // -------------------------------------------------------------

                //if (Math.Abs(result[j][j]) < 1.0E-20) // deprecated
                //  return null; // consider a throw

                for (int i = j + 1; i < n; ++i)
                {
                    result[i][j] /= result[j][j];
                    for (int k = j + 1; k < n; ++k)
                    {
                        result[i][k] -= result[i][j] * result[j][k];
                    }
                }

            } // main j column loop

            return result;
        } // MatrixDecompose

        // -------------------------------------------------------------

        public static double[][] Product(double[][] matrixA, double[][] matrixB)
        {
            int aRows = matrixA.Length; int aCols = matrixA[0].Length;
            int bRows = matrixB.Length; int bCols = matrixB[0].Length;
            if (aCols != bRows)
                throw new Exception("Non-conformable matrices in MatrixProduct");

            double[][] result = MatrixCreate(aRows, bCols);

            for (int i = 0; i < aRows; ++i) // each row of A
                for (int j = 0; j < bCols; ++j) // each col of B
                    for (int k = 0; k < aCols; ++k) // could use k < bRows
                        result[i][j] += matrixA[i][k] * matrixB[k][j];

            //Parallel.For(0, aRows, i =>
            //  {
            //    for (int j = 0; j < bCols; ++j) // each col of B
            //      for (int k = 0; k < aCols; ++k) // could use k < bRows
            //        result[i][j] += matrixA[i][k] * matrixB[k][j];
            //  }
            //);

            return result;
        }

        public static double[][] Product(double[][] m, double x)
        {
            // multiple all cells in m by scalar x
            double[][] result = Duplicate(m); // copy
            for (int i = 0; i < result.Length; ++i)
                for (int j = 0; j < result[i].Length; ++j)
                    result[i][j] *= x;
            return result;
        }

        public static double[][] Add(double[][] a, double[][] b)
        {
            // return a-b
            int rows = a.Length; int cols = a[0].Length;
            double[][] result = MatrixCreate(rows, cols);
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < cols; ++j)
                    result[i][j] = a[i][j] + b[i][j];
            return result;
        }

        public static double[][] Subtract(double[][] a, double[][] b)
        {
            // return a-b
            int rows = a.Length; int cols = a[0].Length;
            double[][] result = MatrixCreate(rows, cols);
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < cols; ++j)
                    result[i][j] = a[i][j] - b[i][j];
            return result;
        }

        public static double[][] Transpose(double[][] matrix)
        {
            int rows = matrix.Length;
            int cols = matrix[0].Length;
            double[][] result = MatrixCreate(cols, rows); // note indexing
            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    result[j][i] = matrix[i][j];
                }
            }
            return result;
        } // TransposeMatrix

        public static double[][] MatrixFromVector(double[] vector)
        {
            // return a column vector-matrix
            int len = vector.Length;
            double[][] result = MatrixCreate(len, 1); // 1 colum
            for (int i = 0; i < len; ++i)
                result[i][0] = vector[i];
            return result;
        }

        public static double[][] MatrixCreate(int rows, int cols)
        {
            // allocates/creates a matrix initialized to all 0.0
            // do error checking here
            double[][] result = new double[rows][];
            for (int i = 0; i < rows; ++i)
                result[i] = new double[cols];
            return result;
        }

        public static double[][] Unitize(double[][] vector)
        {
            // return column vector scaled to unit length
            if (vector[0].Length != 1)
                throw new Exception("Not column vector");

            int len = vector.Length;
            double[][] result = MatrixHelper.MatrixCreate(len, 1);
            double sum = 0.0;
            for (int i = 0; i < len; ++i)
                sum += vector[i][0] * vector[i][0];
            sum = System.Math.Sqrt(sum); // check if 0
            for (int i = 0; i < len; ++i)
                result[i][0] = vector[i][0] / sum;
            return result;
        }

        public static void Show(double[][] m, int dec, bool newl)
        {
            for (int i = 0; i < m.Length; ++i)
            {
                for (int j = 0; j < m[i].Length; ++j)
                {
                    if (m[i][j] >= 0.0) Console.Write(" "); // '+'
                    Console.Write(m[i][j].ToString("F" + dec) + "  ");
                }
                Console.WriteLine("");
            }
            if (newl) Console.WriteLine("");
        }

        public static void ShowVector(double[] v, int dec)
        {
            for (int i = 0; i < v.Length; ++i)
                Console.Write(v[i].ToString("F" + dec) + "  ");
            Console.WriteLine("");
        }
    }
}
