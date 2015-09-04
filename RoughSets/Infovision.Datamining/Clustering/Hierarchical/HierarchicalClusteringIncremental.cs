using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Math;

namespace Infovision.Datamining.Clustering.Hierarchical
{
    public class HierarchicalClusteringIncremental
    {
        public static readonly int ROOT_ID = -1;
        private Func<double[], double[], double> distance;
        private Func<int[], int[], DistanceMatrix, double[][], double> linkage;
        private Dictionary<int, double[]> instances;
        private DendrogramNode root;
        private Dictionary<int, DendrogramNode> nodes;
        private DistanceMatrix distanceMatrix;
        int nextClusterId;

        /// <summary>
        ///   Gets or sets the distance function used
        ///   as a distance metric between data points.
        /// </summary>
        /// 
        public Func<double[], double[], double> Distance
        {
            get { return this.distance; }
            set { this.distance = value; }
        }

        public Func<int[], int[], DistanceMatrix, double[][], double> Linkage
        {
            get { return this.linkage; }
            set { this.linkage = value; }
        }

        public HierarchicalClusteringIncremental()
        {
            instances = new Dictionary<int, double[]>();
            nodes = new Dictionary<int, DendrogramNode>();
            root = new DendrogramNode(ROOT_ID);
            nodes.Add(root.NodeId, root);
            distanceMatrix = new DistanceMatrix(this.Distance);
            nextClusterId = ROOT_ID - 1;
        }

        public HierarchicalClusteringIncremental(Func<double[], double[], double> distance,
                                                 Func<int[], int[], DistanceMatrix, double[][], double> linkage)
            : this()
        {
            this.Distance = distance;
            this.Linkage = linkage;
        }

        public void AddInstance(int id, double[] instance)
        {                        
            //special case no nodes except root node
            if (instances.Count() == 0)
            {
                DendrogramNode newNode = new DendrogramNode
                {
                    NodeId = id
                };

                newNode.Parent = this.root;
                this.root.LeftInstance = newNode.NodeId;
                this.root.LeftNode = newNode;

                instances.Add(id, instance);
                nodes.Add(id, newNode);
                return;
            }
            //only one node (on the left) existing 
            else if (instances.Count() == 1)
            {
                DendrogramNode newNode = new DendrogramNode
                {
                    NodeId = id
                };

                newNode.Parent = this.root;
                this.root.RightInstance = newNode.NodeId;
                this.root.RightNode = newNode;
                double distance = this.Distance(instance, instances[this.root.LeftInstance]);
                this.root.Height = distance;

                distanceMatrix.Add(new MatrixKey(this.root.LeftInstance, id), distance);

                instances.Add(id, instance);
                nodes.Add(id, newNode);

                return;
            }

            //update distance Matrix
            foreach (KeyValuePair<int, double[]> kvp in instances)
            {
                this.distanceMatrix.Add(new MatrixKey(kvp.Key, id), this.Distance(kvp.Value, instance));
            }

            this.instances.Add(id, instance);            
            this.SIHC(id, this.root);            
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
                    NodeId = newInstanceId
                };

                nodes.Add(newInstanceId, singleton);

                DendrogramNode newParentNode = new DendrogramNode
                {
                    NodeId = nextClusterId,
                    LeftNode = singleton,
                    LeftInstance = singleton.NodeId,
                    RightNode = node,
                    RightInstance = node.NodeId,
                    Height = d,
                    Parent = node.Parent,
                    LeftLength = d,
                    RightLength = d - node.Height 
                };

                nodes.Add(nextClusterId, newParentNode);

