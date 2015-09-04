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
        private double currentAvgNodeDistance = 0.0;

        public HierarchicalClusteringIncrementalExt()
            : base()
        {                        
        }

        public HierarchicalClusteringIncrementalExt(Func<double[], double[], double> distance,
                                                    Func<int[], int[], DistanceMatrix, double[][], double> linkage)
            : base(distance, linkage)
        {         
        }

        public override bool AddToCluster(int id, double[] instance)
        {

            if (this.NumberOfInstances < this.MinimumNumberOfInstances
                || (this.NumberOfInstances >= this.MinimumNumberOfInstances
                    && this.NumberOfInstances <= 1))
            {
                return base.AddToCluster(id, instance);                
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

            //TODO remove this line            
            Console.WriteLine("{0} {1}", id, newAvgNodeDistance);

            //TODO commented out for test purposes
            //dendrogram has been significantly changed
            //if (newAvgNodeDistance > currentAvgNodeDistance)
            {
                this.Root = initialClustering.Root;
                this.NextClusterId = initialClustering.NextClusterId;
                this.Nodes = initialClustering.Nodes;
                return true;
            }
            
            //remove instance
            this.RemoveInstance(id);
            foreach (KeyValuePair<int, double[]> kvp in this.Instances)
                this.DistanceMatrix.Remove(new MatrixKey(kvp.Key, id));
            return false;
        }
    }
}
