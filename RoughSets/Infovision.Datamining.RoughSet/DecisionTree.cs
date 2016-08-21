using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;
using System.Diagnostics;

namespace Infovision.Datamining.Roughset
{
    /// <summary>
    /// Interface of a decision tree
    /// </summary>
    public interface IDecisionTree : ILearner, IPredictionModel
    {
        ITreeNode Root { get; }
        decimal Epsilon { get; }
        int NumberOfAttributesToCheckForSplit { get; set; }
    }

    /// <summary>
    /// Base class for decision tree implementations
    /// </summary>
    public abstract class DecisionTree : IDecisionTree
    {
        private DecisionTreeNode root;
        private int decisionAttributeId;
        private decimal mA;
        private long[] decisions;

        public ITreeNode Root { get { return this.root; } }
        public int NumberOfAttributesToCheckForSplit { get; set; }
        public decimal Epsilon { get; set; }
        protected IEnumerable<long> Decisions { get { return this.decisions; } }

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
            this.decisions = new long[data.DataStoreInfo.NumberOfDecisionValues];

            int i = 0;
            foreach (long decisionValue in data.DataStoreInfo.DecisionInfo.InternalValues())
                this.decisions[i++] = decisionValue;

            if (this.Epsilon >= Decimal.Zero)
                this.mA = InformationMeasureWeights.Instance.Calc(
                    EquivalenceClassCollection.Create(attributes, data, data.Weights));
        }

        public virtual double Learn(DataStore data, int[] attributes)
        {
            //Stopwatch s = new Stopwatch();
            //s.Start();

            this.Init(data, attributes);
            EquivalenceClassCollection eqClassCollection = EquivalenceClassCollection.Create(new int[] { }, data, data.Weights);
            if (this.Epsilon >= Decimal.Zero)
                this.root.Measure = InformationMeasureWeights.Instance.Calc(eqClassCollection);
            this.GenerateSplits(eqClassCollection, this.root, attributes);

            //s.Stop();

            ClassificationResult trainResult = this.Classify(data, data.Weights);
            //trainResult.ModelCreationTime = s.ElapsedMilliseconds;
            return 1 - trainResult.Accuracy;
        }

        protected void CreateLeaf(DecisionTreeNode parent, long decisionValue)
        {
            parent.AddChild(new DecisionTreeNode(this.decisionAttributeId, decisionValue, parent));
        }

        protected void CreateLeaf(DecisionTreeNode parent, long decisionValue, decimal decisionWeight)
        {
            parent.AddChild(new DecisionTreeNode(this.decisionAttributeId, decisionValue, decisionWeight, parent));
        }

        protected void DEL_GenerateSplits(EquivalenceClassCollection eqClassCollection, DecisionTreeNode parent)
        {
            if (eqClassCollection.Attributes.Length == 0 || eqClassCollection.NumberOfObjects == 0)
            {
                this.CreateLeaf(parent, eqClassCollection.DecisionWeights.FindMaxValueKey());
                return;
            }

            long singleDecision = eqClassCollection.GetSingleDecision().Key;
            if (singleDecision != -1)
            {
                this.CreateLeaf(parent, singleDecision);
                return;
            }

            //TODO This code needs to be checked only for the last child 
            if (this.Epsilon >= Decimal.Zero)
            {
                decimal m = DecisionTreeHelper.CalcMajorityMeasureFromTree(
                    this.root,
                    eqClassCollection.Data,
                    eqClassCollection.Data.Weights);

                if ((Decimal.One - this.Epsilon) * this.mA <= m)
                {
                    this.CreateLeaf(parent, eqClassCollection.DecisionWeights.FindMaxValueKey());
                    return;
                }
            }

            int maxAttribute = this.DEL_GetNextSplit(eqClassCollection);

            //Generate split on result
            Dictionary<long, EquivalenceClassCollection> subEqClasses = EquivalenceClassCollection.Split(maxAttribute, eqClassCollection);
            foreach (var kvp in subEqClasses)
            {
                DecisionTreeNode newNode = new DecisionTreeNode(maxAttribute, kvp.Key, parent);
                parent.AddChild(newNode);

                this.DEL_GenerateSplits(kvp.Value, newNode);
            }
        }

