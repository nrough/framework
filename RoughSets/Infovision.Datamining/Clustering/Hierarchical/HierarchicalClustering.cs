using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Accord.MachineLearning;
using AForge;

namespace Infovision.Datamining.Clustering.Hierarchical
{
    [Serializable]
    public class HierarchicalClustering
    {        
        protected DistanceMatrix distanceMatrix;
        protected Dictionary<int, HierarchicalCluster> clusters;
        
        private int nextClusterId = 0;        
        private Func<double[], double[], double> distance;

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

        protected int NextClusterId
        {
            get { return this.nextClusterId; }
        }

        /// <summary>
        ///   Initializes a new instance of the HierarchicalClustering algorithm
        /// </summary>        
        public HierarchicalClustering()
            : this(Accord.Math.Distance.SquareEuclidean) { }

        /// <summary>
        ///   Initializes a new instance of the HierarchicalClustering algorithm
        /// </summary>
        ///         
        /// <param name="distance">The distance function to use. Default is to
        /// use the <see cref="Accord.Math.Distance.SquareEuclidean(double[], double[])"/> distance.</param>
        /// 
        public HierarchicalClustering(Func<double[], double[], double> distance)
        {
            if (distance == null)
                throw new ArgumentNullException("distance");

            this.Distance = distance;
        }

        private void Initialize(double[][] points)
        {
            if (points == null)
                throw new ArgumentNullException("points");

            this.CalculateDistanceMatrix(points);
            this.InitClusters(points);            
        }
        
        private void CalculateDistanceMatrix(double[][] points)
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

            // Perform initialization of the clusters
            Initialize(data);

            Console.Write(distanceMatrix.ToString());

            CreateClusters();
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
            while (clusters.Count > 1)
            {
                MatrixKey key = this.GetClustersToMerge();                
                HierarchicalCluster mergedCluster = HierarchicalCluster.MergeClusters(nextClusterId, clusters[key.X], clusters[key.Y]);

                this.RemoveCluster(key.X);
                this.RemoveCluster(key.Y);
                this.AddCluster(mergedCluster);

                distanceMatrix.CalculateDistanceMatrix(key.X, key.Y, mergedCluster.Index);                
            }
        }

        protected virtual MatrixKey GetClustersToMerge()
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

            return new MatrixKey(result[0], result[1]);
        }
    }
}
