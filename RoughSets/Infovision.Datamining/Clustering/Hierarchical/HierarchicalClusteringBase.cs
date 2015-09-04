using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Math;

namespace Infovision.Datamining.Clustering.Hierarchical
{
    [Serializable]
    public abstract class HierarchicalClusteringBase
    {        
        private DendrogramNode root;
        private Func<double[], double[], double> distance;
        private Func<int[], int[], DistanceMatrix, double[][], double> linkage;
        private DistanceMatrix distanceMatrix;
        private Dictionary<int, double[]> instances;
        private int nextNodeId;
        
        //Average leaf level distance        
        private bool isLeafDistanceCalculated;
        private double avgNodeLevelDistance;
        private Dictionary<int, int> node2RootDistance;
        private Dictionary<Tuple<int, int>, int> lca;
        
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
                HierarchicalClusteringBase.TraverseLevelOrder(this.Root, addToLinkCollection);
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

        public double AvgNodeLevelDistance
        {
            get
            {
                if (this.avgNodeLevelDistance < -1)
                {
                    this.avgNodeLevelDistance = this.GetAvgNodeLevelDistance();
                }

                return this.avgNodeLevelDistance;
            }
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
            this.avgNodeLevelDistance = Double.MinValue;           
            this.isLeafDistanceCalculated = false;
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

        protected virtual void RemoveInstance(int instanceId)
        {
            this.instances.Remove(instanceId);
        }

        protected virtual void AddDendrogramNode(DendrogramNode node)
        {
        }
        
        public virtual int[] GetLeaves()
        {
            int[] order = new int[this.NumberOfInstances];
            int pos = 0;
            Action<DendrogramNode> getLeaves = delegate(DendrogramNode d)
            {
                if (d.IsLeaf)
                    order[pos++] = d.Id;
            };

            HierarchicalClusteringBase.TraversePreOrder(this.Root, getLeaves);
            return order;
        }        

        protected virtual int[] GetLeaves(DendrogramNode node)
        {            
            HashSet<int> leaves = new HashSet<int>();

            Action<DendrogramNode> getLeaves = delegate(DendrogramNode d)
            {
                if (d.IsLeaf)
                    leaves.Add(node.Id);
            };

            HierarchicalClusteringBase.TraversePreOrder(this.Root, getLeaves);
            return leaves.ToArray();
        }        

        protected abstract double GetClusterDistance(int[] cluster1, int[] cluster2);

        /// <summary>
        /// Traverse tree in level order and perform Action for every tree node (aka Breadth-first search (BFS))
        /// </summary>
        /// <param name="node"></param>
        /// <param name="action"></param>
        public static void TraverseLevelOrder(DendrogramNode node, Action<DendrogramNode> action)
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

        public static void TraversePreOrder(DendrogramNode node, Action<DendrogramNode> action)
        {
            if (node == null)
                return;

            action.Invoke(node);
            HierarchicalClusteringBase.TraversePreOrder(node.LeftNode, action);
            HierarchicalClusteringBase.TraversePreOrder(node.RightNode, action);
        }

        public static void TraverseInOrder(DendrogramNode node, Action<DendrogramNode> action)
        {
            if (node == null)
                return;
            
            HierarchicalClusteringBase.TraversePreOrder(node.LeftNode, action);
            action.Invoke(node);
            HierarchicalClusteringBase.TraversePreOrder(node.RightNode, action);
        }

        public static void TraversePostOrder(DendrogramNode node, Action<DendrogramNode> action)
        {
            if (node == null)
                return;

            HierarchicalClusteringBase.TraversePreOrder(node.LeftNode, action);
            HierarchicalClusteringBase.TraversePreOrder(node.RightNode, action);
            action.Invoke(node);
        }

        public static void TraverseEulerPath(DendrogramNode node, Action<DendrogramNode> action)
        {
            if (node == null)
                return;

            action.Invoke(node);

            if (node.LeftNode != null)
            {
                HierarchicalClusteringBase.TraverseEulerPath(node.LeftNode, action);
                action.Invoke(node);
            }

            if (node.RightNode != null)
            {
                HierarchicalClusteringBase.TraverseEulerPath(node.RightNode, action);
                action.Invoke(node);
            }
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
                return;

            if (node.IsLeaf)
            {
                result[node.Id] = clusterId;
            }
            else
            {
                SetClusterIdRecursive(clusterId, ref result, node.LeftNode);
                SetClusterIdRecursive(clusterId, ref result, node.RightNode);
            }
        }        

        /// <summary>
        /// <par>Calculates average distance between nodes.</par>
        /// <par>D(node1, node2) = D(node1, root) + D(node2, root) - 2*D(LCA(node1, node2), root)</par>
        /// <par>where D(a, root) is number of levels between node a and tree root and LCA is lowest common ancestor</par>
        /// </summary>
        /// <returns>Average distance</returns>
        public virtual double GetAvgNodeLevelDistance()
        {            
            if (this.isLeafDistanceCalculated == false)
                this.CalculateLeavesDistance();

            double ret = 0.0;
            foreach (KeyValuePair<Tuple<int, int>, int> kvp in this.lca)
                ret += this.GetLeafDistance(kvp.Key.Item1, kvp.Key.Item2);                        
            return ret / lca.Count;
        }

        protected virtual void CalculateLeavesDistance()
        {                                                
            this.CalcLCA();            
            this.isLeafDistanceCalculated = true;
        }

        public virtual void CalcLCA()
        {
            int i = 0, j, nodeIdx, l = 0;
            int[] leaves = new int[this.NumberOfInstances];
            int[] nodeIds = new int[2 * this.NumberOfInstances - 1];
            Dictionary<int, int> nodeId2Idx = new Dictionary<int, int>(nodeIds.Length);
            this.node2RootDistance = new Dictionary<int, int>();
            Action<DendrogramNode> node2Lookup = delegate(DendrogramNode d)
            {
                nodeIds[i] = d.Id;
                nodeId2Idx[d.Id] = i;
                i++;
                if (d.IsLeaf)
                    leaves[l++] = d.Id;

                if (d.IsRoot == true)
                    d.Level = 0;
                else
                    d.Level = d.Parent.Level + 1;
                this.node2RootDistance[d.Id] = d.Level;
            };
            HierarchicalClusteringBase.TraversePreOrder(this.Root, node2Lookup);

            List<int> eulerPath = new List<int>();
            Dictionary<int, int> firstVisit = new Dictionary<int, int>();
            i = 0;
            Action<DendrogramNode> node2Array = delegate(DendrogramNode d)
            {
                eulerPath.Add(nodeId2Idx[d.Id]);
                if (!firstVisit.TryGetValue(d.Id, out nodeIdx))
                {
                    firstVisit[d.Id] = i;                    
                }
                i++;
            };
            HierarchicalClusteringBase.TraverseEulerPath(this.Root, node2Array);

            this.lca = new Dictionary<Tuple<int, int>, int>();
            int indexX, indexY, temp;            
            
            for (i = 0; i < leaves.Length; i++)
            {
                indexX = firstVisit[leaves[i]];
                for (j = i + 1; j < leaves.Length; j++)
                {
                    indexY = firstVisit[leaves[j]];

                    if (indexY < indexX)
                    {
                        temp = indexX;
                        indexX = indexY;
                        indexY = temp;
                    }

                    temp = Int32.MaxValue;
                    for (int m = indexX; m < indexY; m++)
                        if (eulerPath[m] < temp)
                            temp = eulerPath[m];

                    this.lca.Add(leaves[i] <= leaves[j] 
                                    ? new Tuple<int, int>(leaves[i], leaves[j]) 
                                    : new Tuple<int, int>(leaves[j], leaves[i]), 
                                 nodeIds[temp]);
                }
            }
        }

        /// <summary>
        /// https://sites.google.com/site/indy256/algo/sparse_table_rmq
        /// https://sites.google.com/site/indy256/algo/sparse_table_lca
        /// 
        /// </summary>
        public virtual void CalcLCASparseTable()
        {
            
            //TODO

            
            int i = 0, j, nodeIdx, l = 0;
            int[] leaves = new int[this.NumberOfInstances];
            int[] nodeIds = new int[2 * this.NumberOfInstances - 1];
            Dictionary<int, int> nodeId2Idx = new Dictionary<int, int>(nodeIds.Length);
            this.node2RootDistance = new Dictionary<int, int>();
            Action<DendrogramNode> node2Lookup = delegate(DendrogramNode d)
            {
                nodeIds[i] = d.Id;
                nodeId2Idx[d.Id] = i;
                i++;
                if (d.IsLeaf)
                    leaves[l++] = d.Id;

                if (d.IsRoot == true)
                    d.Level = 0;
                else
                    d.Level = d.Parent.Level + 1;
                this.node2RootDistance[d.Id] = d.Level;
            };
            HierarchicalClusteringBase.TraversePreOrder(this.Root, node2Lookup);

            List<int> eulerPath = new List<int>();
            Dictionary<int, int> firstVisit = new Dictionary<int, int>();
            i = 0;
            Action<DendrogramNode> node2Array = delegate(DendrogramNode d)
            {
                eulerPath.Add(nodeId2Idx[d.Id]);
                if (!firstVisit.TryGetValue(d.Id, out nodeIdx))
                {
                    firstVisit[d.Id] = i;
                }
                i++;
            };
            HierarchicalClusteringBase.TraverseEulerPath(this.Root, node2Array);

            this.lca = new Dictionary<Tuple<int, int>, int>();
            
            int[] logTable = new int[eulerPath.Count + 1];
            for (i = 2; i <= eulerPath.Count; i++)
                logTable[i] = logTable[i >> 1] + 1;

            int[][] rmq = new int[logTable[eulerPath.Count] + 1][];
            for (i = 0; i < rmq.Length; i++)
                rmq[i] = new int[eulerPath.Count];

            for (i = 0; i < eulerPath.Count; ++i)
                rmq[0][i] = i;

            for (int k = 1; (1 << k) < eulerPath.Count; ++k)
            {
                for (i = 0; i + (1 << k) <= eulerPath.Count; i++)
                {
                    int x = rmq[k - 1][i];
                    int y = rmq[k - 1][i + (1 << k - 1)];
                    rmq[k][i] = eulerPath[x] <= eulerPath[y] ? x : y;
                }
            }

            for (i = 0; i < leaves.Length; i++)
            {
                for (j = i + 1; j < leaves.Length; j++)
                {
                    int k = logTable[leaves[j] - leaves[i]];
                    int x = rmq[k][leaves[i]];
                    int y = rmq[k][leaves[j] - (1 << k) + 1];
                    
                    this.lca.Add(leaves[i] <= leaves[j] 
                                    ? new Tuple<int, int>(leaves[i], leaves[j]) 
                                    : new Tuple<int, int>(leaves[j], leaves[i]), 
                                 eulerPath[x] <= eulerPath[y] ? x : y);                    
                }
            }
            
        }                 

        public virtual double GetLeafDistance(int node1, int node2)
        {

            if (this.isLeafDistanceCalculated == false)
                this.CalculateLeavesDistance();
            
            int dist1 = this.node2RootDistance[node1];
            int dist2 = this.node2RootDistance[node2];
            int lcsDist = this.node2RootDistance[this.lca[node1 <= node2 
                                                             ? new Tuple<int, int>(node1, node2) 
                                                             : new Tuple<int, int>(node2, node1)]];
            return dist1 + dist2 - (2 * lcsDist);
        }

        protected abstract DendrogramNode FindNode(int nodeId);
        public abstract void Compute();
    }
}
