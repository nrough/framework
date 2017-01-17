using Infovision.Core;
using Infovision.Data;
using Infovision.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Clustering
{
    /// <summary>
    /// https://visualstudiomagazine.com/Articles/2013/12/01/K-Means-Data-Clustering-Using-C.aspx?p=1
    /// </summary>
    [Serializable]
    public class KMeans : ModelBase
    {
        private double[][] means;
        private int[] attr;
        private int[] clusters;
        
        public int K { get; set; } = 4;
        public int Iter { get; set; } = 100;
        public int Retries { get; set; } = 1;
        public Func<double[], double[], double> Distance { get; set; } = Math.Distance.Euclidean;
        
        public KMeans() { }
        
        public ClusteringResult Learn(DataStore data, int[] attributes)
        {
            //check parameters
            if (data == null) throw new ArgumentNullException("data");
            if (attributes == null) throw new ArgumentNullException("attributes");
            if (attributes.Length < 1) throw new ArgumentException("attributes.Length < 1", "attributes");
            if (K < 1) throw new ArgumentOutOfRangeException("K", "this.K < 1");
            if (Retries < 1) throw new ArgumentOutOfRangeException("Retries", "this.Retries < 1");

            attr = new int[attributes.Length];
            Array.Copy(attributes, 0, attr, 0, attributes.Length);

            double[][] rawData = data.ToArray<double>(attributes);
            this.clusters = this.Cluster(rawData, this.K);

            ClusteringResult result = new ClusteringResult();
            result.TestData = data;
            return result;
        }

        public virtual long Compute(DataRecordInternal record)
        {
            //double[] vector = record.ToArray<double>(this.attr);
            
            return -1; //unknown
        }

        private int[] Cluster(double[][] rawData, int numClusters)
        {
            double[][] data = this.Normalized(rawData);
            bool changed = true; bool success = true;
            int[] clustering = this.InitClustering(data.Length, numClusters, 0);
            this.means = this.Allocate(numClusters, data[0].Length);
            int maxCount = data.Length * 10;

            int ct = 0;
            while (changed == true && success == true && ct < maxCount)
            {
                ct++;
                success = this.UpdateMeans(data, clustering, means);
                changed = this.UpdateClustering(data, clustering, means);
            }
            return clustering;
        }

        private double[][] Normalized(double[][] rawData)
        {
            double[][] result = new double[rawData.Length][];
            for (int i = 0; i < rawData.Length; i++)
            {
                result[i] = new double[rawData[i].Length];
                Array.Copy(rawData[i], result[i], rawData[i].Length);
            }

            for (int j = 0; j < result[0].Length; j++)
            {
                double colSum = 0.0;
                for (int i = 0; i < result.Length; i++)
                    colSum += result[i][j];
                double mean = colSum / result.Length;
                double sum = 0.0;
                for (int i = 0; i < result.Length; i++)
                    sum += (result[i][j] - mean) * (result[i][j] - mean);
                double sd = sum / result.Length;
                for (int i = 0; i < result.Length; i++)
                    result[i][j] = (result[i][j] - mean) / sd;
            }

            return result;
        }

        private int[] InitClustering(int numTuples, int numClusters, int seed)
        {            
            int[] clustering = new int[numTuples];
            for (int i = 0; i < numClusters; i++)
                clustering[i] = i;
            for (int i = numClusters; i < clustering.Length; i++)
                clustering[i] = RandomSingleton.Random.Next(0, numClusters);
            return clustering;
        }

        private double[][] Allocate(int numClusters, int numColumns)
        {
            double[][] result = new double[numClusters][];
            for (int k = 0; k < numClusters; k++)
                result[k] = new double[numColumns];
            return result;
        }

        private bool UpdateMeans(double[][] data, int[] clustering, double[][] means)
        {
            int numClusters = means.Length;
            int[] clusterCounts = new int[numClusters];
            for (int i = 0; i < data.Length; i++)
                clusterCounts[clustering[i]]++;

            for (int k = 0; k < numClusters; k++)
                if (clusterCounts[k] == 0)
                    return false;

            means.SetAll(0.0);
            
            for (int i = 0; i < data.Length; i++)                
                for (int j = 0; j < data[i].Length; j++)
                    means[clustering[i]][j] += data[i][j]; // accumulate sum

            for (int k = 0; k < means.Length; k++)
                for (int j = 0; j < means[k].Length; j++)
                    means[k][j] /= clusterCounts[k]; // danger of div by 0

            return true;
        }

        private bool UpdateClustering(double[][] data, int[] clustering, double[][] means)
        {
            int numClusters = means.Length;
            bool changed = false;

            int[] newClustering = new int[clustering.Length];
            Array.Copy(clustering, newClustering, clustering.Length);

            double[] distances = new double[numClusters];

            Parallel.For(0, data.Length, i =>
            {
                for (int k = 0; k < numClusters; k++)
                    distances[k] = this.Distance(data[i], means[k]);

                int newClusterID = MinIndex(distances);
                if (newClusterID != newClustering[i])
                {
                    changed = true;
                    newClustering[i] = newClusterID;
                }
            });

            if (changed == false)
                return false;

            int[] clusterCounts = new int[numClusters];
            for (int i = 0; i < data.Length; i++)
            {
                int cluster = newClustering[i];
                clusterCounts[cluster]++;
            }

            for (int k = 0; k < numClusters; k++)
                if (clusterCounts[k] == 0)
                    return false;

            Array.Copy(newClustering, clustering, newClustering.Length);
            return true; // no zero-counts and at least one change
        }        

        private int MinIndex(double[] distances)
        {
            int indexOfMin = 0;
            double smallDist = distances[0];
            for (int k = 1; k < distances.Length; k++)
            {
                if (distances[k] < smallDist)
                {
                    smallDist = distances[k];
                    indexOfMin = k;
                }
            }
            return indexOfMin;
        }

        private void ShowData(double[][] data, int decimals, bool indices, bool newLine)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (indices) Console.Write(i.ToString().PadLeft(3) + " ");
                for (int j = 0; j < data[i].Length; j++)
                {
                    if (data[i][j] >= 0.0) Console.Write(" ");
                    Console.Write(data[i][j].ToString("F" + decimals) + " ");
                }
                Console.WriteLine("");
            }
            if (newLine) Console.WriteLine("");
        }

        private void ShowVector(int[] vector, bool newLine)
        {
            for (int i = 0; i < vector.Length; i++)
                Console.Write(vector[i] + " ");
            if (newLine) Console.WriteLine("\n");
        }

        private void ShowClustered(double[][] data, int[] clustering, int numClusters, int decimals)
        {
            for (int k = 0; k < numClusters; k++)
            {
                Console.WriteLine("===================");
                for (int i = 0; i < data.Length; i++)
                {
                    int clusterID = clustering[i];
                    if (clusterID != k) continue;
                    Console.Write(i.ToString().PadLeft(3) + " ");
                    for (int j = 0; j < data[i].Length; j++)
                    {
                        if (data[i][j] >= 0.0) Console.Write(" ");
                        Console.Write(data[i][j].ToString("F" + decimals) + " ");
                    }
                    Console.WriteLine("");
                }
                Console.WriteLine("===================");
            } // k
        }        
    }
}