                nextClusterId--;  
            }
            else
            {
                //TODO substitute with leftLenght and RightLenght?

                int[] leftCluster = this.GetLeaves(node.LeftNode);
                int[] rightCluster = this.GetLeaves(node.RightNode);

                double leftDistance = this.GetClusterDistance(leftCluster, cluster0);
                double rightDistance = this.GetClusterDistance(rightCluster, cluster0);


                if (leftDistance < rightDistance)
                {
                    //update_height
                    int[] mergedCluster = new int[leftCluster.Length + 1];
                    Array.Copy(leftCluster, mergedCluster, leftCluster.Length);
                    mergedCluster[leftCluster.Length] = newInstanceId;

                    double newDistance = this.GetClusterDistance(mergedCluster, rightCluster);
                    node.Height = newDistance;

                    this.SIHC(newInstanceId, node.LeftNode);
                }
                else
                {
                    //update_height
                    int[] mergedCluster = new int[rightCluster.Length + 1];
                    Array.Copy(rightCluster, mergedCluster, rightCluster.Length);
                    mergedCluster[rightCluster.Length] = newInstanceId;

                    double newDistance = this.GetClusterDistance(mergedCluster, leftCluster);
                    node.Height = newDistance;

                    this.SIHC(newInstanceId, node.RightNode);
                }
            }
        }        

        public void AddInstance2(int id, double[] instance)
        {
            //create Node for new instance
            DendrogramNode newNode = new DendrogramNode
            {
                NodeId = id
            };
            
            //special case no nodes except root node
            if (instances.Count() == 0)
            {
                newNode.Parent = this.root;
                this.root.LeftInstance = newNode.NodeId;
                this.root.LeftNode = newNode;
                
                instances.Add(id, instance);
                nodes.Add(id, newNode);
                return;
            }
            //only one node (on the left) existing 
            else if (instances.Count() == 1)
            {
                newNode.Parent = this.root;
                this.root.RightInstance = newNode.NodeId;
                this.root.RightNode = newNode;
                double distance = this.Distance(instance, instances[this.root.LeftInstance]);
                this.root.Height = distance;
                
                distanceMatrix.Add(new MatrixKey(id, this.root.LeftInstance), distance);

                instances.Add(id, instance);
                nodes.Add(id, newNode);
                
                return;
            }

            //find closest instance (leaf)
            int minId = 0;
            double minInstanceDistance = Double.MaxValue;
            foreach(KeyValuePair<int, double[]> kvp in this.instances)
            {
                double distance = this.Distance(instance, kvp.Value);
                distanceMatrix.Add(new MatrixKey(id, kvp.Key), distance);
                
                if (distance < minInstanceDistance)
                {
                    minInstanceDistance = distance;
                    minId = kvp.Key;
                }
            }

            this.instances.Add(id, instance);
            int[] cluster0 = { id };
            
            //move upwards to find the best level to insert new node
            DendrogramNode previousNode = this.nodes[minId];
            double prevDistance = minInstanceDistance;

            DendrogramNode currentNode = previousNode.Parent;
            
            while (currentNode.IsRoot != true)
            {
                int[] cluster = this.GetLeaves(currentNode);
                double d = this.GetClusterDistance(cluster0, cluster);

                if (d > minInstanceDistance)
                    break;

                previousNode = currentNode;
                currentNode = currentNode.Parent;
            }

            //currentNode is the new parent node
            //previousNode is the sibling of a new node

            DendrogramNode newParentNode = new DendrogramNode
            {
                NodeId = nextClusterId,
                LeftNode = previousNode,
                LeftInstance = previousNode.NodeId,
                RightNode = newNode,
                RightInstance = newNode.NodeId,
                Height = prevDistance,
                Parent = currentNode,
                LeftLength = 0, //TODO
                RightLength = 0 //TODO
            };
            
            nextClusterId--;

            //update parent
            if (previousNode.NodeId == currentNode.LeftInstance)
            {
                currentNode.LeftNode = newParentNode;
                currentNode.LeftInstance = newParentNode.NodeId;
                currentNode.LeftLength = 0; // TODO 
            }
            else
            {
                currentNode.RightNode = newParentNode;
                currentNode.RightInstance = newParentNode.NodeId;
                currentNode.RightLength = 0; // TODO 
            }

            //update the distances in the remaining parts of cluster
            //TODO


        }

        private int[] GetLeaves(DendrogramNode node)
        {
            HashSet<int> leaves = new HashSet<int>();
            GetLeaves(node, leaves);
            return leaves.ToArray();
        }

        private void GetLeaves(DendrogramNode node, HashSet<int> leaves)
        {
            if (node.IsLeaf)
            {
                leaves.Add(node.NodeId);
                return;
            }
            
            if (node.LeftNode != null)
                GetLeaves(node.LeftNode, leaves);
            if (node.RightNode != null)
                GetLeaves(node.RightNode, leaves);
        }

        protected double GetClusterDistance(int[] cluster1, int[] cluster2)
        {
            if (cluster1.Length == 1 && cluster2.Length == 1)
            {
                if (this.distanceMatrix.ContainsKey(new MatrixKey(cluster1[0], cluster1[0])))
                    return this.distanceMatrix.GetDistance(cluster1[0], cluster1[0]);
                else
                    return this.Distance(this.instances[cluster1[0]], this.instances[cluster2[0]]);
            }

            return this.Linkage(cluster1, cluster2, this.distanceMatrix, this.instances.Values.ToArray());
        }
    }
}
