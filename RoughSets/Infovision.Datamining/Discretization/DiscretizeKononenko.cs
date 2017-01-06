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
            double[] priorCount, double[] left, double[] right, int numOfPossibleCuts, double numOfInstances)
        {
            if (base.StopCondition(priorCount, left, right, numOfPossibleCuts, numOfInstances))
                return true;

            int numClassesTotal = priorCount.Count(val => val > 0);
                        
            // Encode distribution prior to split
            double distPrior = Tools.Log2Binomial(numOfInstances + numClassesTotal - 1, numClassesTotal - 1);

            // Encode instances prior to split.
            double instPrior = Tools.Log2Multinomial(numOfInstances, priorCount);

            double before = instPrior + distPrior;

            // Encode distributions and instances after split.
            double leftSum = left.Sum();
            double rightSum = right.Sum();

            double distAfter = Tools.Log2Binomial(leftSum + numClassesTotal - 1, numClassesTotal - 1)
                             + Tools.Log2Binomial(rightSum + numClassesTotal - 1, numClassesTotal - 1);

            double instAfter = Tools.Log2Multinomial(leftSum, left)
                             + Tools.Log2Multinomial(rightSum, right);

            // Coding cost after split
            double after = System.Math.Log(numOfPossibleCuts, 2) + distAfter + instAfter;

            // Check if split is to be accepted
            if (before > after)
                return false;

            return true;
        }
    }
}
