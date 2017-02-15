using NUnit.Framework;
using Raccoon.Core;
using Raccoon.Data;
using Raccoon.MachineLearning.Roughset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raccoon.MachineLearning.Tests.Roughset
{
    [TestFixture]
    public class CodeSamples
    {
        [Test]
        public void ApproximateDecisionReduct()
        {
            var data = Data.Benchmark.Factory.Golf();

            var parm = new Args();
            parm.SetParameter(ReductFactoryOptions.DecisionTable, data);
            parm.SetParameter(ReductFactoryOptions.ReductType, 
                ReductTypes.FEpsilonApproximateReduct);
            parm.SetParameter(ReductFactoryOptions.FMeasure, 
                (FMeasure) FMeasures.Majority);
            parm.SetParameter(ReductFactoryOptions.Epsilon, 0.05);

            var reductGenerator = ReductFactory.GetReductGenerator(parm);
            var reducts = reductGenerator.GetReducts();

            foreach (IReduct reduct in reducts)
                Console.WriteLine(reduct.Attributes.ToArray().ToStr());
        }
    }
}
