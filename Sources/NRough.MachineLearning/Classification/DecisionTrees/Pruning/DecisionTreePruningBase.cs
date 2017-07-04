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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Classification.DecisionTrees.Pruning
{
    public abstract class DecisionTreePruningBase : IDecisionTreePruning
    {
        //private object syncRoot = new object();

        public DataStore PruningData { get; set; }
        public IDecisionTree DecisionTree { get; set; }
        public double GainThreshold { get; set; }

        public DecisionTreePruningBase()
        {
            this.GainThreshold = 0;
        }

        public DecisionTreePruningBase(IDecisionTree decisionTree, DataStore data)
            : this()
        {           
            if (decisionTree == null)
                throw new ArgumentNullException("decisionTree");

            if (decisionTree.Root == null)
                throw new InvalidOperationException("");

            this.DecisionTree = decisionTree;

            if (data == null)
                throw new ArgumentException("data");

            this.PruningData = data;            
        }

        public static IDecisionTreePruning Construct(PruningType pruningType, IDecisionTree decisionTree, DataStore data)
        {
            IDecisionTreePruning ret;
            switch (pruningType)
            {
                case PruningType.ErrorBasedPruning:
                    ret = new ErrorBasedPruning(decisionTree, data);
                    break;

                case PruningType.ReducedErrorPruning:
                    ret = new ReducedErrorPruningBottomUp(decisionTree, data);
                    break;

                default:
                    throw new NotImplementedException(String.Format("Pruning type {0} is not implemented", pruningType));
            }

            return ret;
        }

        public abstract double Prune();       
    }
}
