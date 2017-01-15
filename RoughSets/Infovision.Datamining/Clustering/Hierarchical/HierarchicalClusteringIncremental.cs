using System;
using System.Collections.Generic;
using System.Linq;
using Infovision.Math;

namespace Infovision.MachineLearning.Clustering.Hierarchical
{
    [Serializable]
    public abstract class HierarchicalClusteringIncremental : HierarchicalClusteringBase
    {
        #region TODO

        //TODO HierarchicalClusteringIncremental.AddToCluster method fails.
        //Parent methods works OK
        //Derived implementation like in SIHC also works fine.
        //Check the difference between this and SIHC implementation
        //It is possible that this implementation is the old one that was not updated at all and instead only SIHC should be used

        #endregion

        private Dictionary<int, DendrogramNode> nodes;
        public int MinimumNumberOfInstances { get; set; }

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
            throw new InvalidOperationException("Do not use Compute() method in incremental clustering. Use AddToCluster instead.");
        }
        
        //public override bool AddToCluster(int id, double[] instance)
        public bool AddToCluster_OLD(int id, double[] instance)
        {
            bool ret = base.AddToCluster(id, instance);

            if (ret == false)
                return false;

            double distance = 0.0;
            double minDistance = double.MaxValue;
            int minLeafKey = -1;
            foreach (KeyValuePair<int, double[]> kvp in this.Instances)
            {
                distance = this.Distance(kvp.Value, instance);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    minLeafKey = kvp.Key;
                }
                this.DistanceMatrix.Add(new MatrixKey(kvp.Key, id), distance);
            }
            this.AddInstance(id, instance);

            //when we reach the minimum number of instances, we create dendrogram using all added instances
            if (this.NumberOfInstances >= this.MinimumNumberOfInstances)
            {
                //this will be executed only once at the beginning
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
                    else //in case of only one instance create root node. The leaf is added later to this root
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
                //add instance to previously created root node
                DendrogramNode newNode = new DendrogramNode
                {
                    Id = id
                };

                newNode.Parent = this.Root;
                this.Root.LeftNode = newNode;

                nodes.Add(newNode.Id, newNode);
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
                distance = this.Distance(instance, this.GetInstance(this.Root.LeftNode.Id));

                this.Root.Height = distance;
                this.Root.LeftLength = distance;
                this.Root.RightLength = distance;

                nodes.Add(id, newNode);

                return true;
            }

            //TODO insert new leaf next to other closest leaf (create new parent, add leaves as their children)

            DendrogramNode siblingLeaf = null;
            if (this.Nodes.TryGetValue(minLeafKey, out siblingLeaf))
            {
                //find sibling parent
                DendrogramNode parentNode = siblingLeaf.Parent;

                DendrogramNode newParentNode = new DendrogramNode
                {
                    Id = this.NextClusterId,
                    Parent = siblingLeaf.Parent,
                    Height = minDistance,
                    LeftNode = siblingLeaf,
                    LeftLength = minDistance,
                    RightNode = null,
                    RightLength = minDistance
                };

                //update parent references
                if (parentNode.LeftNode.Id == siblingLeaf.Id)
                    parentNode.LeftNode = newParentNode;
                else
                    parentNode.RightNode = newParentNode;

                //attach to new parent
                siblingLeaf.Parent = newParentNode;

                //create new leaf node
                DendrogramNode newLeafNode = new DendrogramNode
                {
                    Id = id,
                    Parent = newParentNode,
                    Height = 0.0,
                    LeftNode = null,
                    LeftLength = 0.0,
                    RightNode = null,
                    RightLength = 0.0
                };

                //attach to parent
                newParentNode.RightNode = newLeafNode;
            }
            else
            {
                throw new InvalidOperationException(String.Format("Cannot find leaf node with Id {0}", minLeafKey));
            }

            return true;
        }

        public abstract override bool AddToCluster(int id, double[] instance);
        

        public override double GetClusterDistance(int[] cluster1, int[] cluster2)
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