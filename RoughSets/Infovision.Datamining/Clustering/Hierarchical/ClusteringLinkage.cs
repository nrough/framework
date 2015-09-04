using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Math;

namespace Infovision.Datamining.Clustering.Hierarchical
{
    [Serializable]
    public static class ClusteringLinkage
    {
        public static double Min(int[] pointsA, int[] pointsB, DistanceMatrix distanceMatrix)
        {
            double min = double.MaxValue;
            foreach (int a in pointsA)
            {
                foreach (int b in pointsB)
                {
                    double distance = distanceMatrix.GetDistance(a, b);
                    if (distance < min)
                        min = distance;
                }
            }
            return min;
        }

        public static double Max(int[] pointsA, int[] pointsB, DistanceMatrix distanceMatrix)
        {
            double max = double.MinValue;
            foreach (int a in pointsA)
            {
                foreach (int b in pointsB)
                {
                    double distance = distanceMatrix.GetDistance(a, b);
                    if (distance > max)
                        max = distance;
                }
            }
            return max;
        }

        public static double Average(int[] pointsA, int[] pointsB, DistanceMatrix distanceMatrix)
        {
            double sum = 0;
            int n = pointsA.Length * pointsB.Length;

            foreach (int a in pointsA)
            {
                foreach (int b in pointsB)
                {
                    sum += distanceMatrix.GetDistance(a, b);                    
                }
            }
            return n != 0 ? sum / (double) n : Double.MaxValue;            
        }

        public static double Mean(int[] pointsA, int[] pointsB, DistanceMatrix distanceMatrix)
        {
            double sum = 0;
            int n = pointsA.Length + pointsB.Length;
            int[] merge = new int[n];
            Array.Copy(pointsA, 0, merge, 0, pointsA.Length);
            Array.Copy(pointsB, 0, merge, pointsA.Length, pointsB.Length);

            for (int i = 0; i < merge.Length; i++)
                for (int j = i + 1; j < merge.Length; j++)
                    sum += distanceMatrix.GetDistance(merge[i], merge[j]);

            return n > 1 ? sum / (double) (n * (n - 1.0) / 2.0) : Double.MaxValue;
        }                
    }
}
