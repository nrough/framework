using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Math;

namespace Infovision.Datamining.Clustering.Hierarchical
{
    [Serializable]
    public class HierarchicalClusteringIncrementalExt : HierarchicalClusteringIncremental
    {
        private double currentAvgNodeDistance = 0;

        public HierarchicalClusteringIncrementalExt()
        {                        
        }

        public override void AddToCluster(int id, double[] instance)
        {

            if (this.NumberOfInstances < this.MinimumNumberOfInstances
                || (this.NumberOfInstances >= this.MinimumNumberOfInstances
                    && this.NumberOfInstances <= 1))
            {
                base.AddToCluster(id, instance);
                return;
            }

            //Add new instance
            foreach (KeyValuePair<int, double[]> kvp in this.Instances)
                this.DistanceMatrix.Add(new MatrixKey(kvp.Key, id), this.Distance(kvp.Value, instance));
            this.AddInstance(id, instance);

            HierarchicalClustering initialClustering = new HierarchicalClustering(this.Distance, this.Linkage);
            initialClustering.DistanceMatrix = this.DistanceMatrix;
            initialClustering.Instances = this.Instances;
            initialClustering.Compute();

            double newAvgNodeDistance = initialClustering.GetAvgNodeLevelDistance();

            //dendrogram has been significantly changed
            if (newAvgNodeDistance > currentAvgNodeDistance)
            {
                this.Root = initialClustering.Root;
                this.NextClusterId = initialClustering.NextClusterId;
                this.Nodes = initialClustering.Nodes;
            }
            else
            {
                //remove instance
                this.RemoveInstance(id);
                foreach (KeyValuePair<int, double[]> kvp in this.Instances)
                    this.DistanceMatrix.Remove(new MatrixKey(kvp.Key, id));                
            }      
        }
    }
}
