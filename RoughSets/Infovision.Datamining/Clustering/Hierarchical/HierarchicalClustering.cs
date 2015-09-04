using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Accord.MachineLearning;
using AForge;
using Infovision.Math;

namespace Infovision.Datamining.Clustering.Hierarchical
{
    [Serializable]
    public class HierarchicalClustering
    {        
        protected DistanceMatrix distanceMatrix;
        protected Dictionary<int, HierarchicalCluster> clusters;
        
        private int nextClusterId = 0;
        
        private Func<double[], double[], double> distance;
        private Func<int[], int[], DistanceMatrix, double> linkage;

        private DendrogramLinkCollection dendrogram;       
        private int numOfInstances;

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

        public Func<int[], int[], DistanceMatrix, double> Linkage
        {
            get { return this.linkage; }
            set { this.linkage = value; }
        }

        protected int NextClusterId
        {
            get { return this.nextClusterId; }
        }

        public DendrogramLinkCollection DendrogramLinkCollection
        {
            get { return this.dendrogram; }
        }

        /// <summary>
        ///   Initializes a new instance of the HierarchicalClustering algorithm
        /// </summary>        
        public HierarchicalClustering()
            : this(Accord.Math.Distance.SquareEuclidean, ClusteringLinkage.Min) { }

        /// <summary>
        ///   Initializes a new instance of the HierarchicalClustering algorithm
        /// </summary>
        ///         
        /// <param name="distance">The distance function to use. Default is to
        /// use the <see cref="Accord.Math.Distance.SquareEuclidean(double[], double[])"/> distance.</param>
        /// <param name="linkage">The linkage function to use. Default is to
        /// use the <see cref="ClusteringLinkage.Min(int[], int[], DistanceMatrix)"/> linkage.</param>
        /// 
        public HierarchicalClustering(Func<double[], double[], double> distance, 
                                      Func<int[], int[], DistanceMatrix, double> linkage)
        {
            if (distance == null)
                throw new ArgumentNullException("distance");

            if(linkage == null)
                throw new ArgumentNullException("linkage");

            this.Distance = distance;
            this.Linkage = linkage;

            dendrogram = new DendrogramLinkCollection();
        }

        private void Initialize(double[][] points)
        {
            if (points == null)
                throw new ArgumentNullException("points");

            numOfInstances = points.Length;            

            this.InitDistanceMatrix(points);
            this.InitClusters(points);            
        }
        
        private void InitDistanceMatrix(double[][] points)
        {
            distanceMatrix = new DistanceMatrix(this.Distance);
            distanceMatrix.Initialize(points);
        }

        private void InitClusters(double[][] points)
        {
            clusters = new Dictionary<int, HierarchicalCluster>(points.Length);
            for (int i = 0; i < points.Length; i++)
            {
                HierarchicalCluster cluster = new HierarchicalCluster(i);
                cluster.AddMemberObject(i);
                clusters.Add(i, cluster);
            }
            nextClusterId = points.Length;
        }

        /// <summary>
        ///  Creates a hirarchy of clusters 
        /// </summary>
        /// 
        /// <param name="data">The data where to compute the algorithm.</param>        
        public virtual void Compute(double[][] data)
        {
            // Initial argument checking
            if (data == null)
                throw new ArgumentNullException("data");

            if (data.Length == 0)
                return;

            // Perform initialization of the clusters
            this.Initialize(data);

            Console.Write(distanceMatrix.ToString());

            this.CreateClusters();
        }
        
        protected void AddCluster(HierarchicalCluster cluster)
        {
            clusters.Add(cluster.Index, cluster);
            nextClusterId++;
        }

        protected void RemoveCluster(int key)
        {
            clusters.Remove(key);
        }

        protected virtual void CreateClusters()
        {
            Dictionary<int, DendrogramNode> nodeDictionary = new Dictionary<int,DendrogramNode>();
            
            while (clusters.Count > 1)
            {
                DendrogramLink link = this.GetClustersToMerge();
                int mergedClusterIdx = this.MergeClusters(link);

                if (link.Cluster1 < this.numOfInstances && link.Cluster2 < this.numOfInstances)
                {
                    DendrogramNode newNode = new DendrogramNode
                    {
                        NodeId = mergedClusterIdx,
                        Parent = null,
                        LeftNode = null,
                        LeftInstance = link.Cluster1 < link.Cluster2
                                     ? link.Cluster1
                                     : link.Cluster2,
                        LeftDistance = 0,
                        RightNode = null,
                        RightInstance = link.Cluster1 < link.Cluster2
                                     ? link.Cluster2
                                     : link.Cluster1,
                        RightDistance = 0
                    };

                    nodeDictionary.Add(mergedClusterIdx, newNode);
                }
                else if (link.Cluster1 < this.numOfInstances && link.Cluster2 >= this.numOfInstances)
                {                    
                    DendrogramNode rightNode = nodeDictionary[link.Cluster2];
                    DendrogramNode newNode = new DendrogramNode
                    {
                        NodeId = mergedClusterIdx,
                        Parent = null,
                        
                        LeftNode = null,
                        LeftInstance = link.Cluster1,
                        LeftDistance = 0,
                        
                        RightNode = rightNode,
                        RightInstance = link.Cluster2,
                        RightDistance = 0
                    };

                    rightNode.Parent = newNode;

                    nodeDictionary.Add(mergedClusterIdx, newNode);
                }
                else if (link.Cluster1 >= this.numOfInstances && link.Cluster2 < this.numOfInstances)
                {
                    DendrogramNode leftNode = nodeDictionary[link.Cluster1];
                    DendrogramNode newNode = new DendrogramNode
                    {
                        NodeId = mergedClusterIdx,
                        Parent = null,

                        LeftNode = leftNode,
                        LeftInstance = link.Cluster1,
                        LeftDistance = 0,

                        RightNode = null,
                        RightInstance = link.Cluster2,
                        RightDistance = 0
                    };

                    leftNode.Parent = newNode;

                    nodeDictionary.Add(mergedClusterIdx, newNode);
                }
                else //both clusters are previously merged clusters
                {
                    DendrogramNode leftNode = nodeDictionary[link.Cluster1];
                    DendrogramNode rightNode = nodeDictionary[link.Cluster2];

                    DendrogramNode newNode = new DendrogramNode
                    {
                        NodeId = mergedClusterIdx,
                        Parent = null,

                        LeftNode = leftNode,
                        LeftInstance = link.Cluster1,
                        LeftDistance = 0,

                        RightNode = rightNode,
                        RightInstance = link.Cluster2,
                        RightDistance = 0
                    };

                    leftNode.Parent = newNode;
                    rightNode.Parent = newNode;

                    nodeDictionary.Add(mergedClusterIdx, newNode);
                }


                this.CalculateDistanceMatrix(link.Cluster1, link.Cluster2, mergedClusterIdx);
            }
        }        

