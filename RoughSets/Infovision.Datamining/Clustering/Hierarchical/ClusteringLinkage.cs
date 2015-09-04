using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Math;

namespace Infovision.Datamining.Clustering.Hierarchical
{
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

        public static double Mean(int[] pointsA, int[] pointsB, DistanceMatrix distanceMatrix)
        {
            double sum = 0;
            int size = pointsA.Length + pointsB.Length;

            foreach (int a in pointsA)
            {
                foreach (int b in pointsB)
                {
                    sum += distanceMatrix.GetDistance(a, b);                    
                }
            }
            return size != 0 ? sum / (double) size : 0;            
        }
    }
}
