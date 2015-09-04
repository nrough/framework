using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Clustering.Hierarchical
{
    public class DendrogramLinkCollection : IEnumerable<DendrogramLink>
    {        
        List<DendrogramLink> linkages;
        Dictionary<int, DendrogramNode> nodeDictionary;
        DendrogramNode rootNode;
        int numOfInstances;

        public int Count
        {
            get { return linkages.Count; }
        }

        public DendrogramLinkCollection(int numOfInstances)
        {
            linkages = new List<DendrogramLink>();
            nodeDictionary = new Dictionary<int, DendrogramNode>();
            this.numOfInstances = numOfInstances;
        }
        
        public void Add(int cluster1, int cluster2, double distance, int mergedClusterId, bool isRoot)
        {
            linkages.Add(new DendrogramLink(cluster1, cluster2, distance));

            DendrogramNode newNode;
            if (cluster1 < this.numOfInstances && cluster2 < this.numOfInstances)
            {
                newNode = new DendrogramNode
                {
                    NodeId = mergedClusterId,
                    Parent = null,
                    LeftNode = null,
                    LeftInstance = cluster1 < cluster2 ? cluster1 : cluster2,
                    LeftDistance = distance,
                    RightNode = null,
                    RightInstance = cluster1 < cluster2 ? cluster2 : cluster1,
                    RightDistance = distance
                };

                nodeDictionary.Add(mergedClusterId, newNode);
            }
            else if (cluster1 < this.numOfInstances && cluster2 >= this.numOfInstances)
            {
                DendrogramNode rightNode = nodeDictionary[cluster2];
                
                newNode = new DendrogramNode
                {
                    NodeId = mergedClusterId,
                    Parent = null,

                    LeftNode = null,
                    LeftInstance = cluster1,
                    LeftDistance = distance,

                    RightNode = rightNode,
                    RightInstance = cluster2,
                    RightDistance = distance
                };

                rightNode.Parent = newNode;

                nodeDictionary.Add(mergedClusterId, newNode);
            }
            else if (cluster1 >= this.numOfInstances && cluster2 < this.numOfInstances)
            {
                DendrogramNode leftNode = nodeDictionary[cluster1];
                
                newNode = new DendrogramNode
                {
                    NodeId = mergedClusterId,
                    Parent = null,

                    LeftNode = leftNode,
                    LeftInstance = cluster1,
                    LeftDistance = distance,

                    RightNode = null,
                    RightInstance = cluster2,
                    RightDistance = distance
                };

                leftNode.Parent = newNode;

                nodeDictionary.Add(mergedClusterId, newNode);
            }
            else //both clusters are previouslcluster2 merged clusters
            {
                DendrogramNode leftNode = nodeDictionary[cluster1];
                DendrogramNode rightNode = nodeDictionary[cluster2];

                newNode = new DendrogramNode
                {
                    NodeId = mergedClusterId,
                    Parent = null,

                    LeftNode = leftNode,
                    LeftInstance = cluster1,
                    LeftDistance = distance,

                    RightNode = rightNode,
                    RightInstance = cluster2,
                    RightDistance = distance
                };

                leftNode.Parent = newNode;
                rightNode.Parent = newNode;

                nodeDictionary.Add(mergedClusterId, newNode);
            }

            if (isRoot)
                this.rootNode = newNode;
        }

        public void SetRoot(int clusterId, bool isRoot)
        {
            if (nodeDictionary.ContainsKey(clusterId))
            {
                rootNode = isRoot ? nodeDictionary[clusterId] : null;
            }
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
            linkages[row] = new DendrogramLink(linkages[row].Cluster2, linkages[row].Cluster1, linkages[row].Distance);                        
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
            return linkages.GetEnumerator();
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
