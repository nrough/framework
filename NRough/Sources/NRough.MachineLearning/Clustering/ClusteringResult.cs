using NRough.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Clustering
{
    [Serializable]
    public class ClusteringResult
    {
        public double Error { get; set; }
        public long[] Labels { get; set; }
        public double[] ClusterReps { get; set; }
        public long Result { get; set; }
        public DataStore TestData { get; set; }
    }
}
