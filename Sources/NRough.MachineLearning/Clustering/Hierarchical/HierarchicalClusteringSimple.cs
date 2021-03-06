﻿// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

using System;
using NRough.Math;

namespace NRough.MachineLearning.Clustering.Hierarchical
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