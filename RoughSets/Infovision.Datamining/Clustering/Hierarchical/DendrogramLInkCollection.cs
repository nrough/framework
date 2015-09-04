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
        private List<DendrogramLink> linkages;
        private Dictionary<int, DendrogramNode> nodeDictionary;
        private DendrogramNode rootNode;
        private int numOfInstances;

        public int Count
        {
            get { return linkages.Count; }
        }

        public DendrogramLinkCollection(int numOfInstances)
        {
            this.numOfInstances = numOfInstances;
            linkages = new List<DendrogramLink>(this.numOfInstances - 1);
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
                        
            linkages.Add(new DendrogramLink(c1, c2, distance));

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
