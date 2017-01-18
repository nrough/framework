using System;
using Raccoon.Math;

namespace Raccoon.MachineLearning.Clustering.Hierarchical
{
    [Serializable]
    public class HierarchicalClusteringSimple : HierarchicalClustering
    {
        public HierarchicalClusteringSimple()
            : base()
        {
        }

        public HierarchicalClusteringSimple(Func<double[], double[], double> distance, Func<int[], int[], DistanceMatrix, double[][], double> linkage)
            : base(distance, linkage)
        {
        }

        public HierarchicalClusteringSimple(DistanceMatrix matrix, Func<int[], int[], DistanceMatrix, double[][], double> linkage)
            : base(matrix, linkage)
        {
        }

        protected override void CreateClusters()
        {
            double[,] clusterDistance = new double[this.NumberOfInstances, this.NumberOfInstances];
            for (int i = 0; i < this.NumberOfInstances; i++)
            {
                for (int j = i + 1; j < this.NumberOfInstances; j++)
                {
                    clusterDistance[i, j] = this.GetClusterDistance(i, j);
                    clusterDistance[j, i] = clusterDistance[i, j];
                }
            }

            int iMin1 = -1;
            int iMin2 = -1;
            int minSize1 = -1;
            int minSize2 = -1;

            for (int i = 0; i < this.NumberOfInstances - 1; i++)
            {
                double minDistance = double.MaxValue;
                for (int j = 0; j < this.NumberOfInstances; j++)
                {
                    int sizeJ = this.GetClusterSize(j);
                    if (sizeJ > 0)
                    {
                        for (int k = j + 1; k < this.NumberOfInstances; k++)
                        {
                            int sizeK = this.GetClusterSize(k);
                            if (sizeK > 0)
                            {
                                double distance = clusterDistance[j, k];
                                if (distance < minDistance)
                                {
                                    minDistance = distance;
                                    iMin1 = j;
                                    iMin2 = k;
                                    minSize1 = sizeJ;
                                    minSize2 = sizeK;
                                }
                                //prefer to join smaller clusters startFromIdx
                                else if (distance == minDistance
                                    && (sizeJ < minSize1 || sizeK < minSize2))
                                {
                                    iMin1 = j;
                                    iMin2 = k;
                                    minSize1 = sizeJ;
                                    minSize2 = sizeK;
                                }
                            }
                        }
                    }
                }

                this.MergeClustersSimple(iMin1, iMin2, minDistance);

                // update distances
                for (int j = 0; j < NumberOfInstances; j++)
                {
                    if (j != iMin1 && this.GetClusterSize(j) != 0)
                    {
                        int i1 = System.Math.Min(iMin1, j);
                        int i2 = System.Math.Max(iMin1, j);
                        double distance = this.GetClusterDistance(i1, i2);
                        clusterDistance[i1, i2] = distance;
                        clusterDistance[i2, i1] = distance;
                    }
                }
            }
        }
    }
}