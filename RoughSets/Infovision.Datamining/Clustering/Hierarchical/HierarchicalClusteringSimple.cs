using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infovision.Math;

namespace Infovision.Datamining.Clustering.Hierarchical
{
    [Serializable]
    public class HierarchicalClusteringSimple : HierarchicalClustering
    {
        public HierarchicalClusteringSimple()
            : base()
        {
        }

        public HierarchicalClusteringSimple(Func<double[], double[], double> distance, Func<int[], int[], DistanceMatrix, double> linkage)
            : base(distance, linkage)
        {
        }
        
        protected override void CreateClusters()
        {
            while (clusters.Count > 1)
            {
                MatrixKey key = this.GetClustersToMerge();               
                HierarchicalCluster mergedCluster = HierarchicalCluster.MergeClusters(this.NextClusterId, clusters[key.X], clusters[key.Y]);

                this.RemoveCluster(key.X);
                this.RemoveCluster(key.Y);
                this.AddCluster(mergedCluster);               
            }
        }

        protected override MatrixKey GetClustersToMerge()
        {
            int[] key = new int[2] {-1, -1};
            int[] clusterIds = clusters.Keys.ToArray();                   
            double minClusterDistance = double.MaxValue;                        

            for (int j = 0; j < clusterIds.Length; j++)
            {
                for (int k = j + 1; k < clusterIds.Length; k++)
                {                    
                    double minObjectDistance = this.Linkage(clusters[clusterIds[j]].MemberObjects.ToArray(), clusters[clusterIds[k]].MemberObjects.ToArray(), this.distanceMatrix);                                                           

                    if (minObjectDistance < minClusterDistance)
                    {
                        minClusterDistance = minObjectDistance;

                        key[0] = clusterIds[j];
                        key[1] = clusterIds[k];
                    }
                }
            }

            return new MatrixKey(key[0], key[1]);                        
        }               
    }
}
