using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;

namespace Infovision.Datamining.Roughset
{
    public interface IDecisionTree
    {
        public void Learn(DataStore data, int[] attributes);
    }
    
    public class DecisionTree : IDecisionTree
    {
        public DecisionTree()
        {
        }

        public void Learn(DataStore data, int[] attributes)
        {            
            EquivalenceClassCollection eqClasscollection = EquivalenceClassCollection.Create(attributes, data, 0, data.Weights);
            this.GenerateSplits(data, eqClasscollection);
        }

        protected void GenerateSplits(DataStore data, EquivalenceClassCollection eqClassCollection)
        {
            decimal maxScore = Decimal.MinValue;
            int maxAttribute = 0;

            foreach(int attribute in eqClassCollection.Attributes)
            {                
                decimal score = this.GetScore(eqClassCollection, attribute);
                if (maxScore < score)
                {
                    maxScore = score;
                    maxAttribute = attribute;
                }
            }

            //Generate split on maxAttribute
            Dictionary<long, EquivalenceClassCollection> subEqClasses = EquivalenceClassCollection.Split(eqClassCollection, maxAttribute);
            foreach(var kvp in subEqClasses)
            {
                this.GenerateSplits(data, kvp.Value);
            }
        }

        protected decimal GetScore(EquivalenceClassCollection eqClassCollection, int attributeId)
        {
            EquivalenceClassCollection attributeEqClasses = EquivalenceClassCollection.Create(new int[] { attributeId }, eqClassCollection, 0);
            
            throw new NotImplementedException();
        }
    }
}