using Infovision.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Discretization
{
    [Serializable]
    public class DiscretizeKononenko : DiscretizeSupervisedBase
    {
        public DiscretizeKononenko()
            : base() {}

        protected override bool StopCondition(
            double[] priorCount, double[] left, double[] right, int numOfPossibleCuts, int numOfInstances, double instanceWeight)
        {
            if (base.StopCondition(priorCount, left, right, numOfPossibleCuts, numOfInstances, instanceWeight))
                return true;

            int labelCount = priorCount.Count(val => val > 0);            

            double before = Tools.Log2Binomial(numOfInstances + labelCount - 1, labelCount - 1)
                + Tools.Log2Multinomial(numOfInstances, priorCount.Select(d => (int)d).ToArray());

            double leftSum = left.Sum();
            double rightSum = right.Sum();

            double after = System.Math.Log(numOfInstances, 2)
                + Tools.Log2Binomial((int)leftSum + labelCount - 1, labelCount - 1)
                + Tools.Log2Binomial((int)rightSum + labelCount - 1, labelCount - 1)
                + Tools.Log2Multinomial((int)leftSum, left.Select(d => (int)d).ToArray())
                + Tools.Log2Multinomial((int)rightSum, right.Select(d => (int)d).ToArray());
                                   
            if (before > after)
                return false;

            return true;
        }
    }
}
