using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infovision.Datamining.Clustering.Hierarchical
{
    [Serializable]
    public class HierarchicalClusteringSimple : HierarchicalClustering
    {
        public HierarchicalClusteringSimple()
            : base()
        {
        }

        public HierarchicalClusteringSimple(Func<double[], double[], double> distance)
            : base(distance)
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
                    double minObjectDistance = double.MaxValue;
                    foreach (int objectIdA in clusters[clusterIds[j]].MemberObjects)
                    {
                        foreach (int objectIdB in clusters[clusterIds[k]].MemberObjects)
                        {
                            double distance = distanceMatrix.GetDistance(objectIdA, objectIdB);
                            
                            //TODO to be substituted be linkage delegate
                            if (distance < minObjectDistance)
                            {
                                minObjectDistance = distance;
                            }
                        }
                    }

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
