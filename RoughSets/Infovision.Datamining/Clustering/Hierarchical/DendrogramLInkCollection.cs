﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Clustering.Hierarchical
{
    public class DendrogramLinkCollection : IEnumerable<DendrogramLink>
    {        
        private DendrogramLink[] linkages;
        int nextLinkagesIdx = 0;
        private Dictionary<int, DendrogramNode> nodeDictionary;
        private DendrogramNode rootNode;
        private DendrogramNode lastAddedNode;
        private int numOfInstances;

        public int Count
        {
            get { return linkages.Length; }
        }

        public double MaxHeight
        {
            get
            {
                if (this.rootNode != null)
                    return this.rootNode.Height;
                else if (this.lastAddedNode != null)
                    return this.lastAddedNode.Height;
                else
                    return 0;
            }
        }

        public DendrogramLinkCollection(int numOfInstances)
        {
            this.numOfInstances = numOfInstances;
            linkages = new DendrogramLink[this.numOfInstances - 1];
            nodeDictionary = new Dictionary<int, DendrogramNode>(this.numOfInstances - 1);
        }
        
        public void Add(int cluster1, int cluster2, double distance, int mergedClusterId, bool isRoot)
        {
            int c1 = cluster1;
            int c2 = cluster2;

            if (cluster2 < cluster1)
            {
                c1 = cluster2;
                c2 = cluster1;
            }

            linkages[nextLinkagesIdx++] = new DendrogramLink(mergedClusterId, c1, c2, distance);

            DendrogramNode newNode;
            if (c1 < this.numOfInstances && c2 < this.numOfInstances)
            {
                newNode = new DendrogramNode
                {
                    NodeId = mergedClusterId,
                    Parent = null,
                    
                    LeftNode = null,
                    LeftInstance = c1,
                    LeftLength = distance,
                    
                    RightNode = null,
                    RightInstance = c2,
                    RightLength = distance,
                    
                    Height = distance
                };

                nodeDictionary.Add(mergedClusterId, newNode);
            }
            else if (c1 < this.numOfInstances && c2 >= this.numOfInstances)
            {
                DendrogramNode rightNode = nodeDictionary[c2];
                
                newNode = new DendrogramNode
                {
                    NodeId = mergedClusterId,
                    Parent = null,

                    LeftNode = null,
                    LeftInstance = c1,
                    LeftLength = distance,

                    RightNode = rightNode,
                    RightInstance = c2,
                    RightLength = distance - rightNode.Height,
                    Height = distance
                };

                rightNode.Parent = newNode;

                nodeDictionary.Add(mergedClusterId, newNode);
            }
            else if (c1 >= this.numOfInstances && c2 < this.numOfInstances)
            {
                DendrogramNode leftNode = nodeDictionary[c1];
                
                newNode = new DendrogramNode
                {
                    NodeId = mergedClusterId,
                    Parent = null,

                    LeftNode = leftNode,
                    LeftInstance = c1,
                    LeftLength = distance - leftNode.Height,

                    RightNode = null,
                    RightInstance = c2,
                    RightLength = distance,
                    
                    Height = distance
                };

                leftNode.Parent = newNode;

                nodeDictionary.Add(mergedClusterId, newNode);
            }
            else //both clusters are previouslcluster2 merged clusters
            {
                DendrogramNode leftNode = nodeDictionary[c1];
                DendrogramNode rightNode = nodeDictionary[c2];

                newNode = new DendrogramNode
                {
                    NodeId = mergedClusterId,
                    Parent = null,

                    LeftNode = leftNode,
                    LeftInstance = c1,
                    LeftLength = distance - leftNode.Height,

                    RightNode = rightNode,
                    RightInstance = c2,
                    RightLength = distance - rightNode.Height,
                    
                    Height = distance
                };

                leftNode.Parent = newNode;
                rightNode.Parent = newNode;

                nodeDictionary.Add(mergedClusterId, newNode);
            }

            if (isRoot)
                this.rootNode = newNode;

            this.lastAddedNode = newNode;
        }

        public void SetRoot(int clusterId, bool isRoot)
        {
            if (nodeDictionary.ContainsKey(clusterId))
            {
                rootNode = isRoot ? nodeDictionary[clusterId] : null;
            }
        }

        public int[] ComputeLeafNodesFromTree()
        {
            int[] order = new int[this.Count + 1];
            int pos = 0;
            WalkChildrenTree(ref order, ref pos, this.rootNode);
            return order;
        }

        private void WalkChildrenTree(ref int[] order, ref int pos, DendrogramNode node)
        {
            if (node == null)            
                throw new ArgumentNullException("node");

            if(node.LeftNode != null)
                WalkChildrenTree(ref order, ref pos, node.LeftNode);
            else
                order[pos++] = node.LeftInstance;

            if(node.RightNode != null)
                WalkChildrenTree(ref order, ref pos, node.RightNode);
            else
                order[pos++] = node.RightInstance;
        }

        public int[] ComputeLeafNodes()
        {
            int[] order = new int[this.Count + 1];
            int pos = 0;
            ReorderChildren();
            WalkChildren(ref order, ref pos, this.Count - 1);
            return order;
        }

        private void WalkChildren(ref int[] order, ref int pos, int row)
        {
            int n = this.Count + 1;
            int node = this.linkages[row].Cluster1;

            if (node >= n)
            {
                WalkChildren(ref order, ref pos, node - n);
            }

            if (node < n)
                order[pos++] = node;

            node = this.linkages[row].Cluster2;
            if (node >= n)
            {
                WalkChildren(ref order, ref pos, node - n);
            }

            if (node < n)
                order[pos++] = node;
        }

        private void ReorderChildren()
        {
            int n = this.Count + 1;
            for (int i = 0; i < this.Count; i++)
            {                
                if (this.linkages[i].Cluster1 < n && this.linkages[i].Cluster2 < n)
                {
                    if (this.linkages[i].Cluster1 > this.linkages[i].Cluster2)
                    {
                        Swap(i);
                    }
                }
                else if (this.linkages[i].Cluster1 < n && this.linkages[i].Cluster2 >= n)
                {
                    Swap(i);
                }
                else if (this.linkages[i].Cluster1 >= n && this.linkages[i].Cluster2 >= n)
                {
                    if (this.linkages[i].Cluster1 > this.linkages[i].Cluster2)
                    {
                        Swap(i);
                    }
                }
            }
        }

        private void Swap(int row)
        {
            linkages[row] = new DendrogramLink(linkages[row].Id, linkages[row].Cluster2, linkages[row].Cluster1, linkages[row].Distance);                        
        }

        private int[] CutOff(int numberOfClusters)
        {                        
            if(numberOfClusters <= 0)
                return new int[0];

            int size = System.Math.Min(this.numOfInstances, System.Math.Max(numberOfClusters, 0));
            int[] result = new int[size];

            if(size == this.numOfInstances)
            {
                Array.Copy(linkages, result, size);
                return result;
            }

            int level = 0;
            for (int i = this.linkages.Length - 1; i >= 0; i--)
            {
                result[level++] = this.linkages[i].Id;
                if (level >= numberOfClusters)
                    break;
            }

            return result;
        }

        private int[] CutOff(double distanceThreshold)
        {            
            int level = 0;                        
            for (int i = this.linkages.Length - 1; i >= 0; i--)
            {                
                if (this.linkages[i].Distance < distanceThreshold)
                    break;
                level++;
            }

            if(level > this.numOfInstances - 1)
                level = this.numOfInstances - 1;

            int numberOfClusters = level + 1;
            int[] result = new int[numberOfClusters];

            level = 0;
            for (int i = this.linkages.Length - 1; i >= 0; i--)
            {
                result[level++] = this.linkages[i].Id;
                if (level >= numberOfClusters)
                    break;
            }             

            return result;
        }

        public int[] GetClusterMembership(int numberOfClusters)
        {
            int[] clusters = this.CutOff(numberOfClusters);
            return this.GetClusterMembership(clusters);            
        }

        public int[] GetClusterMembership(double distanceThreshold)
        {
            int[] clusters = this.CutOff(distanceThreshold);
            return this.GetClusterMembership(clusters);
        }

        private int[] GetClusterMembership(int[] clusters)
        {
            int[] result = new int[this.numOfInstances];

            for (int i = 0; i < clusters.Length; i++)
            {
                if(clusters[i] < this.numOfInstances)
                {
                    result[clusters[i]] = clusters[i];
                }
                else
                {
                    DendrogramNode node = nodeDictionary[clusters[i]];
                    WalkClusterMembershipFromNode(clusters[i], ref result, node);               
                }
            }

            return result;
        }

        private void WalkClusterMembershipFromNode(int clusterId, ref int[] result, DendrogramNode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (node.LeftNode != null)
                WalkClusterMembershipFromNode(clusterId, ref result, node.LeftNode);
            else
                result[node.LeftInstance] = clusterId;

            if (node.RightNode != null)
                WalkClusterMembershipFromNode(clusterId, ref result, node.RightNode);
            else
                result[node.RightInstance] = clusterId;
        }        

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (DendrogramLink link in this)
                sb.AppendLine(link.ToString());            
            return sb.ToString();
        }

        /// <summary>
        ///   Returns an enumerator that iterates through a collection.
        /// </summary>
        /// 
        /// <returns>
        ///   An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// 
        public IEnumerator<DendrogramLink> GetEnumerator()
        {
            return linkages.AsEnumerable().GetEnumerator();
        }

        /// <summary>
        ///   Returns an enumerator that iterates through a collection.
        /// </summary>
        /// 
        /// <returns>
        ///   An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// 
        IEnumerator IEnumerable.GetEnumerator()
        {
            return linkages.GetEnumerator();
        } 
    }
}
