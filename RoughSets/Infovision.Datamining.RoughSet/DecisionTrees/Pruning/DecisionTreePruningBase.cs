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
        private object syncRoot = new object();
        public DataStore PruningData { get; private set; }
        public IDecisionTree DecisionTree { get; private set; }
        public double GainThreshold { get; set; }

        public DecisionTreePruningBase(IDecisionTree decisionTree, DataStore data)
        {           
            if (decisionTree == null)
                throw new ArgumentNullException("decisionTree");

            if (decisionTree.Root == null)
                throw new InvalidOperationException("");

            this.DecisionTree = decisionTree;

            if (data == null)
                throw new ArgumentException("data");

            this.PruningData = data;

            this.GainThreshold = 0;
        }

        public abstract double Prune();
    }
}
