using NRough.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Discretization
{
    [Serializable]
    public class DiscretizeKononenko : DiscretizeSupervisedBase
    {
        public DiscretizeKononenko()
            : base() {}

        protected override bool StopCondition(
            EquivalenceClassCollection priorEqClasses,
            EquivalenceClassCollection splitEqClasses,
            int numOfPossibleCuts)
        {
            if (base.StopCondition(priorEqClasses, splitEqClasses, numOfPossibleCuts))
                return true;

            int labelCount = priorEqClasses.DecisionCount.Count(kvp => kvp.Value > 0);

            double before = Tools.Log2Binomial(priorEqClasses.NumberOfObjects + labelCount - 1, labelCount - 1)
                + Tools.Log2Multinomial(priorEqClasses.NumberOfObjects, priorEqClasses.DecisionCount.Values.ToArray());

            double leftSum = splitEqClasses[leftInstance].NumberOfObjects;
            double rightSum = splitEqClasses[rightInstance].NumberOfObjects;

            double after = System.Math.Log(priorEqClasses.NumberOfObjects, 2)
                + Tools.Log2Binomial((int)leftSum + labelCount - 1, labelCount - 1)
                + Tools.Log2Binomial((int)rightSum + labelCount - 1, labelCount - 1)
                + Tools.Log2Multinomial((int)leftSum, splitEqClasses[leftInstance].DecisionCount.Values.ToArray())
                + Tools.Log2Multinomial((int)rightSum, splitEqClasses[rightInstance].DecisionCount.Values.ToArray());

            if (before > after)
                return false;

            return true;
        }

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