        protected void GenerateSplits(EquivalenceClassCollection eqClassCollection, DecisionTreeNode parent, int[] attributes)
        {
            var splitInfo = Tuple.Create<EquivalenceClassCollection, DecisionTreeNode, int[]>(eqClassCollection, parent, attributes);
            var queue = new Queue<Tuple<EquivalenceClassCollection, DecisionTreeNode, int[]>>();
            queue.Enqueue(splitInfo);
            bool isConverged = false;

            while (queue.Count != 0)
            {
                var currentInfo = queue.Dequeue();
                EquivalenceClassCollection currentEqClassCollection = currentInfo.Item1;
                DecisionTreeNode currentParent = currentInfo.Item2;
                int[] currentAttributes = currentInfo.Item3;

                if (isConverged 
                    || currentAttributes.Length == 0 
                    || currentEqClassCollection.NumberOfObjects == 0)
                {
                    var decision = currentEqClassCollection.DecisionWeights.FindMaxValuePair();
                    this.CreateLeaf(currentParent, decision.Key, decision.Value);
                    continue;
                }

                var singleDecision = currentEqClassCollection.GetSingleDecision();
                if (singleDecision.Key != -1)
                {
                    this.CreateLeaf(currentParent, singleDecision.Key, singleDecision.Value);
                    continue;
                }

                if (this.Epsilon >= Decimal.Zero)
                {
                    decimal m = this.MeasureSum(this.root);
                    if ((Decimal.One - this.Epsilon) * this.mA <= m)
                    {
                        this.CreateLeaf(currentParent, currentEqClassCollection.DecisionWeights.FindMaxValueKey());
                        isConverged = true;
                        continue;
                    }
                }

                var nextSplit = this.GetNextSplit(currentEqClassCollection, currentAttributes);
                int maxAttribute = nextSplit.Item1;
                var subEqClasses = EquivalenceClassCollection.Split(maxAttribute, nextSplit.Item3);

                foreach (var kvp in subEqClasses)
                {
                    DecisionTreeNode newNode = new DecisionTreeNode(maxAttribute, kvp.Key, currentParent);
                    currentParent.AddChild(newNode);

                    if (this.Epsilon >= Decimal.Zero)
                        newNode.Measure = InformationMeasureWeights.Instance.Calc(kvp.Value);

                    var newSplitInfo = Tuple.Create<EquivalenceClassCollection, DecisionTreeNode, int[]>(
                            kvp.Value, 
                            newNode, 
                            currentAttributes.RemoveValue(maxAttribute));

                    queue.Enqueue(newSplitInfo);
                }
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
            Stopwatch s = new Stopwatch();

            s.Start();

            ClassificationResult result = new ClassificationResult(testData, testData.DataStoreInfo.GetDecisionValues());
            result.QualityRatio = ((DecisionTreeNode)this.Root).GetChildUniqueKeys().Count;
            result.EnsembleSize = 1;

            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = InfovisionConfiguration.MaxDegreeOfParallelism
            };

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

            s.Stop();

            result.ClassificationTime = s.ElapsedMilliseconds ;
            return result;
        }

        protected virtual int DEL_GetNextSplit(EquivalenceClassCollection eqClassCollection)
        {
            double currentScore = this.GetCurrentScore(eqClassCollection);
            int[] localAttributes = eqClassCollection.Attributes;

            if (this.NumberOfAttributesToCheckForSplit != -1)
            {
                int m = System.Math.Min(localAttributes.Length, this.NumberOfAttributesToCheckForSplit);
                localAttributes = localAttributes.RandomSubArray(m);
            }

            object tmpLock = new object();
            var rangePrtitioner = Partitioner.Create(0, localAttributes.Length, System.Math.Max(1, localAttributes.Length / Environment.ProcessorCount));
            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = InfovisionConfiguration.MaxDegreeOfParallelism
            };

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
                            = EquivalenceClassCollection.DEL_Create(new int[] { localAttributes[i] }, eqClassCollection);

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

        protected virtual Tuple<int, double, EquivalenceClassCollection> GetNextSplit(EquivalenceClassCollection eqClassCollection, int[] attributesToTest)
        {
            double currentScore = this.GetCurrentScore(eqClassCollection);
            int[] localAttributes = attributesToTest;

            if (this.NumberOfAttributesToCheckForSplit != -1)
            {
                int m = System.Math.Min(localAttributes.Length, this.NumberOfAttributesToCheckForSplit);
                localAttributes = localAttributes.RandomSubArray(m);
            }

            object tmpLock = new object();
            var rangePartitioner = Partitioner.Create(
                0, 
                localAttributes.Length, 
                System.Math.Max(1, localAttributes.Length / InfovisionConfiguration.MaxDegreeOfParallelism));

            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = InfovisionConfiguration.MaxDegreeOfParallelism
            };

            Tuple<int, double, EquivalenceClassCollection>[] scores 
                = new Tuple<int, double, EquivalenceClassCollection>[localAttributes.Length];

