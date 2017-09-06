using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NRough.MachineLearning.LDA;
using NRough.Math;

namespace NRough.Tests.MachineLearning.LDA
{
    [TestFixture]
    public class FisherTest
    {
        [Test]
        public void Example()
        {
            Console.WriteLine("\nBegin linear discriminate analysis (LDA) demo\n");
            Console.WriteLine("Goal is to predict Politic (liberal = 0 or conservative = 1) ");
            Console.WriteLine("from person's Age and Income\n");

            double[][] data = new double[8][];
            data[0] = new double[] { 1.0, 4.0, 0 };
            data[1] = new double[] { 2.0, 2.0, 0 };
            data[2] = new double[] { 3.0, 3.0, 0 };

            data[3] = new double[] { 6.0, 6.0, 1 };
            data[4] = new double[] { 7.0, 7.0, 1 };
            data[5] = new double[] { 8.0, 9.0, 1 };
            data[6] = new double[] { 8.0, 7.0, 1 };
            data[7] = new double[] { 9.0, 8.0, 1 };

            Console.WriteLine("Normalized and encoded training data:\n");
            Console.WriteLine(" Age   Income  Politic");
            Console.WriteLine("----------------------");
            MatrixHelper.Show(data, 2, true);

            Console.WriteLine("\nCalculating the discriminate w");
            double[][] w = Fisher.Discriminate(data, true); // calls ScatterWithin which calls Scatter
            Console.WriteLine("Done calculating discriminate. Discriminate is: \n");
            MatrixHelper.Show(w, 6, true);

            double[] x = new double[] { 4.0, 5.0 };
            //double[] unknown = new double[] { 5.6, 5.0 };

            Console.WriteLine("Predicting class for Age Income =");
            MatrixHelper.ShowVector(x, 1);
            int c = Fisher.Prediction(data, x, w);
            Console.Write("\nPredicted class: " + c);
            if (c == 0)
                Console.WriteLine(" (liberal)");
            else
                Console.WriteLine(" (conservative)");

            // 2. pooled covariance approach to compute Discriminate
            double[][] pooled = Fisher.Pooled(data, false);
            double[][] pooledInv = MatrixHelper.Inverse(pooled);
            double[][] M0 = Fisher.Mean(data, 0);
            double[][] M1 = Fisher.Mean(data, 1);
            double[][] diff = MatrixHelper.Subtract(M0, M1);
            double[][] prod = MatrixHelper.Product(pooledInv, diff);
            double[][] pooledW = MatrixHelper.Unitize(prod); 
            MatrixHelper.Show(pooledW, 6, true); // exactly same as non-pooled scatter
            Console.WriteLine("\nDiscriminate calculated using pooled covariance:");
            MatrixHelper.Show(pooledW, 4, true);

            // Sb (between-class scatter) not explicitly used 
            //Two different equations for Sb
            //See https://www.ics.uci.edu/~welling/teaching/KernelsICS273B/Fisher-LDA.pdf

            Console.WriteLine("\nEnd LDA binary classification demo\n");
            Console.ReadLine();
        }
    }
}
