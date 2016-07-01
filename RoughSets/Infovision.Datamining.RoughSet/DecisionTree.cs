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
        public int NumberOfRandomAttributes { get; set; }
        
        public DecisionTree()
        {
            this.root = null;
            this.decisionAttributeId = -1;
            this.NumberOfRandomAttributes = -1;
        }

        public void Learn(DataStore data, int[] attributes)
        {
            this.root = new DecisionTreeNode(-1, -1);
            this.decisionAttributeId = data.DataStoreInfo.DecisionFieldId;
            EquivalenceClassCollection eqClasscollection = EquivalenceClassCollection.Create(attributes, data, 0, data.Weights);
            this.GenerateSplits(eqClasscollection, this.root);
        }

        protected void CreateLeaf(DecisionTreeNode parent, long decisionValue)
        {
            parent.AddChild(new DecisionTreeNode(this.decisionAttributeId, decisionValue));
        }

        protected void GenerateSplits(EquivalenceClassCollection eqClassCollection, DecisionTreeNode parent)
        {
            if (eqClassCollection.ObjectsCount == 0 || eqClassCollection.Attributes.Length == 0)
            {
                this.CreateLeaf(parent, eqClassCollection.DecisionWeights.FindMaxValueKey());
                return;
            }
            
            PascalSet<long> decisions = eqClassCollection.DecisionSet;

            if (decisions.Count == 1)
            {
                this.CreateLeaf(parent, decisions.First());
                return;
            }

            int maxAttribute = this.GetNextSplit(eqClassCollection, decisions);

            //Generate split on result
            Dictionary<long, EquivalenceClassCollection> subEqClasses = EquivalenceClassCollection.Split(eqClassCollection, maxAttribute);
            foreach(var kvp in subEqClasses)
            {
                DecisionTreeNode newNode = new DecisionTreeNode(maxAttribute, kvp.Key);
                parent.AddChild(newNode);

                this.GenerateSplits(kvp.Value, newNode);
            }
        }        

        public long Compute(DataRecordInternal record, ITreeNode subtree)
        {
            if (subtree == null)
                throw new ArgumentNullException("subtree");

            ITreeNode current = subtree;
            while (current != null)
            {
                if (current.IsLeaf)
                    return current.Value;

                if (current.Children == null)
                    throw new InvalidOperationException("There is an error in decision tree structure. Non leaf nodes should have non null children list.");

                current = current.Children.Where(x => x.Value == record[x.Key]).FirstOrDefault();
            }

            return -1;
        }

        public ClassificationResult Classify(DataStore testData, decimal[] weights = null)
        {
            ClassificationResult result = new ClassificationResult(testData, testData.DataStoreInfo.GetDecisionValues());

            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = System.Math.Max(1, Environment.ProcessorCount / 2)
            };
#if DEBUG
            options.MaxDegreeOfParallelism = 1;
#endif

            if (weights == null)
            {
                double w = 1.0 / testData.NumberOfRecords;
                //for(int objectIndex=0; objectIndex<testData.NumberOfRecords; objectIndex++)
                Parallel.For(0, testData.NumberOfRecords, options, objectIndex =>
                {
                    DataRecordInternal record = testData.GetRecordByIndex(objectIndex, false);
                    var prediction = this.Compute(record, this.Root);
                    result.AddResult(objectIndex, prediction, record[testData.DataStoreInfo.DecisionFieldId], w);
                }
                );
            }
            else
            {
                //for (int objectIndex = 0; objectIndex < testData.NumberOfRecords; objectIndex++)
                Parallel.For(0, testData.NumberOfRecords, options, objectIndex =>
                {
                    DataRecordInternal record = testData.GetRecordByIndex(objectIndex, false);
                    var prediction = this.Compute(record, this.Root);
                    result.AddResult(
                        objectIndex,
                        prediction,
                        record[testData.DataStoreInfo.DecisionFieldId],
                        (double)weights[objectIndex]);
                }
                );
            }

            return result;
        }

        protected virtual int GetNextSplit(EquivalenceClassCollection eqClassCollection, PascalSet<long> decisions)
        {
            int[] localAttributes = eqClassCollection.Attributes;
            if (this.NumberOfRandomAttributes != -1)
            {
                int m = System.Math.Min(localAttributes.Length, this.NumberOfRandomAttributes);
                localAttributes = localAttributes.RandomSubArray(m);
            }

            int result = 0;
            double maxScore = Double.MinValue;
            double currentScore = this.GetCurrentScore(eqClassCollection, decisions);

            foreach (int attribute in localAttributes)
            {
                EquivalenceClassCollection attributeEqClasses
                    = EquivalenceClassCollection.Create(new int[] { attribute }, eqClassCollection, 0);
                attributeEqClasses.RecalcEquivalenceClassStatistic(eqClassCollection.Data);

                double score = this.GetSplitScore(attributeEqClasses, currentScore);
                if (maxScore < score)
                {
                    maxScore = score;
                    result = attribute;
                }
            }

            return result;
        }

        protected virtual double GetCurrentScore(EquivalenceClassCollection eqClassCollection, PascalSet<long> decisions)
        {
            throw new NotImplementedException();
        }

        protected virtual double GetSplitScore(EquivalenceClassCollection attributeEqClasses, double currentScore)
        {
            throw new NotImplementedException();
        }

    }

    public class DecisionTreeID3 : DecisionTree
    {
        protected override double GetSplitScore(EquivalenceClassCollection attributeEqClasses, double entropy)
        {
            double result = 0;
            foreach (var eq in attributeEqClasses)
            {
                double localEntropy = 0;
                foreach (var dec in eq.DecisionSet)
                {
                    decimal decWeight = eq.GetDecisionWeight(dec);
                    double p = (double)(decWeight / eq.WeightSum);
                    if (p != 0)
                        localEntropy -= p * System.Math.Log(p, 2);
                }

                result += (double)(eq.WeightSum / attributeEqClasses.WeightSum) * localEntropy;
            }

            return entropy - result;
        }

        protected override double GetCurrentScore(EquivalenceClassCollection eqClassCollection, PascalSet<long> decisions)
        {
            double entropy = 0;
            foreach (long dec in decisions)
            {
                decimal decWeightedProbability = eqClassCollection.CountWeightDecision(dec);
                double p = (double)(decWeightedProbability / eqClassCollection.WeightSum);
                if (p != 0)
                    entropy -= p * System.Math.Log(p, 2);
            }
            return entropy;
        }
    }

    public class DecisionTreeC45 : DecisionTreeID3
    {
        protected override double GetSplitScore(EquivalenceClassCollection attributeEqClasses, double entropy)
        {
            double gain = base.GetSplitScore(attributeEqClasses, entropy);
            double splitInfo = this.SplitInfo(attributeEqClasses);
            return (splitInfo == 0) ? 0 : gain / splitInfo;
        }

        private double SplitInfo(EquivalenceClassCollection eqClassCollection)
        {
            double result = 0;
            foreach (var eq in eqClassCollection)
            {
                double p = (double)(eq.WeightSum / eqClassCollection.WeightSum);
                if (p != 0)
                    result -= p * System.Math.Log(p, 2);
            }
            return result;
        }
    }

    public class DecisionTreeCART : DecisionTree
    {
        protected override double GetSplitScore(EquivalenceClassCollection attributeEqClasses, double gini)
        {
            throw new NotImplementedException();
        }

        protected override double GetCurrentScore(EquivalenceClassCollection eqClassCollection, PascalSet<long> decisions)
        {
            throw new NotImplementedException();
        }
    }

    
}