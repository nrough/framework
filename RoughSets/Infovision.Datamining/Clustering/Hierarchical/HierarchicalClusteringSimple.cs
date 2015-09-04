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

        public HierarchicalClusteringSimple(DistanceMatrix matrix, Func<int[], int[], DistanceMatrix, double> linkage)
            : base(matrix, linkage)
        {
        }
        
        protected override void CreateClusters()
        {                        
            while (this.HasMoreClustersToMerge())
            {
                DendrogramLink link = this.GetClustersToMergeSimple();
                this.MergeClusters(link);
            }
        }                       
    }
}
