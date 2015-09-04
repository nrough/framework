using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Math;

namespace Infovision.Datamining.Clustering.Hierarchical
{
    public abstract class HierarchicalClusteringBase
    {        
        private DendrogramNode root;
        private Func<double[], double[], double> distance;
        private Func<int[], int[], DistanceMatrix, double[][], double> linkage;
        private DistanceMatrix distanceMatrix;
        private Dictionary<int, double[]> instances;
        private int nextNodeId;
        
        public delegate void DistanceChangedEventHandler(object sender, EventArgs e);
        private event DistanceChangedEventHandler distanceChanged;

        /// <summary>
        ///   Gets or sets the distance function used
        ///   as a distance metric between data points.
        /// </summary>
        /// 
        public Func<double[], double[], double> Distance
        {
            get { return this.distance; }
            set 
            { 
                this.distance = value;
                this.OnDistanceChanged(EventArgs.Empty);
            }
        }
        

        public Func<int[], int[], DistanceMatrix, double[][], double> Linkage
        {
            get { return this.linkage; }
            set { this.linkage = value; }
        }

        public DendrogramNode Root
        {
            get { return this.root; }
            set { this.root = value; }
        }               

        public int NumberOfInstances
        {
            get { return instances.Count; }
        }

        public int NextClusterId
        {
            get { return this.nextNodeId; }
            set { this.nextNodeId = value; }
        }

        public virtual DendrogramLinkCollection DendrogramLinkCollection
        {
            get 
            {
                DendrogramLinkCollection dlc = new DendrogramLinkCollection(this.NumberOfInstances);
                int i = this.NumberOfInstances - 2;
                Action<DendrogramNode> addToLinkCollection = delegate(DendrogramNode d)
                {
                    if(d.IsLeaf == false)
                        dlc[i--] = new DendrogramLink(d.Id,
                                                      d.LeftNode.Id, 
                                                      d.RightNode.Id, 
                                                      d.Height);
                };
                HierarchicalClusteringBase.BFSTraversal(this.Root, addToLinkCollection);
                dlc.Sort(new DendrogramLinkDistAscendingComparer());
                return dlc;
            }
        }

        public DistanceMatrix DistanceMatrix
        {
            get { return this.distanceMatrix; }
            set { this.distanceMatrix = value; }
        }

        public Dictionary<int, double[]> Instances
        {
            get { return this.instances; }
            set { this.instances = value; }
        }

        public HierarchicalClusteringBase()
        {
            Init();
        }
                
        public HierarchicalClusteringBase(Func<double[], double[], double> distance,
                                          Func<int[], int[], DistanceMatrix, double[][], double> linkage)
            : this()
        {
            if (distance == null)
                throw new ArgumentNullException("distance");

            if (linkage == null)
                throw new ArgumentNullException("linkage");

            this.Distance = distance;
            this.Linkage = linkage;
        }

        protected virtual void Init()
        {
            this.Distance = Similarity.Euclidean;
            this.Linkage = ClusteringLinkage.Complete;
            this.nextNodeId = -1;                        
            this.instances = new Dictionary<int, double[]>();
        }

        protected virtual int GetNextNodeId()
        {
            nextNodeId--;
            return nextNodeId;
        }

        public double[] GetInstance(int instanceId)
        {
            return this.instances[instanceId];
        }

        protected virtual void AddInstance(int instanceId, double[] data)
        {
            this.instances.Add(instanceId, data);
        }

        protected virtual void AddDendrogramNode(DendrogramNode node)
        {
        }
        
        public int[] ComputeLeafNodes()
        {
            int[] order = new int[this.NumberOfInstances];
            int pos = 0;
            WalkChildrenTree(ref order, ref pos, this.root);
            return order;
        }

        private void WalkChildrenTree(ref int[] order, ref int pos, DendrogramNode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (node.IsLeaf)
            {
                order[pos++] = node.Id;
                return;
            }
            else
            {
                if (node.LeftNode != null)
                    WalkChildrenTree(ref order, ref pos, node.LeftNode);
                
                if (node.RightNode != null)
                    WalkChildrenTree(ref order, ref pos, node.RightNode);
            }                        
        }

        protected virtual int[] GetLeaves(DendrogramNode node)
        {
            HashSet<int> leaves = new HashSet<int>();
            GetLeaves(node, leaves);
            return leaves.ToArray();
        }

        private void GetLeaves(DendrogramNode node, HashSet<int> leaves)
        {
            if (node.IsLeaf)
            {
                leaves.Add(node.Id);
                return;
            }

            if (node.LeftNode != null)
                GetLeaves(node.LeftNode, leaves);
            if (node.RightNode != null)
                GetLeaves(node.RightNode, leaves);
        }

