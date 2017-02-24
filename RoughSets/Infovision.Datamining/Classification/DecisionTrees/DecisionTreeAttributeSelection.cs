using NRough.Core;
using NRough.MachineLearning.Roughsets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Classification.DecisionTrees
{
    public static class DecisionTreeAttributeSelection
    {
        public static int[] ApproximateReductAttributeSelection(LocalDataStoreSlot data, int[] attributes)
        {
            Args parms = new Args(4);
            parms.SetParameter(ReductFactoryOptions.DecisionTable, data);
            parms.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductMajority);
            parms.SetParameter(ReductFactoryOptions.Epsilon, 0.01 );
            parms.SetParameter(ReductFactoryOptions.NumberOfReducts, 100);

            IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
            generator.Run(); 

            var reducts = generator.GetReducts();
            reducts.Sort(ReductAccuracyComparer.Default);
            IReduct bestReduct = reducts.FirstOrDefault();
             
            return bestReduct.Attributes.ToArray();
        }

        public static int[] ObliviousTreeAttributeRanking(LocalDataStoreSlot data, int[] attributes)
        {
            return attributes;
        }
    }
}