            Parallel.ForEach(rangePartitioner, options,
                (range) =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        var attributeEqClasses = EquivalenceClassCollection.Create(
                            localAttributes[i], eqClassCollection);

                        scores[i] = Tuple.Create<int, double, EquivalenceClassCollection>(
                                localAttributes[i],
                                this.GetSplitScore(attributeEqClasses, currentScore),
                                attributeEqClasses);
                    }
                });

            double max = Double.MinValue;
            int maxIndex = -1;
            for (int i = 0; i < scores.Length; i++)
            {
                if(max < scores[i].Item2)
                {
                    max = scores[i].Item2;
                    maxIndex = i;
                }
            }
            return scores[maxIndex];
        }

        protected virtual double GetCurrentScore(EquivalenceClassCollection eqClassCollection)
        {
            throw new NotImplementedException();
        }

        protected virtual double GetSplitScore(EquivalenceClassCollection attributeEqClasses, double currentScore)
        {
            throw new NotImplementedException();
        }

        private decimal MeasureSum(ITreeNode node)
        {
            decimal sum = 0;
            TreeNodeTraversal.TraversePostOrder(node, n => 
            {
                if (n.IsLeaf)
                    sum += n.Measure;
            });
            return sum;
        }
    }

    /// <summary>
    /// ID3 Tree Implementation
    /// </summary>
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

        protected override double GetCurrentScore(EquivalenceClassCollection eqClassCollection)
        {
            double entropy = 0;
            foreach (long dec in this.Decisions)
            {
                decimal decWeightedProbability = eqClassCollection.CountWeightDecision(dec);
                double p = (double)(decWeightedProbability / eqClassCollection.WeightSum);
                if (p != 0)
                    entropy -= p * System.Math.Log(p, 2);
            }
            return entropy;
        }
    }

    /// <summary>
    /// C4.5 Tree Implemetation
    /// </summary>
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
    /// Implementation is based on the following example http://csucidatamining.weebly.com/assign-4.html
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

        protected override double GetCurrentScore(EquivalenceClassCollection eqClassCollection)
        {
            double s2 = 0;
            foreach (long dec in this.Decisions)
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

        protected override double GetCurrentScore(EquivalenceClassCollection eqClassCollection)
        {
            return 0;
        }
    }

    public class DecisionTreeReduct : DecisionTreeRough
    {
        public string ReductFactoryKey { get; set; }
        public WeightGenerator WeightGenerator { get; set; }
        public PermutationCollection PermutationCollection { get; set; }
        public int Iterations { get; set; }

        public DecisionTreeReduct()
            : base()
        {
            this.ReductFactoryKey = ReductFactoryKeyHelper.ApproximateReductMajorityWeights;
            this.Iterations = 1;
            this.Epsilon = Decimal.Zero;
        }

        public override double Learn(DataStore data, int[] attributes)
        {
            if (this.WeightGenerator == null)
                this.WeightGenerator = new WeightGeneratorMajority(data);

            if (this.PermutationCollection == null)
                this.PermutationCollection = new PermutationCollection(this.Iterations, attributes);

            Args parms = new Args();
            parms.SetParameter(ReductGeneratorParamHelper.TrainData, data);
            parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, this.ReductFactoryKey);
            parms.SetParameter(ReductGeneratorParamHelper.WeightGenerator, this.WeightGenerator);
            parms.SetParameter(ReductGeneratorParamHelper.Epsilon, this.Epsilon);
            parms.SetParameter(ReductGeneratorParamHelper.PermutationCollection, this.PermutationCollection);
            parms.SetParameter(ReductGeneratorParamHelper.UseExceptionRules, false);
            parms.SetParameter(ReductGeneratorParamHelper.NumberOfReducts, this.Iterations);
            parms.SetParameter(ReductGeneratorParamHelper.NumberOfReductsToTest, this.Iterations);

            IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
            if (generator is ReductGeneratorMeasure)
                ((ReductGeneratorMeasure)generator).UsePerformanceImprovements = false;
            generator.Run();

            IReductStoreCollection reducts = generator.GetReductStoreCollection();
            IReductStoreCollection reductsfiltered = null;
            if (generator is ReductGeneratorMeasure)
                reductsfiltered = reducts.Filter(1, new ReductLengthComparer());
            else
                reductsfiltered = reducts.FilterInEnsemble(1, new ReductStoreLengthComparer(true));

            IReduct reduct = reductsfiltered.First().Where(r => r.IsException == false).FirstOrDefault();

            return base.Learn(data, reduct.Attributes.ToArray());
        }
    }
}