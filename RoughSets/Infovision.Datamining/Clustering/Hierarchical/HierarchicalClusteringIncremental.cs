using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Math;

namespace Infovision.Datamining.Clustering.Hierarchical
{
    [Serializable]
    public class HierarchicalClusteringIncremental : HierarchicalClusteringBase
    {        
        private Dictionary<int, DendrogramNode> nodes;
        public int MinimumNumberOfInstances { get; set; }

        //TODO
        public Dictionary<int, DendrogramNode> Nodes
        {
            get { return this.nodes; }
            set { this.nodes = value; }
        }
        
        public HierarchicalClusteringIncremental()
            : base()
        {         
        }

        public HierarchicalClusteringIncremental(Func<double[], double[], double> distance,
                                                 Func<int[], int[], DistanceMatrix, double[][], double> linkage)
            : base(distance, linkage)
        {         
        }

        protected override void Init()
        {            
            nodes = new Dictionary<int, DendrogramNode>();
            this.MinimumNumberOfInstances = 3;

            base.Init();            
        }

        protected override void OnDistanceChanged(EventArgs e)
        {
            base.OnDistanceChanged(e);
            this.DistanceMatrix = new DistanceMatrix(this.Distance);
        }

        protected override void AddDendrogramNode(DendrogramNode node)
        {
            base.AddDendrogramNode(node);
            nodes.Add(node.Id, node);
        }

        /// <summary>
        ///  Creates a hierarchy of clusters 
        /// </summary>
        /// 
        /// <param name="data">The data where to compute the algorithm.</param>        
        public override void Compute()
        {            
            foreach(KeyValuePair<int, double[]> kvp in this.Instances)            
            {
                this.AddToCluster(kvp.Key, kvp.Value);
            }
        }

        public virtual void AddToCluster(int id, double[] instance)
        {            
            foreach (KeyValuePair<int, double[]> kvp in this.Instances)
                this.DistanceMatrix.Add(new MatrixKey(kvp.Key, id), this.Distance(kvp.Value, instance));
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
                                                
                        return;
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
                return;
            }
            
            
            //special case no nodes except root node
            if (this.NumberOfInstances == 0)
            {
                DendrogramNode newNode = new DendrogramNode
                {
                    Id = id
                };

                newNode.Parent = this.Root;
                this.Root.LeftNode = newNode;
                
                nodes.Add(newNode.Id, newNode);
                return;
            }
            //only one node (on the left) existing 
            else if (this.NumberOfInstances == 1)
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
                
                nodes.Add(id, newNode);

                return;
            }
            
            this.SIHC(id, this.Root);
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

                nodes.Add(singleton.Id, singleton);

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
                

                nodes.Add(newParentNode.Id, newParentNode);

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

        protected override double GetClusterDistance(int[] cluster1, int[] cluster2)
        {
            if (cluster1.Length == 1 && cluster2.Length == 1)
            {
                if (this.DistanceMatrix.ContainsKey(new MatrixKey(cluster1[0], cluster1[0])))
                    return this.DistanceMatrix.GetDistance(cluster1[0], cluster1[0]);
                else
                    return this.Distance(this.Instances[cluster1[0]], this.Instances[cluster2[0]]);
            }

            return this.Linkage(cluster1, cluster2, this.DistanceMatrix, this.Instances.Values.ToArray());
        }

        protected override DendrogramNode FindNode(int nodeId)
        {
            return this.nodes[nodeId];
        }
    }
}
