using Infovision.Core;
using Infovision.MachineLearning.Roughset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Classification.DecisionTrees
{
    public static class DecisionTreeAttributeSelection
    {
        public static int[] ApproximateReductAttributeSelection(LocalDataStoreSlot data, int[] attributes)
        {
            Args parms = new Args(4);
            parms.SetParameter(ReductGeneratorParamHelper.TrainData, data);
            parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajority);
            parms.SetParameter(ReductGeneratorParamHelper.Epsilon, 0.01 );
            parms.SetParameter(ReductGeneratorParamHelper.NumberOfReducts, 100);

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
