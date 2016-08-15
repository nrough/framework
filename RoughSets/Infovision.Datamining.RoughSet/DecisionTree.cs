using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;
using Infovision.Math;

namespace Infovision.Datamining.Roughset
{
    public interface IDecisionTree : ILearner, IPredictionModel
    {
        ITreeNode Root { get; }
        decimal Epsilon { get; }
        int NumberOfAttributesToCheckForSplit { get; set; }
    }

    public abstract class DecisionTree : IDecisionTree
    {
        private DecisionTreeNode root;
        private int decisionAttributeId;
        private decimal mA;

        public ITreeNode Root { get { return this.root; } }
        public int NumberOfAttributesToCheckForSplit { get; set; }
        public decimal Epsilon { get; set; }

        public DecisionTree()
        {
            this.root = null;
            this.decisionAttributeId = -1;
            this.NumberOfAttributesToCheckForSplit = -1;
            this.Epsilon = decimal.MinusOne;
        }

        protected void Init(DataStore data, int[] attributes)
        {
            this.root = new DecisionTreeNode(-1, -1, null);
            this.decisionAttributeId = data.DataStoreInfo.DecisionFieldId;

            EquivalenceClassCollection eqClassCollection = EquivalenceClassCollection.Create(attributes, data, data.Weights);
            IReduct reduct = new ReductWeights(data, attributes, Decimal.Zero, data.Weights, eqClassCollection);
            this.mA = new InformationMeasureWeights().Calc(reduct);

        }

        public virtual double Learn(DataStore data, int[] attributes)
        {
            this.Init(data, attributes);

            EquivalenceClassCollection eqClasscollection = EquivalenceClassCollection.Create(attributes, data, data.Weights);
            this.GenerateSplits(eqClasscollection, this.root);
            ClassificationResult trainResult = this.Classify(data, data.Weights);
            return 1 - trainResult.Accuracy;
        }

        protected void CreateLeaf(DecisionTreeNode parent, long decisionValue)
        {
            parent.AddChild(new DecisionTreeNode(this.decisionAttributeId, decisionValue, parent));
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

            if (this.Epsilon >= Decimal.Zero)
            {
                decimal m = DecisionTreeHelper.CalcMajorityMeasureFromTree(this.root, eqClassCollection.Data, eqClassCollection.Data.Weights);
                if ((Decimal.One - this.Epsilon) * this.mA <= m)
                {
                    this.CreateLeaf(parent, eqClassCollection.DecisionWeights.FindMaxValueKey());
                    return;
                }
            }

            int maxAttribute = this.GetNextSplit(eqClassCollection, decisions);

            //Generate split on result
            Dictionary<long, EquivalenceClassCollection> subEqClasses 
                = EquivalenceClassCollection.Split(eqClassCollection, maxAttribute);
            foreach (var kvp in subEqClasses)
            {
                DecisionTreeNode newNode = new DecisionTreeNode(maxAttribute, kvp.Key, parent);
                parent.AddChild(newNode);

                this.GenerateSplits(kvp.Value, newNode);
            }
        }

        private bool IsNextNodeALeafNode(ITreeNode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            
            if (node.Children == null)
                throw new InvalidOperationException("node.Children == null");

            if (node.Children.First().Key == this.decisionAttributeId)
                return true;

            return false;
        }

        private long GetDecision(ITreeNode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (node.Children == null)
                throw new ArgumentException("node.Children == null", "node");

            return node.Children.First().Value;
        }

        public long Compute(DataRecordInternal record)
        {
            if (this.Root == null)
                throw new InvalidOperationException("this.Root == null");

            ITreeNode current = this.Root;
            while (current != null)
            {
                if (current.IsLeaf)
                    return current.Value;

                if (this.IsNextNodeALeafNode(current))
                    return this.GetDecision(current);

                current = current.Children.Where(x => x.Value == record[x.Key]).FirstOrDefault();
            }

            return -1;
        }

        public ClassificationResult Classify(DataStore testData, decimal[] weights = null)
        {
            ClassificationResult result = new ClassificationResult(testData, testData.DataStoreInfo.GetDecisionValues());
            result.QualityRatio = ((DecisionTreeNode)this.Root).GetChildUniqueKeys().Count;
            result.EnsembleSize = 1;

            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = System.Math.Max(1, Environment.ProcessorCount)
            };
#if DEBUG
            options.MaxDegreeOfParallelism = 1;
#endif

