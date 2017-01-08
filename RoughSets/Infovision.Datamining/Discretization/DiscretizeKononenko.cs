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
            double before = Tools.Log2Binomial(numOfPossibleCuts + labelCount - 1, labelCount - 1)
                + Tools.Log2Multinomial(numOfPossibleCuts, priorCount);

            double leftSum = left.Sum();
            double rightSum = right.Sum();

            double after = System.Math.Log(numOfPossibleCuts, 2)
                + Tools.Log2Binomial(leftSum + labelCount - 1, labelCount - 1)
                + Tools.Log2Binomial(rightSum + labelCount - 1, labelCount - 1)
                + Tools.Log2Multinomial(leftSum, left)
                + Tools.Log2Multinomial(rightSum, right);
                                   
            if (before > after)
                return false;

            return true;
        }
    }
}
