using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Math;
using Infovision.Statistics;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    public interface IDecisionTree
    {
        void Learn(DataStore data, int[] attributes);
    }
    
    public abstract class DecisionTree : IDecisionTree
    {
        private DecisionTreeNode root;
        private int decisionAttributeId;

        public DecisionTreeNode Root { get { return this.root; } }
        
        public DecisionTree()
        {
            this.root = null;
        }

        public void Learn(DataStore data, int[] attributes)
        {
            this.root = null;
            this.decisionAttributeId = data.DataStoreInfo.DecisionFieldId;
            EquivalenceClassCollection eqClasscollection = EquivalenceClassCollection.Create(attributes, data, 0, data.Weights);
            this.GenerateSplits(eqClasscollection, this.root, -1);
        }

        protected void GenerateSplits(EquivalenceClassCollection eqClassCollection, DecisionTreeNode parent, long value)
        {
            if (eqClassCollection.ObjectsCount == 0)
            {
                return;
            }

            if(eqClassCollection.Attributes.Length == 1)
            {
                long maxDecision = eqClassCollection.DecisionWeights.FindMaxValueKey();
                parent.AddChild(
                    new DecisionTreeNode(
                        this.decisionAttributeId, 
                        maxDecision));
                return;
            }
            
            PascalSet<long> decisions = eqClassCollection.DecisionSet;

            if (decisions.Count == 1)
            {
                parent.AddChild(
                    new DecisionTreeNode(
                        this.decisionAttributeId,
                        decisions.First()));
                return;
            }

            int maxAttribute = this.GetNextSplit(eqClassCollection, decisions);

            DecisionTreeNode newNode = null;

            //root
            if (parent == null)
            {
                this.root = new DecisionTreeNode(maxAttribute, value);
                parent = this.root;
                newNode = this.root;
            }
            else
            {                
                newNode = new DecisionTreeNode(maxAttribute, value);
                parent.AddChild(newNode);
            }

            //Generate split on result
            Dictionary<long, EquivalenceClassCollection> subEqClasses = EquivalenceClassCollection.Split(eqClassCollection, maxAttribute);
            foreach(var kvp in subEqClasses)
            {
                this.GenerateSplits(kvp.Value, newNode, kvp.Key);
            }
        }

        protected abstract int GetNextSplit(EquivalenceClassCollection eqClassCollection, PascalSet<long> decisions);
    }

    public class DecisionTreeID3 : DecisionTree
    {
        protected override int GetNextSplit(EquivalenceClassCollection eqClassCollection, PascalSet<long> decisions)
        {
            int result = 0;

            double entropy = 0;
            foreach (long dec in decisions)
            {
                decimal decWeightedProbability = eqClassCollection.CountWeightDecision(dec);
                entropy -= (double)(decWeightedProbability / eqClassCollection.ObjectsWeightCount)
                    * System.Math.Log((double)(decWeightedProbability / eqClassCollection.ObjectsWeightCount), 2);
            }

            double maxGain = Double.MinValue;            
            foreach (int attribute in eqClassCollection.Attributes)
            {
                EquivalenceClassCollection attributeEqClasses = EquivalenceClassCollection.Create(new int[] { attribute }, eqClassCollection, 0);
                attributeEqClasses.RecalcEquivalenceClassStatistic(eqClassCollection.Data);

                double score = entropy - this.Gain(attributeEqClasses);
                if (maxGain < score)
                {
                    maxGain = score;
                    result = attribute;
                }
            }

            return result;
        }

        protected double Gain(EquivalenceClassCollection eqClassCollection)
        {
            double result = 0;
            foreach (var eq in eqClassCollection)
            {
                double localEntropy = 0;
                foreach (var dec in eq.DecisionSet)
                {
                    decimal decWeight = eq.GetDecisionWeight(dec);
                    localEntropy -= ((double)(decWeight / eq.WeightSum) * System.Math.Log((double)(decWeight / eq.WeightSum), 2));
                }
                result += (double)(eq.WeightSum / eqClassCollection.ObjectsWeightCount) * localEntropy;
            }

            return result;
        }
    }

    public class DecisionTreeC45 : DecisionTreeID3
    {
        protected override int GetNextSplit(EquivalenceClassCollection eqClassCollection, PascalSet<long> decisions)
        {
            int result = 0;

            double entropy = 0;
            foreach (long dec in decisions)
            {
                decimal decWeightedProbability = eqClassCollection.CountWeightDecision(dec);
                entropy -= (double)(decWeightedProbability / eqClassCollection.ObjectsWeightCount)
                    * System.Math.Log((double)(decWeightedProbability / eqClassCollection.ObjectsWeightCount), 2);
            }

            double maxGain = Double.MinValue;
            foreach (int attribute in eqClassCollection.Attributes)
            {
                EquivalenceClassCollection attributeEqClasses = EquivalenceClassCollection.Create(new int[] { attribute }, eqClassCollection, 0);
                attributeEqClasses.RecalcEquivalenceClassStatistic(eqClassCollection.Data);

                double score = (entropy - this.Gain(attributeEqClasses)) / this.SplitInfo(attributeEqClasses);
                if (maxGain < score)
                {
                    maxGain = score;
                    result = attribute;
                }
            }

            return result;
        }

        protected double SplitInfo(EquivalenceClassCollection eqClassCollection)
        {
            double result = 0;
            foreach (var eq in eqClassCollection)
                result += (double)(eq.WeightSum / eqClassCollection.ObjectsWeightCount) * System.Math.Log((double)(eq.WeightSum / eqClassCollection.ObjectsWeightCount), 2);
            return result;
        }
    }
}