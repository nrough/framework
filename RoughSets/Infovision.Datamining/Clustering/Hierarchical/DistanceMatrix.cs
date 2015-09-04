using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infovision.Datamining.Clustering.Hierarchical
{
    public interface IDistanceMatrix
    {
        Func<double[], double[], double> Distance
        {
            get;
            set;
        }
    }
    
    
    public class DistanceMatrix
    {
        private Dictionary<MatrixKey, double> matrix = new Dictionary<MatrixKey, double>();
        private Func<double[], double[], double> distance;

        public DistanceMatrix(Func<double[], double[], double> distance)
        {
            this.Distance = distance;
        }
                
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

        public void Initialize(double[][] points)
        {
            //calculate initial distance matrix
            for (int i = 0; i < points.Length; i++)
            {
                for (int j = i + 1; j < points.Length; j++)
                {
                    MatrixKey key = new MatrixKey(i, j);
                    matrix.Add(key, this.Distance(points[i], points[j]));
                }
            }   
        }

        public MatrixKey FindMinimumDistance()
        {
            int[] result = new int[2] { -1, -1 };
            double minDistance = Double.MaxValue;

            foreach(KeyValuePair<MatrixKey, double> kvp in matrix)
            {                
                if (kvp.Value < minDistance)
                {
                    result[0] = kvp.Key.X;
                    result[1] = kvp.Key.Y;
                    minDistance = kvp.Value;
                }
            }

            Console.WriteLine("Smallest distance is {0}. Under key: {1}-{2}", minDistance,  result[0],  result[1]);

            return new MatrixKey(result[0], result[1]);
        }

        public void CalculateDistanceMatrix(AgnesCluster mergedCluster1, AgnesCluster mergedCluster2, AgnesCluster newCluster)
        {
            CalculateDistanceMatrix(mergedCluster1.Index, mergedCluster2.Index, newCluster.Index);                        
        }

        public void CalculateDistanceMatrix(int mergedCluster1, int mergedCluster2, int newCluster)
        {
            Dictionary<MatrixKey, double> newMatrix = new Dictionary<MatrixKey, double>(matrix.Count);

            foreach (KeyValuePair<MatrixKey, double> kvp in matrix)
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

                    var query = matrix.Where(x => (x.Key.X == mergedCluster1 || x.Key.X == mergedCluster2)
                                                    && x.Key.Y == kvp.Key.Y);

                    foreach (KeyValuePair<MatrixKey, double> item in query)
                    {
                        Console.WriteLine("{0} Checking distance d({1}) = {2}", kvp.Key, item.Key, item.Value);
                    }

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

                    var query = matrix.Where(x => (x.Key.Y == mergedCluster1 || x.Key.Y == mergedCluster2)
                                                    && x.Key.X == kvp.Key.X);
                    
                    foreach (KeyValuePair<MatrixKey, double> item in query)
                    {
                        Console.WriteLine("{0} Checking distance d({1}) = {2}", kvp.Key, item.Key, item.Value);
                    }

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

            matrix = newMatrix;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<MatrixKey, double> kvp in matrix)
            {
                sb.AppendLine(String.Format("d({0}, {1}) = {2}", kvp.Key.X, kvp.Key.Y, kvp.Value));
            }
            return sb.ToString();
        }
    }
}
