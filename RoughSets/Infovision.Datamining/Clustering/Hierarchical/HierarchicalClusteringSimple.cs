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
            while (this.HasMoreClustersToMerge())
            {
                DendrogramLink link = this.GetClustersToMerge();
                this.MergeClusters(link);
            }
        }

        protected override DendrogramLink GetClustersToMerge()
        {
            int[] key = new int[2] {-1, -1};
            
            //TODO ToArray() is expensive
            //TODO clusters need to be private in parent class
            int[] clusterIds = clusters.Keys.ToArray();                   
            double minClusterDistance = double.MaxValue;                        

            for (int j = 0; j < clusterIds.Length; j++)
            {
                for (int k = j + 1; k < clusterIds.Length; k++)
                {
                    double minObjectDistance = this.GetClusterDistance(clusterIds[j], clusterIds[k]);                    

                    if (minObjectDistance < minClusterDistance)
                    {
                        minClusterDistance = minObjectDistance;

                        key[0] = clusterIds[j];
                        key[1] = clusterIds[k];
                    }
                }
            }

            return new DendrogramLink(key[0], key[1], minClusterDistance);                        
        }               
    }
}
