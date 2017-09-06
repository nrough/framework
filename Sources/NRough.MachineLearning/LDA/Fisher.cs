using NRough.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.LDA
{
    // Fisher's Linear Discriminate Analysis (LDA)
    // for binary classification
    // note: a different LDA is Latent Dirichlet Allocation 

    [CLSCompliant(true)]
    public class Fisher
    {
        public static int Prediction(double[][] data, double[] x, double[][] w)
        {
            // item to predict, x, is a normal array
            // convert array to 1-col matrix then call regular Prediction()
            double[][] xm = MatrixHelper.MatrixFromVector(x); // x to col matrix
            return Prediction(data, xm, w);
        }

        //https://msdn.microsoft.com/en-us/library/bhc3fa7f(v=vs.100).aspx (CLS warning described)
        public static int Prediction(double[][] data, double[][] x, double[][] w)
        {
            // item to predict, x, is a 1-col matrix (column vector)
            int dim = data[0].Length - 1; // at least 2
            double[][] wT = MatrixHelper.Transpose(w); // 1 x d
            double[][] m0 = Mean(data, 0); // d x 1
            double[][] m1 = Mean(data, 1); // d x 1
            double[][] m = MatrixHelper.Add(m0, m1); // d x 1
            m = MatrixHelper.Product(m, 0.5); // d x 1 

            // threshold constant c (distance from origin)
            double[][] tc = MatrixHelper.Product(wT, m); // ((1xd)(dx1) = 1 x 1
                                                  // distance from origin
            double[][] wTx = MatrixHelper.Product(wT, x); // (1xd)(dx1) = 1 x 1

            double adjust = 0.0;

            // optional weighting by probabilities:
            //int n0 = 0; int n1 = 0; // number each class
            //for (int i = 0; i < data.Length; ++i)
            //{
            //  int label = (int)data[i][dim];
            //  if (label == 0)
            //    ++n0;
            //  else
            //    ++n1;
            //}
            //double p0 = (n0 * 1.0) / data.Length;
            //double p1 = (n1 * 1.0) / data.Length;
            //double adjust = Math.Log(p0 / p1);

            if (wTx[0][0] - adjust > tc[0][0]) // a bit tricky
                return 0;
            else
                return 1;
        }

        public static double[][] Mean(double[][] data, int c)
        {
            // return mean of class c (0 or 1) as column vector
            int dim = data[0].Length - 1;
            double[][] result = MatrixHelper.MatrixCreate(dim, 1);
            int ct = 0;

            for (int i = 0; i < data.Length; ++i)
            {
                int label = (int)data[i][dim];
                if (label == c)
                {
                    for (int j = 0; j < dim; ++j)
                        result[j][0] += data[i][j];
                    ++ct;
                }
            } // i

            for (int i = 0; i < dim; ++i)
                result[i][0] /= ct;

            return result;
        } // Mean

        public static double[][] Discriminate(double[][] data, bool unitize)
        {
            // returns the w vector as a column 
            // calls helper ScatterWithin which calls Scatter
            double[][] mean0 = Mean(data, 0);
            double[][] mean1 = Mean(data, 1);
            Console.WriteLine("\nClass means: \n");
            MatrixHelper.Show(mean0, 2, true);
            MatrixHelper.Show(mean1, 2, true);

            double[][] Sw = ScatterWithin(data); // sum of S0 and S1
            Console.WriteLine("The within-class combined scatter matrix Sw: \n");
            MatrixHelper.Show(Sw, 4, true);

            double[][] SwInv = MatrixHelper.Inverse(Sw);

            double[][] diff = MatrixHelper.Subtract(mean0, mean1);
            double[][] w = MatrixHelper.Product(SwInv, diff);

            if (unitize == true)
                return MatrixHelper.Unitize(w);
            else
                return w;
        }

        public static double[][] ScatterWithin(double[][] data)
        {
            // Sw = within class scatter = S0 + S1
            double[][] S0 = Scatter(data, 0);
            double[][] S1 = Scatter(data, 1);
            Console.WriteLine("Scatter matrices S0 S1: \n");
            MatrixHelper.Show(S0, 4, true);
            MatrixHelper.Show(S1, 4, true);
            double[][] Sw = MatrixHelper.Add(S0, S1);
            return Sw;
        }

        public static double[][] Scatter(double[][] data, int c)
        {
            // scatter matrix of class c
            // [Sw (within class) is sum of class scatters]
            int dim = data[0].Length - 1;
            double[][] mean = Mean(data, c); // mean as col vector
            double[][] result = MatrixHelper.MatrixCreate(dim, dim); // d x d
            for (int i = 0; i < data.Length; ++i)
            {
                int label = (int)data[i][dim]; // 0 or 1
                if (label == c)
                {
                    double[][] x = MatrixHelper.MatrixCreate(dim, 1); // d x 1
                    for (int j = 0; j < dim; ++j)
                        x[j][0] = data[i][j];
                    double[][] diff = MatrixHelper.Subtract(x, mean); // d x 1
                    double[][] diffT = MatrixHelper.Transpose(diff);  // 1 x d
                    double[][] prod = MatrixHelper.Product(diff, diffT); // d x d

                    result = MatrixHelper.Add(result, prod); // accumulate
                }
            } // i
            return result;
        } // Scatter        

        // ============================================

        public static double[][] Covariance(double[][] data, int c, bool sample)
        {
            // covariance is essentially Scatter divided by number items
            int n = 0;
            int dim = data[0].Length - 1;
            for (int i = 0; i < data.Length; ++i)
            {
                int label = (int)data[i][dim];
                if (label == c)
                    ++n;
            }
            double[][] s = Scatter(data, c);
            double[][] result = MatrixHelper.Duplicate(s);
            for (int i = 0; i < dim; ++i)
            {
                for (int j = 0; j < dim; ++j)
                {
                    if (sample == true)
                        result[i][j] /= n - 1;
                    else
                        result[i][j] /= n;
                }
            }
            return result;
        } // Covariance

        public static double[][] Pooled(double[][] data, bool sample)
        {
            // pooled covariance: (n1*C1 + n2*C2) / (n1 + n2)
            int n0 = 0; int n1 = 0;
            int dim = data[0].Length - 1;
            for (int i = 0; i < data.Length; ++i)
            {
                int label = (int)data[i][dim]; // 0 or 1
                if (label == 0) ++n0; else ++n1;
            }

            double[][] C0 = Covariance(data, 0, sample);
            double[][] C1 = Covariance(data, 1, sample);

            double[][] result = MatrixHelper.MatrixCreate(dim, dim);
            for (int i = 0; i < dim; ++i)
                for (int j = 0; j < dim; ++j)
                    result[i][j] = (n0 * C0[i][j] + n1 * C1[i][j]) / (n0 + n1);
            return result;
        } // Pooled
    }

    // R language analysis for validation of C# program
    //> library(MASS)
    //>
    //> data1 = read.table(file.choose(), header=T, sep=',')
    //>
    //> data1
    //  Age Income Politic
    //1   1      4       A
    //2   2      2       A
    //3   3      3       A
    //4   6      6       B
    //5   7      7       B
    //6   8      9       B
    //7   8      7       B
    //8   9      8       B
    //>
    //> data1.lda <- lda(formula = Politic ~ Age + Income, data = data1)
    //>
    //> data1.lda

    //Prior probabilities of groups:
    //    A     B
    //0.375 0.625

    //Group means:
    //  Age Income
    //A 2.0    3.0
    //B 7.6    7.4

    //Coefficients of linear discriminants:
    //             LD1
    //Age    0.6859669
    //Income 0.3919811
    //>

    // Show the C# w vector is equivalent to the R result
    //> v = c(0.6859669, 0.3919811)
    //> len = sqrt(v[1]^2 + v[2]^2)
    //> w = v / len
    //> w
    //[1] 0.8682431 0.4961390
    //>

    // Make a prediction using R
    //> unknown <- data.frame(Age=4,Income=5,Politic='?')
    //> predict(data1.lda, unknown)
    //          A          B
    //1 0.9516662 0.04833379

    // a few LDA resources
    //See http://www.cbcb.umd.edu/~hcorrada/PracticalML/pdf/lectures/classification.pdf
    //See http://sites.stat.psu.edu/~jiali/course/stat597e/notes2/lda.pdf
    //See http://www.saedsayad.com/lda.htm
}
