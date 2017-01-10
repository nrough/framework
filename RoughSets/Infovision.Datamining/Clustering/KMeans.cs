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
    [Serializable]
    public class KMeans : ModelBase
    {
        private List<long[]> centroids;
        private int[] attributes;
        private int[] clusters;
        
        public int K { get; set; } = 4;
        public int Iter { get; set; } = 100;
        public Func<double[], double[], double> Distance { get; set; } = Similarity.Euclidean;
        
        public KMeans() { }

        public void Learn(DataStore data, int[] attributes)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (attributes == null) throw new ArgumentNullException("attributes");
            if (attributes.Length < 1) throw new ArgumentException("attributes.Length < 1", "attributes");
            if (this.K < 1) throw new InvalidOperationException();

            this.clusters = new int[data.NumberOfRecords];
            for (int i = 0; i < this.K; i++)
                this.clusters[i] = RandomSingleton.Random.Next(0, this.K);

            int[] clusterCount = new int[this.K];
            for (int i = 0; i < this.clusters.Length; i++)
                clusterCount[this.clusters[i]]++;

            double[][] means = new double[this.K][];
            for (int i = 0; i < this.K; i++)
                means[i] = new double[attributes.Length];

            for (int i = 0; i < this.clusters.Length; i++)
                for (int j = 0; j < attributes.Length; j++)
                    means[this.clusters[i]][j] += data.GetFieldValue(i, attributes[j]);

            for (int i = 0; i < means.Length; i++)
                for (int j = 0; j < attributes.Length; j++)
                    means[i][j] /= clusterCount[i];

            //https://visualstudiomagazine.com/Articles/2013/12/01/K-Means-Data-Clustering-Using-C.aspx?admgarea=features&p=1

        }

        public virtual long Compute(DataRecordInternal record)
        {
            return -1; //unknown
        }
    }
}
