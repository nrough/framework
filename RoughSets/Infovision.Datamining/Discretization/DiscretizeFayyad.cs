﻿using Infovision.Data;
using Infovision.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Discretization
{
    [Serializable]
    public class DiscretizeFayyad : DiscretizeSupervisedBase
    {
        public DiscretizeFayyad()
            : base() {}

        protected override bool StopCondition(
            double[] priorCount, double[] left, double[] right, 
            int numOfPossibleCuts, int numOfInstances, double instanceWeight)
        {
            if (base.StopCondition(priorCount, left, right, numOfPossibleCuts, numOfInstances, instanceWeight))
                return true;
            
            double priorEntropy = Tools.Entropy(priorCount);
            double entropyLeft = Tools.Entropy(left);
            double entropyRight = Tools.Entropy(right);

            double gain = priorEntropy
                - ((left.Sum() / instanceWeight) * entropyLeft)
                - ((right.Sum() / instanceWeight) * entropyRight);

            int numClassesTotal = priorCount.Count(val => val > 0);
            int numClassesLeft = left.Count(val => val > 0);
            int numClassesRight = right.Count(val => val > 0);

            double delta = System.Math.Log(System.Math.Pow(3, numClassesTotal) - 2.0, 2)
                - ((numClassesTotal * priorEntropy) - (numClassesRight * entropyRight) - (numClassesLeft * entropyLeft));
            
            if (gain > ((System.Math.Log(numOfInstances - 1.0, 2) + delta) / (double)numOfInstances))
                return false;

            return true;
        }
    }
}
