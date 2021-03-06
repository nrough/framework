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
using System.Collections.Generic;
using System.Linq;
using NRough.Math;
using NRough.Core;
using NRough.Core.DataStructures;

namespace NRough.MachineLearning.Clustering.Hierarchical
{
    [Serializable]
    public class HierarchicalClustering : HierarchicalClusteringBase
    {
        private PriorityQueue<HierarchicalClusterTuple, HierarchicalClusterTupleValueAscendingComparer> queue;
        private Dictionary<int, HierarchicalCluster> clusters;
        private Dictionary<int, DendrogramNode> nodes;

        private Dictionary<int, int> nodeIdLookupSimple;

        //TODO Do we need nodes dictionary?
        public Dictionary<int, DendrogramNode> Nodes
        {
            get { return this.nodes; }
            set { this.nodes = value; }
        }

        /// <summary>
        ///   Initializes a new instance of the HierarchicalClustering algorithm
        /// </summary>
        public HierarchicalClustering()
            : this(NRough.Math.Distance.SquaredEuclidean, ClusteringLinkage.Single) { }

        /// <summary>
        ///   Initializes a new instance of the HierarchicalClustering algorithm
        /// </summary>
        ///
        /// <param name="distance">The distance function to use. Default is to
        /// use the <see cref="NRough.Math.Distance.SquaredEuclidean(double[], double[])"/> distance.</param>
        /// <param name="linkage">The linkage function to use. Default is to
        /// use the <see cref="ClusteringLinkage.Single(int[], int[], DistanceMatrix)"/> linkage.</param>
        ///
        public HierarchicalClustering(Func<double[], double[], double> distance,
                                      Func<int[], int[], DistanceMatrix, double[][], double> linkage)
            : base(distance, linkage)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the HierarchicalClustering algorithm
        /// </summary>
        ///
        /// <param name="distanceMatrix">The distance matrix to use. </param>
        /// <param name="linkage">The linkage function to use. Default is to
        /// use the <see cref="ClusteringLinkage.Single(int[], int[], DistanceMatrix)"/> linkage.</param>
        ///
        public HierarchicalClustering(DistanceMatrix matrix, Func<int[], int[], DistanceMatrix, double[][], double> linkage)
            : base(matrix, linkage)
        {
            if (matrix.Distance != null)
                this.Distance = matrix.Distance;

            this.Linkage = linkage;

            this.DistanceMatrix = new DistanceMatrix();
            this.DistanceMatrix.Distance = matrix.Distance;
            foreach (KeyValuePair<SymetricPair<int, int>, double> kvp in matrix)
                this.DistanceMatrix.Add(new SymetricPair<int, int>(kvp.Key.Item1, kvp.Key.Item2), kvp.Value);
        }

        private void Initialize()
        {
            bool calculateDistanceMatrix = false;
            if (this.DistanceMatrix == null)
            {
                int size = this.Instances.Count * (this.Instances.Count - 1) / 2;
                this.DistanceMatrix = new DistanceMatrix(size, this.Distance);
                calculateDistanceMatrix = true;
            }

            //dendrogramLinkCollection = new DendrogramLinkCollection(this.Instances.Count);
            queue = new PriorityQueue<HierarchicalClusterTuple, HierarchicalClusterTupleValueAscendingComparer>();

            this.nodes = new Dictionary<int, DendrogramNode>();
            this.clusters = new Dictionary<int, HierarchicalCluster>(this.Instances.Count);

            int[] instanceKeys = this.Instances.Keys.ToArray();

            for (int i = 0; i < instanceKeys.Length; i++)
            {
                DendrogramNode singleton = new DendrogramNode
                {
                    Id = instanceKeys[i]
                };

                this.nodes.Add(instanceKeys[i], singleton);

                HierarchicalCluster cluster = new HierarchicalCluster(instanceKeys[i]);
                cluster.AddMemberObject(instanceKeys[i]);
                clusters.Add(instanceKeys[i], cluster);

                for (int j = i + 1; j < instanceKeys.Length; j++)
                {
                    if (calculateDistanceMatrix)
                        this.DistanceMatrix[instanceKeys[i], instanceKeys[j]] = this.Distance(this.Instances[instanceKeys[i]], this.Instances[instanceKeys[j]]);

                    HierarchicalClusterTuple tuple = new HierarchicalClusterTuple(instanceKeys[i], instanceKeys[j], this.DistanceMatrix[instanceKeys[i], instanceKeys[j]], 1, 1);
                    queue.Enqueue(tuple);
                }
            }
        }

        /// <summary>
        ///  Creates a hierarchy of clusters
        /// </summary>
        ///
        /// <param name="data">The data where to compute the algorithm.</param>
        public override void Compute()
        {
            if (this.Instances == null)
                throw new InvalidOperationException("Instances must be set before calling Compute()");

            if (this.Instances.Count == 0)
                return;

            this.Initialize();
            this.CreateClusters();

            this.Root = this.FindNode(this.nodes.Keys.Min());

            this.Cleanup();
        }

        protected int GetClusterSize(int clusterIdx)
        {
            return this.clusters[clusterIdx].Count;
        }

        protected bool ExistsCluster(int clusterIdx)
        {
            return this.clusters.ContainsKey(clusterIdx);
        }

        protected bool HasMoreClustersToMerge()
        {
            return this.clusters.Count > 1;
        }

