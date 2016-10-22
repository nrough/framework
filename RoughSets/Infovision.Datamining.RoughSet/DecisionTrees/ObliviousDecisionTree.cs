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

        protected override double GetCurrentScore(EquivalenceClassCollection eqClassCollection)
        {
            return 0;
        }

        protected override SplitInfo GetSplitInfoSymbolic(int attributeId, EquivalenceClassCollection data, double dummy)
        {
            int[] newAttributes = new int[data.Attributes.Length + 1];
            if(data.Attributes.Length != 0)
                Array.Copy(data.Attributes, newAttributes, data.Attributes.Length);
            newAttributes[data.Attributes.Length] = attributeId;

            double m;
            EquivalenceClassCollection attributeEqClasses = null;
            if (!cache.TryGetValue(newAttributes, out m))
            {
                attributeEqClasses = EquivalenceClassCollection.Create(newAttributes, data.Data, data.Data.Weights);
                m = InformationMeasureWeights.Instance.Calc(attributeEqClasses);
                cache.Add(newAttributes, m);
            }

            if(data.Indices.Length != data.Data.NumberOfRecords)
                attributeEqClasses = EquivalenceClassCollection.Create(newAttributes, data.Data, data.Data.Weights, data.Indices);

            return new SplitInfo(attributeId, m, attributeEqClasses, SplitType.Discreet, ComparisonType.EqualTo, 0);
        }
    }
}
