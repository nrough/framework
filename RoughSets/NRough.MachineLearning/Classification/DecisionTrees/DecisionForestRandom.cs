using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRough.MachineLearning;
using NRough.Data;
using NRough.Core;

namespace NRough.MachineLearning.Classification.DecisionTrees
{
    public class DecisionForestRandom<TTree> : DecisionForestBase<TTree>
        where TTree : IDecisionTree, new()
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

        public DecisionForestRandom(string modelName)
            : base(modelName) { }

        protected override TTree InitDecisionTree()
        {
            TTree tree = base.InitDecisionTree();

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
