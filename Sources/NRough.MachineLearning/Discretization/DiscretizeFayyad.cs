//
// NRough Framework
// http://www.nrough.net
//
// Copyright © Sebastian Widz, 2017
// authors at nrough.net

//This file is part of NRough.

//Foobar is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 3 of the License, or
//(at your option) any later version.
//
//Foobar is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Foobar; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//
using NRough.Data;
using NRough.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Discretization
{
    [Serializable]
    public class DiscretizeFayyad : DiscretizeSupervisedBase
    {
        public DiscretizeFayyad()
            : base()
        {
            this.UseWeights = true;
        }

        protected override bool StopCondition(
            EquivalenceClassCollection priorEqClasses,
            EquivalenceClassCollection splitEqClasses,
            int numOfPossibleCuts)
        {
            if (base.StopCondition(priorEqClasses, splitEqClasses, numOfPossibleCuts))
                return true;

            double priorEntropy = ImpurityMeasure.Entropy(priorEqClasses);            
            double splitEntropy = ImpurityMeasure.Entropy(splitEqClasses);            

            double gain = priorEntropy - splitEntropy;            
            
            int numClassesTotal = priorEqClasses.DecisionWeight.Count(kvp => kvp.Value > 0);
            int numClassesLeft = splitEqClasses[leftInstance].DecisionWeight.Count(kvp => kvp.Value > 0);
            int numClassesRight = splitEqClasses[rightInstance].DecisionWeight.Count(kvp => kvp.Value > 0);

            double entropyLeft = ImpurityMeasure.Entropy(new EquivalenceClassCollection(splitEqClasses[leftInstance]));
            double entropyRight = ImpurityMeasure.Entropy(new EquivalenceClassCollection(splitEqClasses[rightInstance]));

            double delta = System.Math.Log(System.Math.Pow(3, numClassesTotal) - 2.0, 2)
                - ((numClassesTotal * priorEntropy) 
                    - (numClassesRight * entropyRight) 
                    - (numClassesLeft * entropyLeft));

            if (gain > ((System.Math.Log(priorEqClasses.NumberOfObjects - 1.0, 2) + delta) / (double)priorEqClasses.NumberOfObjects))
                return false;

            return true;
        }

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
