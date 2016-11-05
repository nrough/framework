using Infovision.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionTrees.Pruning
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
