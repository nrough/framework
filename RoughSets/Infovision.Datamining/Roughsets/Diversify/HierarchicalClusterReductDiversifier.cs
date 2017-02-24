using NRough.Data;
using NRough.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Roughsets.Diversify
{
    [Serializable]
    public class HierarchicalClusterReductDiversify : ReductDiversifyBase
    {
        public DataStore DecisionTable { get; set; }
        public int NumberOfReducts { get; set; }
        public Func<double[], double[], double> Distance;
        public Func<int[], int[], DistanceMatrix, double[][], double> Linkage;
        public Func<IReduct, double[], RuleQualityMethod, double[]> ReductToVectorMethod;

        public HierarchicalClusterReductDiversify()
            : base()
        {
        }


    }
}
