//
// NRough Framework
// http://www.nrough.net
//
// Copyright © Sebastian Widz, 2017
// authors at nrough.net

//This file is part of NRough.

//Foobar is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 3 of the License, or
//(at your option) any later version.
//
//Foobar is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Foobar; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//
using System;
using System.Collections.Generic;
using NRough.Math;

namespace NRough.MachineLearning.Clustering.Hierarchical
{
    [Serializable]
    public class HierarchicalClusteringIncrementalExt : HierarchicalClusteringSIHC
    {
        private HierarchicalClusteringIncrementalExt()
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
                //Console.WriteLine("{0} {1}", this.NumberOfInstances, correlation);

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

            Console.WriteLine("{0} {1}", id, correlation);

            this.Root = newClustering.Root;
            this.NextClusterId = newClustering.NextClusterId;
            this.Nodes = newClustering.Nodes;
            return true;
        }
        */
    }
}