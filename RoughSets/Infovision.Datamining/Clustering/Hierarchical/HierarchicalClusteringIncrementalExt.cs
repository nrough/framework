using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Math;

namespace Infovision.Datamining.Clustering.Hierarchical
{
    [Serializable]
    public class HierarchicalClusteringIncrementalExt : HierarchicalClusteringSIHC
    {
        HierarchicalClusteringIncrementalExt()
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
            base.AddToCluster(id, instance);

            if (this.NumberOfInstances % this.MinimumNumberOfInstances == 0
                    && this.NumberOfInstances != this.MinimumNumberOfInstances)
            {
                HierarchicalClustering newClustering = new HierarchicalClustering(this.Distance, this.Linkage);
                newClustering.DistanceMatrix = new DistanceMatrix(this.DistanceMatrix);
                newClustering.Instances = new Dictionary<int, double[]>(this.Instances);
                newClustering.Compute();

                double correlation = newClustering.BakersGammaIndex(this);
                Console.WriteLine("{0} {1}", this.NumberOfInstances, correlation);

                this.Root = newClustering.Root;
                this.NextClusterId = newClustering.NextClusterId;
                this.Nodes = newClustering.Nodes;
            }

            return true;
        }

        /*
        public override bool AddToCluster(int id, double[] instance)
        {
            if (this.NumberOfInstances < this.MinimumNumberOfInstances
                || (this.NumberOfInstances >= this.MinimumNumberOfInstances
                    && this.NumberOfInstances <= 1))
            {
                return base.AddToCluster(id, instance);                
            }                        

            HierarchicalClustering newClustering = new HierarchicalClustering(this.Distance, this.Linkage);
            newClustering.DistanceMatrix = new DistanceMatrix(this.DistanceMatrix);
            newClustering.Instances = new Dictionary<int, double[]>(this.Instances);

            //Add new instances to new clustering
            foreach (KeyValuePair<int, double[]> kvp in newClustering.Instances)
                newClustering.DistanceMatrix.Add(new MatrixKey(kvp.Key, id), newClustering.Distance(kvp.Value, instance));
            newClustering.AddInstance(id, instance);
            
            newClustering.Compute();            

            double correlation = newClustering.GetDendrogramCorrelation(this);

            //Add new instance
            foreach (KeyValuePair<int, double[]> kvp in this.Instances)
                this.DistanceMatrix.Add(new MatrixKey(kvp.Key, id), this.Distance(kvp.Value, instance));
            this.AddInstance(id, instance);
                        
            //TODO remove this line
            Console.WriteLine("{0} {1}", id, correlation);
            
            this.Root = newClustering.Root;
            this.NextClusterId = newClustering.NextClusterId;
            this.Nodes = newClustering.Nodes;
            return true;             
        }
        */

        
        /*
        //version complete vs. incremental
        public override bool AddToCluster(int id, double[] instance)
        {
            if (this.NumberOfInstances < this.MinimumNumberOfInstances
                || (this.MinimumNumberOfInstances <= 1 && this.NumberOfInstances <= 1))
            {
                return base.AddToCluster(id, instance);
            }

            HierarchicalClustering newClustering = new HierarchicalClustering(this.Distance, this.Linkage);
            newClustering.DistanceMatrix = new DistanceMatrix(this.DistanceMatrix);
            newClustering.Instances = new Dictionary<int, double[]>(this.Instances);

            //Add new instances to new clustering
            foreach (KeyValuePair<int, double[]> kvp in newClustering.Instances)
                newClustering.DistanceMatrix.Add(new MatrixKey(kvp.Key, id), newClustering.Distance(kvp.Value, instance));
            newClustering.AddInstance(id, instance);

            newClustering.Compute();

            HierarchicalClusteringIncremental incrementalClustering = new HierarchicalClusteringIncremental(this.Distance, this.Linkage);
            incrementalClustering.DistanceMatrix = new DistanceMatrix(this.DistanceMatrix);
            incrementalClustering.Instances = new Dictionary<int, double[]>(this.Instances);
            incrementalClustering.Root = this.Root;
            incrementalClustering.NextClusterId = this.NextClusterId;
            incrementalClustering.Nodes = this.Nodes;            
            incrementalClustering.AddToCluster(id, instance);


            //double correlation = newClustering.GetDendrogramCorrelation(incrementalClustering);
            double correlation = newClustering.BakersGammaIndex(incrementalClustering);

            //Add new instance
            foreach (KeyValuePair<int, double[]> kvp in this.Instances)
                this.DistanceMatrix.Add(new MatrixKey(kvp.Key, id), this.Distance(kvp.Value, instance));
            this.AddInstance(id, instance);

            //TODO remove this line
            Console.WriteLine("{0} {1}", id, correlation);

            this.Root = newClustering.Root;
            this.NextClusterId = newClustering.NextClusterId;
            this.Nodes = newClustering.Nodes;
            return true;
        }
        */
    }
}
