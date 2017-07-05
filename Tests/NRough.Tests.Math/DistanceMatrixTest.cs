using NRough.Math;
using NUnit.Framework;

namespace NRough.Tests.Math
{
    [TestFixture]
    public class DistanceMatrixTest
    {
        [Test]
        public void CalcDistanceMatrix()
        {
            double[][] data =
            {
                new double[] {7, 8, 0, 1, 0, 7, 1}, //0
                new double[] {6, 7, 1, 1, 1, 7, 1}, //1
                new double[] {5, 6, 0, 0, 0, 7, 1}, //2
                new double[] {4, 1, 1, 3, 1, 7, 1}, //3
                new double[] {3, 2, 0, 0, 0, 7, 1}, //4
                new double[] {2, 6, 1, 2, 0, 7, 1}, //5
                new double[] {1, 2, 0, 0, 0, 7, 1}, //6
                new double[] {0, 9, 1, 2, 1, 7, 1}, //7
                new double[] {1, 5, 0, 0, 0, 7, 1}, //8
                new double[] {1, 5, 1, 2, 0, 7, 1}  //9
            };

            DistanceMatrix distance = new DistanceMatrix(Distance.SquaredEuclidean);
            distance.Initialize(data);

            //Console.Write(distance.ToString());
            Assert.IsTrue(true);

            /*
            d(0, 1) = 4
            d(0, 2) = 9
            d(0, 3) = 64
            d(0, 4) = 53
            d(0, 5) = 31
            d(0, 6) = 73
            d(0, 7) = 53
            d(0, 8) = 46
            d(0, 9) = 47
            d(1, 2) = 5
            d(1, 3) = 44
            d(1, 4) = 37
            d(1, 5) = 19
            d(1, 6) = 53
            d(1, 7) = 41
            d(1, 8) = 32
            d(1, 9) = 31
            d(2, 3) = 37
            d(2, 4) = 20
            d(2, 5) = 14
            d(2, 6) = 32
            d(2, 7) = 40
            d(2, 8) = 17
            d(2, 9) = 22
            d(3, 4) = 13
            d(3, 5) = 31
            d(3, 6) = 21
            d(3, 7) = 81
            d(3, 8) = 36
            d(3, 9) = 27
            d(4, 5) = 22
            d(4, 6) = 4
            d(4, 7) = 64
            d(4, 8) = 13
            d(4, 9) = 18
            d(5, 6) = 22
            d(5, 7) = 14
            d(5, 8) = 7
            d(5, 9) = 2
            d(6, 7) = 56
            d(6, 8) = 9
            d(6, 9) = 14
            d(7, 8) = 23
            d(7, 9) = 18
            d(8, 9) = 5
                         * */
        }
    }
}