        protected virtual DendrogramLink GetClustersToMerge()
        {
            int[] result = new int[2] { -1, -1 };
            double minDistance = Double.MaxValue;

            foreach (KeyValuePair<MatrixKey, double> kvp in this.distanceMatrix.ReadOnlyMatrix)
            {
                if (kvp.Value < minDistance)
                {
                    result[0] = kvp.Key.X;
                    result[1] = kvp.Key.Y;
                    minDistance = kvp.Value;
                }
            }

            return new DendrogramLink(result[0], result[1], minDistance);
        }

        protected int MergeClusters(int x, int y, double distance)
        {
            HierarchicalCluster mergedCluster = HierarchicalCluster.MergeClusters(nextClusterId, clusters[x], clusters[y]);

            this.RemoveCluster(x);
            this.RemoveCluster(y);
            this.AddCluster(mergedCluster);
            this.dendrogram.Add(x, y, distance);
            return mergedCluster.Index;
        }

        protected int MergeClusters(MatrixKey key, double distance)
        {
            return this.MergeClusters(key.X, key.Y, distance);                      
        }

        protected int MergeClusters(DendrogramLink link)
        {
            return this.MergeClusters(link.Cluster1, link.Cluster2, link.Distance);
        }

        private void CalculateDistanceMatrix(HierarchicalCluster mergedCluster1, HierarchicalCluster mergedCluster2, HierarchicalCluster newCluster)
        {
            CalculateDistanceMatrix(mergedCluster1.Index, mergedCluster2.Index, newCluster.Index);
        }

        private void CalculateDistanceMatrix(int mergedCluster1, int mergedCluster2, int newCluster)
        {            
            DistanceMatrix newMatrix = new DistanceMatrix(distanceMatrix.Distance);

            foreach (KeyValuePair<MatrixKey, double> kvp in distanceMatrix.ReadOnlyMatrix)
            {
                // If the cluster IDs in the key are both the same as the recently merged clusters, skip it. This value
                // won't be in the new table.
                if ((kvp.Key.X == mergedCluster1 && kvp.Key.Y == mergedCluster2)
                    || (kvp.Key.Y == mergedCluster2 && kvp.Key.X == mergedCluster1))
                {
                    // don't need this value anymore, since these clusters are merged into clNew
                    continue;
                }
                // If one of the cluster IDs in the key match with one of the recently merged clusters, the use
                // this value to calculate the distances of the new cluster.
                else if (kvp.Key.X == mergedCluster1 || kvp.Key.X == mergedCluster2)
                {
                    MatrixKey newKey = new MatrixKey(newCluster, kvp.Key.Y);

                    var query = distanceMatrix.ReadOnlyMatrix.Where(x => (x.Key.X == mergedCluster1 || x.Key.X == mergedCluster2)
                                                    && x.Key.Y == kvp.Key.Y);

                    //TODO To be substituted by linkage delegate
                    var newDistance = query.Aggregate(query.First(), (min, curr) => curr.Value < min.Value ? curr : min);

                    if (newMatrix.ContainsKey(newKey))
                    {
                        newMatrix[newKey] = newDistance.Value;
                    }
                    else
                    {
                        newMatrix.Add(newKey, newDistance.Value);
                    }

                }
                // This block is same as above. Only checking the other cluster ID. This eliminates the dependency of the order
                // of the IDs in the key.
                else if (kvp.Key.Y == mergedCluster1 || kvp.Key.Y == mergedCluster2)
                {
                    MatrixKey newKey = new MatrixKey(newCluster, kvp.Key.X);

                    var query = distanceMatrix.ReadOnlyMatrix.Where(x => (x.Key.Y == mergedCluster1 || x.Key.Y == mergedCluster2)
                                                    && x.Key.X == kvp.Key.X);

                    //TODO To be substituted by linkage delegate
                    var newDistance = query.Aggregate(query.First(), (min, curr) => curr.Value < min.Value ? curr : min);

                    if (newMatrix.ContainsKey(newKey))
                    {
                        newMatrix[newKey] = newDistance.Value;
                    }
                    else
                    {
                        newMatrix.Add(newKey, newDistance.Value);
                    }
                }
                // If the IDs both don't match with the current key, then this distance is not relevant to
                // the recently merged clusters. So we can use the old value as is.
                else
                {
                    newMatrix.Add(kvp.Key, kvp.Value);
                }

            }

            distanceMatrix = newMatrix;

        }
    }
}