            if (weights == null)
            {
                double w = 1.0 / testData.NumberOfRecords;
                Parallel.For(0, testData.NumberOfRecords, options, objectIndex =>
                {
                    DataRecordInternal record = testData.GetRecordByIndex(objectIndex, false);
                    var prediction = this.Compute(record);
                    result.AddResult(objectIndex, prediction, record[testData.DataStoreInfo.DecisionFieldId], w);
                }
                );
            }
            else
            {
                Parallel.For(0, testData.NumberOfRecords, options, objectIndex =>
                {
                    DataRecordInternal record = testData.GetRecordByIndex(objectIndex, false);
                    var prediction = this.Compute(record);
                    result.AddResult(
                        objectIndex,
                        prediction,
                        record[testData.DataStoreInfo.DecisionFieldId],
                        (double)weights[objectIndex]);
                }
                );
            }

            result.ClassificationTime = 0;
            result.QualityRatio = 0;

            return result;
        }

        protected virtual int GetNextSplit(EquivalenceClassCollection eqClassCollection, PascalSet<long> decisions)
        {
            double currentScore = this.GetCurrentScore(eqClassCollection, decisions);
            int[] localAttributes = eqClassCollection.Attributes;

            if (this.NumberOfAttributesToCheckForSplit != -1)
            {
                int m = System.Math.Min(localAttributes.Length, this.NumberOfAttributesToCheckForSplit);
                localAttributes = localAttributes.RandomSubArray(m);
            }

            object tmpLock = new object();
            var rangePrtitioner = Partitioner.Create(0, localAttributes.Length);
            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };
#if DEBUG
            options.MaxDegreeOfParallelism = 1;
#endif

            Pair<int, double> bestAttribute = new Pair<int, double>(-1, Double.MinValue);
            Parallel.ForEach(
                rangePrtitioner,
                options,
                () => new Pair<int, double>(-1, Double.MinValue),
                (range, loopState, initialValue) =>
                {
                    Pair<int, double> partialBestAttribute = initialValue;
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        EquivalenceClassCollection attributeEqClasses
                            = EquivalenceClassCollection.Create(new int[] { localAttributes[i] }, eqClassCollection);
                        attributeEqClasses.RecalcEquivalenceClassStatistic(eqClassCollection.Data);
                        double score = this.GetSplitScore(attributeEqClasses, currentScore);

                        if (partialBestAttribute.Item2 < score)
                        {
                            partialBestAttribute.Item2 = score;
                            partialBestAttribute.Item1 = localAttributes[i];
                        }
                    }
                    return partialBestAttribute;
                },
                (localPartialBestAttribute) =>
                {
                    lock (tmpLock)
                    {
                        if (bestAttribute.Item2 < localPartialBestAttribute.Item2)
                        {
                            bestAttribute.Item2 = localPartialBestAttribute.Item2;
                            bestAttribute.Item1 = localPartialBestAttribute.Item1;
                        }
                    }
                });

            return bestAttribute.Item1;
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

    /// <summary>
    /// CART Tree implementation
    /// </summary>
    /// <remarks>
    /// Based on the following example http://csucidatamining.weebly.com/assign-4.html
    /// </remarks>
    public class DecisionTreeCART : DecisionTree
    {
        
        protected override double GetSplitScore(EquivalenceClassCollection attributeEqClasses, double gini)
        {
            double attributeGini = 0;
            foreach (var eq in attributeEqClasses)
            {
                double pA = (double)(eq.WeightSum / attributeEqClasses.WeightSum);

                double s2 = 0;
                foreach (long dec in eq.DecisionSet)
                {
                    double pD = (double)(eq.GetDecisionWeight(dec) / eq.WeightSum); 
                    s2 += (pD * pD);
                }
                attributeGini += (pA * (1.0 - s2));
            }
            return gini - attributeGini;
        }

        protected override double GetCurrentScore(EquivalenceClassCollection eqClassCollection, PascalSet<long> decisions)
        {
            double s2 = 0;
            foreach (long dec in decisions)
            {
                decimal decWeightedProbability = eqClassCollection.CountWeightDecision(dec);
                double p = (double)(decWeightedProbability / eqClassCollection.WeightSum);
                s2 += (p * p);
            }
            return 1.0 - s2;
        }
    }


    public class DecisionTreeRough : DecisionTree
    {
        protected override double GetSplitScore(EquivalenceClassCollection attributeEqClasses, double dummy)
        {
            decimal result = Decimal.Zero;            
            decimal maxValue, sum;
            foreach (var eq in attributeEqClasses)
            {
                maxValue = Decimal.MinValue;
                foreach (long decisionValue in eq.DecisionValues)
                {
                    sum = eq.GetDecisionWeight(decisionValue);
                    if (sum > maxValue)
                        maxValue = sum;
                }
                result += maxValue;
            }
            return (double)result;
        }

        protected override double GetCurrentScore(EquivalenceClassCollection eqClassCollection, PascalSet<long> decisions)
        {
            return 0;
        }
    }
}