        protected abstract double GetClusterDistance(int[] cluster1, int[] cluster2);

        public static void BFSTraversal(DendrogramNode node, Action<DendrogramNode> action)
        {            
            Queue<DendrogramNode> queue = new Queue<DendrogramNode>();
            queue.Enqueue(node);            

            while (queue.Count != 0)
            {
                DendrogramNode currentNode = queue.Dequeue();
                action.Invoke(currentNode);

                if (currentNode.LeftNode != null)
                    queue.Enqueue(currentNode.LeftNode);
                if (currentNode.RightNode != null)
                    queue.Enqueue(currentNode.RightNode);                
            }
        }

        public static void PreOrderTraversal(DendrogramNode node, Action<DendrogramNode> action)
        {
            if (node == null)
                return;

            action.Invoke(node);
            HierarchicalClusteringBase.PreOrderTraversal(node.LeftNode, action);
            HierarchicalClusteringBase.PreOrderTraversal(node.RightNode, action);
        }

        public static void InOrderTraversal(DendrogramNode node, Action<DendrogramNode> action)
        {
            if (node == null)
                return;
            
            HierarchicalClusteringBase.PreOrderTraversal(node.LeftNode, action);
            action.Invoke(node);
            HierarchicalClusteringBase.PreOrderTraversal(node.RightNode, action);
        }

        public static void PostOrderTraversal(DendrogramNode node, Action<DendrogramNode> action)
        {
            if (node == null)
                return;

            HierarchicalClusteringBase.PreOrderTraversal(node.LeftNode, action);            
            HierarchicalClusteringBase.PreOrderTraversal(node.RightNode, action);
            action.Invoke(node);
        }

        protected virtual void OnDistanceChanged(EventArgs e) 
        {
            if (distanceChanged != null)
                distanceChanged(this, e);
        }

        private int GetCutOffLevel(double distanceThreshold)
        {
            DendrogramLinkCollection linkages = this.DendrogramLinkCollection;
            int level = 0;
            for (int i = linkages.Count - 1; i >= 0; i--)
            {
                if (linkages[i].Distance < distanceThreshold)
                    break;
                level++;
            }

            if (level > this.NumberOfInstances - 1)
                level = this.NumberOfInstances - 1;

            int numberOfClusters = level + 1;
            return numberOfClusters;
        }

        public int[] GetClusterMembership(double distanceThreshold)
        {
            return this.GetClusterMembership(this.GetCutOffLevel(distanceThreshold));
        }

        public int[] GetClusterMembership(int numberOfClusters)
        {
            DendrogramLinkCollection linkages = this.DendrogramLinkCollection;
            int[] result = new int[this.NumberOfInstances];
            numberOfClusters = System.Math.Min(this.NumberOfInstances, System.Math.Max(numberOfClusters, 0));

            SetClusterIdRecursive(this.Root.Id, ref result, this.Root);
            for (int j = 0; j < numberOfClusters - 1; j++)
            {
                DendrogramNode node = this.FindNode(linkages[linkages.Count - 1 - j].Id);

                if (node.LeftNode != null)
                    SetClusterIdRecursive(node.LeftNode.Id, ref result, node.LeftNode);
                else
                    result[node.Id] = node.LeftNode.Id;

                if (node.RightNode != null)
                    SetClusterIdRecursive(node.RightNode.Id, ref result, node.RightNode);
                else
                    result[node.Id] = node.RightNode.Id;
            }

            return result;
        }

        public Dictionary<int, List<int>> GetClusterMembershipAsDict(int numberOfClusters)
        {
            int[] membership = this.GetClusterMembership(numberOfClusters);
            Dictionary<int, List<int>> result = new Dictionary<int, List<int>>();
            for (int i = 0; i < membership.Length; i++)
            {
                if (result.ContainsKey(membership[i]))
                {
                    List<int> tmpList = result[membership[i]];
                    tmpList.Add(i);
                }
                else
                {
                    List<int> tmpList = new List<int>();
                    tmpList.Add(i);
                    result.Add(membership[i], tmpList);
                }
            }

            return result;
        }

        private void SetClusterIdRecursive(int clusterId, ref int[] result, DendrogramNode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (node.IsLeaf)
            {
                result[node.Id] = clusterId;
                return;
            }
            
            if (node.LeftNode != null)
                SetClusterIdRecursive(clusterId, ref result, node.LeftNode);
            
            if (node.RightNode != null)
                SetClusterIdRecursive(clusterId, ref result, node.RightNode);            
        }

        protected abstract DendrogramNode FindNode(int nodeId);
        public abstract void Compute();
    }
}
