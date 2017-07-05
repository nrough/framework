// 
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
using NRough.Math;

namespace NRough.MachineLearning.Clustering.Hierarchical
{
    [Serializable]
    public class HierarchicalClusteringSIHC : HierarchicalClusteringIncremental
    {
        public HierarchicalClusteringSIHC()
            : base()
        {
        }

        public HierarchicalClusteringSIHC(Func<double[], double[], double> distance,
                                                 Func<int[], int[], DistanceMatrix, double[][], double> linkage)
            : base(distance, linkage)
        {
        }

        public override bool AddToCluster(int id, double[] instance)
        {
            //base.AddToCluster(id, instance);
            foreach (KeyValuePair<int, double[]> kvp in this.Instances)
                this.DistanceMatrix.Add(new SymetricPair<int, int>(kvp.Key, id), this.Distance(kvp.Value, instance));
            this.AddInstance(id, instance);

            if (this.NumberOfInstances >= this.MinimumNumberOfInstances)
            {
                if (this.Root == null)
                {
                    if (this.NumberOfInstances > 1)
                    {
                        HierarchicalClustering initialClustering = new HierarchicalClustering(this.Distance, this.Linkage);
                        initialClustering.DistanceMatrix = this.DistanceMatrix;
                        initialClustering.Instances = this.Instances;
                        initialClustering.Compute();

                        this.Root = initialClustering.Root;
                        this.NextClusterId = initialClustering.NextClusterId;
                        this.Nodes = initialClustering.Nodes;

                        return true;
                    }
                    else
                    {
                        this.Root = new DendrogramNode(this.NextClusterId);
                        this.AddDendrogramNode(this.Root);
                        this.GetNextNodeId();
                    }
                }
            }
            else
            {
                return true;
            }

            //special case no nodes except root node
            if (this.NumberOfInstances - 1 == 0)
            {
                DendrogramNode newNode = new DendrogramNode
                {
                    Id = id
                };

                newNode.Parent = this.Root;
                this.Root.LeftNode = newNode;

                this.Nodes.Add(newNode.Id, newNode);
                return true;
            }
            //only one node (on the left) existing
            else if (this.NumberOfInstances - 1 == 1)
            {
                DendrogramNode newNode = new DendrogramNode
                {
                    Id = id
                };

                newNode.Parent = this.Root;
                this.Root.RightNode = newNode;
                double distance = this.Distance(instance, this.GetInstance(this.Root.LeftNode.Id));

                this.Root.Height = distance;
                this.Root.LeftLength = distance;
                this.Root.RightLength = distance;

                this.Nodes.Add(id, newNode);

                return true;
            }

            this.SIHC(id, this.Root);

            return true;
        }

        protected void SIHC(int newInstanceId, DendrogramNode node)
        {
            int[] cluster1 = this.GetLeaves(node);
            int[] cluster0 = new int[] { newInstanceId };
            double d = this.GetClusterDistance(cluster0, cluster1);

            if (node.Height <= d)
            {
                DendrogramNode singleton = new DendrogramNode
                {
                    Id = newInstanceId
                };

                this.Nodes.Add(singleton.Id, singleton);

                DendrogramNode newParentNode = new DendrogramNode
                {
                    Id = this.NextClusterId,
                    LeftNode = singleton,
                    RightNode = node,
                    Height = d,
                    Parent = node.Parent,
                    LeftLength = d,
                    RightLength = d - node.Height
                };

                //update child reference on current node parent
                if (node.Parent != null)
                {
                    if (node.Parent.LeftNode == node)
                    {
                        node.Parent.LeftNode = newParentNode;
                        node.Parent.LeftLength = node.Parent.Height - newParentNode.Height;
                    }
                    else
                    {
                        node.Parent.RightNode = newParentNode;
                        node.Parent.RightLength = node.Parent.Height - newParentNode.Height;
                    }
                }

                //set parent on new leaf node
                singleton.Parent = newParentNode;

                //update root
                if (node.Parent == null)
                    this.Root = newParentNode;

                //update parent on current node
                node.Parent = newParentNode;

                this.Nodes.Add(newParentNode.Id, newParentNode);

                this.GetNextNodeId();
            }
            else
            {
                int[] leftCluster = this.GetLeaves(node.LeftNode);
                int[] rightCluster = this.GetLeaves(node.RightNode);

                double leftDistance = this.GetClusterDistance(leftCluster, cluster0);
                double rightDistance = this.GetClusterDistance(rightCluster, cluster0);

                if (leftDistance < rightDistance)
                {
                    int[] mergedCluster = new int[leftCluster.Length + 1];
                    Array.Copy(leftCluster, mergedCluster, leftCluster.Length);
                    mergedCluster[leftCluster.Length] = newInstanceId;
                    double newDistance = this.GetClusterDistance(mergedCluster, rightCluster);
                    node.Height = newDistance;

                    this.SIHC(newInstanceId, node.LeftNode);
                }
                else
                {
                    int[] mergedCluster = new int[rightCluster.Length + 1];
                    Array.Copy(rightCluster, mergedCluster, rightCluster.Length);
                    mergedCluster[rightCluster.Length] = newInstanceId;
                    double newDistance = this.GetClusterDistance(mergedCluster, leftCluster);
                    node.Height = newDistance;

                    this.SIHC(newInstanceId, node.RightNode);
                }
            }
        }
    }
}