        protected virtual void CreateClusters()
        {
            for (int i = 0; i < this.NumberOfInstances - 1; i++)
            {
                // use priority queue to find next best pair to cluster
                HierarchicalClusterTuple t;
                bool existsX = false, existsY = false;
                do
                {
                    t = queue.Dequeue();

                    if (t != null)
                    {
                        existsX = clusters.ContainsKey(t.X);
                        existsY = clusters.ContainsKey(t.Y);
                    }
                } while (t != null
                            && (!(existsX && existsY)
                                || ((existsX && clusters[t.X].MemberObjects.Count != t.SizeX)
                                    || (existsY && clusters[t.Y].MemberObjects.Count != t.SizeY))));

                if (t != null)
                {
                    int newClusterId = this.MergeClusters(t.X, t.Y, t.Value);

                    // update distances & queue
                    foreach (KeyValuePair<int, HierarchicalCluster> kvp in clusters)
                    {
                        if (kvp.Key != newClusterId)
                        {
                            int i1 = System.Math.Min(newClusterId, kvp.Key);
                            int i2 = System.Math.Max(newClusterId, kvp.Key);

                            double distance = this.GetClusterDistance(i1, i2);
                            HierarchicalClusterTuple newTuple = new HierarchicalClusterTuple(i1,
                                                                                             i2,
                                                                                              distance,
                                                                                             clusters[i1].MemberObjects.Count,
                                                                                             clusters[i2].MemberObjects.Count);
                            queue.Enqueue(newTuple);
                        }
                    }
                }
            }
        }

        protected int MergeClusters(int x, int y, double distance)
        {
            HierarchicalCluster destination = clusters[x];
            HierarchicalCluster source = clusters[y];
            HierarchicalCluster.MergeClustersInPlace(destination, source);

            clusters.Remove(y); //remove empty cluster
            clusters.Add(this.NextClusterId, destination); //add new merged cluster under new id
            clusters.Remove(x); //remove old cluster id

            int c1 = x;
            int c2 = y;
            if (c2 < c1)
            {
                c1 = y;
                c2 = x;
            }

            DendrogramNode leftNode = nodes[c1];
            DendrogramNode rightNode = nodes[c2];

            DendrogramNode newNode = new DendrogramNode
            {
                Id = this.NextClusterId,
                Parent = null,

                LeftNode = leftNode,
                LeftLength = distance - leftNode.Height,

                RightNode = rightNode,
                RightLength = distance - rightNode.Height,

                Height = distance
            };

            leftNode.Parent = newNode;
            rightNode.Parent = newNode;
            nodes.Add(newNode.Id, newNode);
            this.GetNextNodeId();
            return newNode.Id;
        }

        protected int MergeClustersSimple(int x, int y, double distance)
        {
            if (this.nodeIdLookupSimple == null)
            {
                this.nodeIdLookupSimple = new Dictionary<int, int>();
                foreach (KeyValuePair<int, double[]> kvp in this.Instances)
                    this.nodeIdLookupSimple.Add(kvp.Key, kvp.Key);
            }

            HierarchicalCluster destination = clusters[x];
            HierarchicalCluster source = clusters[y];
            HierarchicalCluster.MergeClustersInPlace(destination, source);

            //clusters.Remove(y); //remove empty cluster
            //clusters.Add(this.NextClusterId, destination); //add new merged cluster under new id
            //clusters.Remove(x); //remove old cluster id

            int c1 = x;
            int c2 = y;
            if (c2 < c1)
            {
                c1 = y;
                c2 = x;
            }

            DendrogramNode leftNode = nodes[this.nodeIdLookupSimple[c1]];
            DendrogramNode rightNode = nodes[this.nodeIdLookupSimple[c2]];

            DendrogramNode newNode = new DendrogramNode
            {
                Id = this.NextClusterId,
                Parent = null,

                LeftNode = leftNode,
                LeftLength = distance - leftNode.Height,

                RightNode = rightNode,
                RightLength = distance - rightNode.Height,

                Height = distance
            };

            leftNode.Parent = newNode;
            rightNode.Parent = newNode;
            nodes.Add(newNode.Id, newNode);

            this.nodeIdLookupSimple[x] = newNode.Id;
            this.nodeIdLookupSimple.Remove(y);

            this.GetNextNodeId();
            return newNode.Id;
        }

        public override double GetClusterDistance(int[] cluster1, int[] cluster2)
        {
            return this.Linkage(cluster1, cluster2, this.DistanceMatrix, Instances.Values.ToArray());
        }

        protected double GetClusterDistance(int i, int j)
        {
            if (clusters[i].Count == 1 && clusters[j].Count == 1)
                //assume that clusterId and objectId at the beginning are the same
                return this.DistanceMatrix.GetDistance(i, j);

            int[] cluster1 = clusters[i].MemberObjects.ToArray();
            int[] cluster2 = clusters[j].MemberObjects.ToArray();

            return this.Linkage(cluster1, cluster2, this.DistanceMatrix, Instances.Values.ToArray());
        }

        protected void Cleanup()
        {
            this.clusters = null;
            this.nodeIdLookupSimple = null;
        }

        protected override DendrogramNode FindNode(int nodeId)
        {
            return this.nodes[nodeId];
        }

        //TODO Implement toString()
        /*
        public override string ToString()
        {
            //StringBuilder sb = new StringBuilder();
            //Action<DendrogramNode>
            return DendrogramLinkCollection.ToString();
        }
        */
    }
}