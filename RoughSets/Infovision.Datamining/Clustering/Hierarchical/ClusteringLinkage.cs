using System;
using Raccoon.Math;

namespace Raccoon.MachineLearning.Clustering.Hierarchical
{
    [Serializable]
    public static class ClusteringLinkage
    {        
        //aka Min
        public static double Single(int[] cluster1, int[] cluster2, DistanceMatrix distanceMatrix, double[][] data)
        {
            double min = Double.MaxValue;
            foreach (int a in cluster1)
            {
                foreach (int b in cluster2)
                {
                    double distance = distanceMatrix.GetDistance(a, b);
                    if (distance < min)
                        min = distance;
                }
            }
            return min;
        }

        //aka Max
        public static double Complete(int[] cluster1, int[] cluster2, DistanceMatrix distanceMatrix, double[][] data)
        {
            double max = double.MinValue;
            foreach (int a in cluster1)
            {
                foreach (int b in cluster2)
                {
                    double distance = distanceMatrix.GetDistance(a, b);
                    if (distance > max)
                        max = distance;
                }
            }
            return max;
        }

        public static double Average(int[] cluster1, int[] cluster2, DistanceMatrix distanceMatrix, double[][] data)
        {
            double sum = 0.0;
            int n = cluster1.Length * cluster2.Length;

            foreach (int a in cluster1)
                foreach (int b in cluster2)
                    sum += distanceMatrix.GetDistance(a, b);

            return n != 0 ? sum / (double)n : double.MaxValue;
        }

        //aka Group average
        public static double Mean(int[] cluster1, int[] cluster2, DistanceMatrix distanceMatrix, double[][] data)
        {
            double sum = 0;
            int[] merge = new int[cluster1.Length + cluster2.Length];
            Array.Copy(cluster1, 0, merge, 0, cluster1.Length);
            Array.Copy(cluster2, 0, merge, cluster1.Length, cluster2.Length);

            for (int i = 0; i < merge.Length; i++)
                for (int j = i + 1; j < merge.Length; j++)
                    sum += distanceMatrix.GetDistance(merge[i], merge[j]);

            int n = merge.Length;
            return n > 1 ? sum / (double)(n * (n - 1.0) / 2.0) : double.MaxValue;
        }

        /// <summary>
        ///     <para>finds the distance of the change in caused by merging the cluster.</para>
        ///     <para>The information of a cluster is calculated as the error sum of squares</para>
        ///     <para>of the centroids of the cluster and its members.</para>
        /// </summary>
        /// <param name="cluster1"></param>
        /// <param name="cluster2"></param>
        /// <param name="distanceMatrix"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double Ward(int[] cluster1, int[] cluster2, DistanceMatrix distanceMatrix, double[][] data)
        {
            double ESS1 = ClusteringLinkage.CalcESS(cluster1, distanceMatrix, data);
            double ESS2 = ClusteringLinkage.CalcESS(cluster2, distanceMatrix, data);

            int[] merged = new int[cluster1.Length + cluster2.Length];
            Array.Copy(cluster1, 0, merged, 0, cluster1.Length);
            Array.Copy(cluster2, 0, merged, cluster1.Length, cluster2.Length);

            double ESS = ClusteringLinkage.CalcESS(merged, distanceMatrix, data);

            return (ESS * merged.Length)
                    - (ESS1 * cluster1.Length)
                    - (ESS2 * cluster2.Length);
        }

        /// <summary>
        /// calculated error sum-of-squares for instances ward centroid
        /// </summary>
        /// <param name="cluster"></param>
        /// <param name="distanceMatrix"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private static double CalcESS(int[] cluster, DistanceMatrix distanceMatrix, double[][] data)
        {
            if (distanceMatrix.Distance == null)
                throw new InvalidOperationException("Distance method is not set.");

            double[] centroid = new double[data[0].Length];
            for (int i = 0; i < cluster.Length; i++)
                for (int j = 0; j < data[i].Length; j++)
                    centroid[j] += data[cluster[i]][j];

            for (int j = 0; j < data[0].Length; j++)
                centroid[j] /= cluster.Length;

            double fESS = 0;
            for (int i = 0; i < cluster.Length; i++)
                fESS += distanceMatrix.Distance(centroid, data[cluster[i]]);
            return fESS / cluster.Length;
        }

        //aka Median linkage (WPGMC)
        public static double Centroid(int[] cluster1, int[] cluster2, DistanceMatrix distanceMatrix, double[][] data)
        {
            // finds the distance of the centroids of the clusters
            double[] centroid1 = new double[data[0].Length];
            for (int i = 0; i < cluster1.Length; i++)
                for (int j = 0; j < data[cluster1[i]].Length; j++)
                    centroid1[j] += data[cluster1[i]][j];

            double[] centroid2 = new double[data[0].Length];
            for (int i = 0; i < cluster2.Length; i++)
                for (int j = 0; j < data[cluster2[i]].Length; j++)
                    centroid2[j] += data[cluster2[i]][j];

            for (int j = 0; j < data[0].Length; j++)
            {
                centroid1[j] /= cluster1.Length;
                centroid2[j] /= cluster2.Length;
            }

            return distanceMatrix.Distance(centroid1, centroid2);
        }

        public static double CompleteAdjusted(int[] cluster1, int[] cluster2, DistanceMatrix distanceMatrix, double[][] data)
        {
            double result = ClusteringLinkage.Complete(cluster1, cluster2, distanceMatrix, data);

            // calculate adjustment, which is the largest within cluster distance
            double maxDistance = 0;
            for (int i = 0; i < cluster1.Length; i++)
                for (int j = i + 1; j < cluster1.Length; j++)
                {
                    double distance = distanceMatrix.GetDistance(cluster1[i], cluster1[j]);
                    if (maxDistance < distance)
                        maxDistance = distance;
                }

            for (int i = 0; i < cluster2.Length; i++)
                for (int j = i + 1; j < cluster2.Length; j++)
                {
                    double distance = distanceMatrix.GetDistance(cluster2[i], cluster2[j]);
                    if (maxDistance < distance)
                    {
                        maxDistance = distance;
                    }
                }

            result -= maxDistance;

            return result;
        }
    }
}