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
    public class Agnes
    {        
        private DistanceMatrix distanceMatrix;
        private int nextClusterId = 0;
        private Dictionary<int, AgnesCluster> clusters;
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

        /// <summary>
        ///   Initializes a new instance of the Agnes algorithm
        /// </summary>        
        public Agnes()
            : this(Accord.Math.Distance.SquareEuclidean) { }

        /// <summary>
        ///   Initializes a new instance of the Agnes algorithm
        /// </summary>
        ///         
        /// <param name="distance">The distance function to use. Default is to
        /// use the <see cref="Accord.Math.Distance.SquareEuclidean(double[], double[])"/> distance.</param>
        /// 
        public Agnes(Func<double[], double[], double> distance)
        {
            if (distance == null)
                throw new ArgumentNullException("distance");

            this.Distance = distance;
        }


        /// <summary>
        ///   Randomizes the clusters inside a dataset.
        /// </summary>
        /// 
        /// <param name="points">The data to randomize the algorithm.</param>
        /// <param name="useSeeding">True to use the k-means++ seeding algorithm. False otherwise.</param>
        /// 
        public void Initialize(double[][] points)
        {
            if (points == null)
                throw new ArgumentNullException("points");

            this.CalculateDistanceMatrix(points);
            this.CreateClusterCandidates(points);            
        }
        
        private void CalculateDistanceMatrix(double[][] points)
        {
            distanceMatrix = new DistanceMatrix(this.Distance);
            distanceMatrix.Initialize(points);
        }

        private void CreateClusterCandidates(double[][] points)
        {
            clusters = new Dictionary<int, AgnesCluster>(points.Length);
            for (int i = 0; i < points.Length; i++)
            {
                clusters.Add(i, new AgnesCluster(i));
            }
            nextClusterId = points.Length;
        }

        /// <summary>
        ///  Creates a hirarchy of clusters 
        /// </summary>
        /// 
        /// <param name="data">The data where to compute the algorithm.</param>        
        public void Compute(double[][] data)
        {
            // Initial argument checking
            if (data == null)
                throw new ArgumentNullException("data");           

            // Perform initialization of the clusters
            Initialize(data);
            Console.Write(distanceMatrix.ToString());

            while(clusters.Count > 1)
            {                
                MatrixKey key = this.distanceMatrix.FindMinimumDistance();                                             

                Console.WriteLine("Clusters to merge: {0}, {1}", key.X, key.Y);

                AgnesCluster mergedCluster = AgnesCluster.MergeClusters(nextClusterId, clusters[key.X], clusters[key.Y]);
                
                this.RemoveCluster(key.X);
                this.RemoveCluster(key.Y);
                this.AddCluster(mergedCluster);

                distanceMatrix.CalculateDistanceMatrix(key.X, key.Y, mergedCluster.Index);

                Console.Write(distanceMatrix.ToString());                                                
            }
        }
        
        private void AddCluster(AgnesCluster cluster)
        {
            clusters.Add(nextClusterId, cluster);
            nextClusterId++;
        }

        private void RemoveCluster(int key)
        {
            clusters.Remove(key);
        }
    }
}
