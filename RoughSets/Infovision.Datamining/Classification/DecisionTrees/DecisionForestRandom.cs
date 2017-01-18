using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raccoon.MachineLearning;
using Raccoon.Data;
using Raccoon.Core;

namespace Raccoon.MachineLearning.Classification.DecisionTrees
{
    public class DecisionForestRandom<T> : DecisionForestBase<T>
        where T : IDecisionTree, new()
    {
        private int numberOfAttributesToCheckForSplit = -1;
        private bool isNumberOfAttributesToCheckForSplitSet = false;

        public int NumberOfAttributesToCheckForSplit
        {
            get { return this.numberOfAttributesToCheckForSplit; }

            set
            {
                this.numberOfAttributesToCheckForSplit = value;
                this.isNumberOfAttributesToCheckForSplitSet = true;
            }
        }

        public DecisionForestRandom()
            : base() { }

        protected override T InitDecisionTree()
        {
            T tree = base.InitDecisionTree();

            if (this.NumberOfAttributesToCheckForSplit > 0)
                tree.NumberOfAttributesToCheckForSplit = this.NumberOfAttributesToCheckForSplit;

            return tree;
        }

        public override ClassificationResult Learn(DataStore data, int[] attributes)
        {
            if (!this.isNumberOfAttributesToCheckForSplitSet)
                this.NumberOfAttributesToCheckForSplit = (int)System.Math.Floor(System.Math.Sqrt(attributes.Length));

            return base.Learn(data, attributes);
        }
    }
}
