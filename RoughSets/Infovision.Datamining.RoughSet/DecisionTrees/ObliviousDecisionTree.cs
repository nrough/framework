using Infovision.Data;
using Infovision.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    public class ObliviousDecisionTree : DecisionTreeBase
    {
        private Dictionary<int[], double> cache;

        public ObliviousDecisionTree()
            : base()
        {
            this.cache = new Dictionary<int[], double>(new ArrayComparer<int>());
        }

        protected override SplitInfo GetSplitInfoSymbolic(int attributeId, EquivalenceClassCollection data, double dummy)
        {
            int[] newAttributes = new int[data.Attributes.Length + 1];
            Array.Copy(data.Attributes, newAttributes, data.Attributes.Length);
            newAttributes[data.Attributes.Length] = attributeId;

            double m = 0.0;
            if (!cache.TryGetValue(newAttributes, out m))
            {
                var attributeEqClasses = EquivalenceClassCollection.Create(newAttributes, data.Data, data.Data.Weights);
                m = InformationMeasureWeights.Instance.Calc(attributeEqClasses);
                cache.Add(newAttributes, m);
            }

            var subsetEqClassCollection = EquivalenceClassCollection.Create(newAttributes, data.Data, data.Data.Weights, data.Indices);

            return new SplitInfo(attributeId, m, subsetEqClassCollection, SplitType.Discreet, ComparisonType.EqualTo, 0);
        }
    }
}
