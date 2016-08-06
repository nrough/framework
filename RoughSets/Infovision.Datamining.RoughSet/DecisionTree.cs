using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    public interface ILearner
    {
        double Learn(DataStore data, int[] attributes);
    }

    public interface IDecisionTree : ILearner
    {
        ITreeNode Root { get; }

        long Compute(DataRecordInternal record);

        ClassificationResult Classify(DataStore data, decimal[] weights = null);
    }

    public interface IRandomForestTree : IDecisionTree
    {
        int NumberOfRandomAttributes { get; set; }
    }

    public abstract class DecisionTree : IRandomForestTree
    {
        private DecisionTreeNode root;
        private int decisionAttributeId;

        public ITreeNode Root { get { return this.root; } }
        public int NumberOfRandomAttributes { get; set; }

        public DecisionTree()
        {
            this.root = null;
            this.decisionAttributeId = -1;
            this.NumberOfRandomAttributes = -1;
        }

        protected void Init(DataStore data)
        {
            this.root = new DecisionTreeNode(-1, -1);
            this.decisionAttributeId = data.DataStoreInfo.DecisionFieldId;
        }

        public virtual double Learn(DataStore data, int[] attributes)
        {
            this.Init(data);

            EquivalenceClassCollection eqClasscollection = EquivalenceClassCollection.Create(attributes, data, 0, data.Weights);
            this.GenerateSplits(eqClasscollection, this.root);
            ClassificationResult trainResult = this.Classify(data, data.Weights);
            return 1 - trainResult.Accuracy;
        }

        protected void CreateLeaf(DecisionTreeNode parent, long decisionValue)
        {
            parent.AddChild(new DecisionTreeNode(this.decisionAttributeId, decisionValue));
        }

        protected void GenerateSplits(EquivalenceClassCollection eqClassCollection, DecisionTreeNode parent)
        {
            if (eqClassCollection.ObjectsCount == 0 
                || eqClassCollection.Attributes.Length == 0 
                || this.NumberOfRandomAttributes == 0)
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
            if(maxAttribute == -1)
            {
                this.CreateLeaf(parent, eqClassCollection.DecisionWeights.FindMaxValueKey());
                return;
            }

            //Generate split on result
            Dictionary<long, EquivalenceClassCollection> subEqClasses = EquivalenceClassCollection.Split(eqClassCollection, maxAttribute);
            foreach (var kvp in subEqClasses)
            {
                DecisionTreeNode newNode = new DecisionTreeNode(maxAttribute, kvp.Key);
                parent.AddChild(newNode);

                this.GenerateSplits(kvp.Value, newNode);
            }
        }

        private bool IsNextNodeALeafNode(ITreeNode node)
        {
            if (node.Children == null)
                throw new InvalidOperationException("node.Children == null");

            if (node.Children.First().Key == this.decisionAttributeId)
                return true;

            return false;
        }

        private long GetDecision(ITreeNode node)
        {
            //TODO This is not very convenient as we might want to store more decision as leaves or some probabilities
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
                MaxDegreeOfParallelism = Environment.ProcessorCount
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
            if (this.NumberOfRandomAttributes != -1)
            {
                int m = System.Math.Min(localAttributes.Length, this.NumberOfRandomAttributes);
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
                            = EquivalenceClassCollection.Create(new int[] { localAttributes[i] }, eqClassCollection, 0);
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

    public interface IRandomForestMember
    {
        IRandomForestTree Tree { get; }
        double Error { get; }
    }

    public class RandomForestMember : IRandomForestMember
    {
        public IRandomForestTree Tree { get; set; }
        public double Error { get; set; }
    }

    public class RandomForest<T> : ILearner, IEnumerable<IRandomForestMember>
        where T : IRandomForestTree, new()
    {
        private List<IRandomForestMember> trees;
        private int attributeLengthSum;

        public int Size { get; set; }
        public int BagSizePercent { get; set; }
        public int NumberOfRandomAttributes { get; set; }
        public DataSampler DataSampler { get; set; }

        public virtual double AverageNumberOfAttributes
        {
            get
            {
                return (double)this.attributeLengthSum / (double)this.Size;
            }
        }

        public RandomForest()
        {
            this.Size = 500;
            this.BagSizePercent = 100;
            this.NumberOfRandomAttributes = -1;

            this.trees = new List<IRandomForestMember>(this.Size);
        }

        public virtual double Learn(DataStore data, int[] attributes)
        {
            DataSampler sampler = (this.DataSampler != null) ? this.DataSampler : new DataSampler(data);
            if (this.BagSizePercent != -1)
                sampler.BagSizePercent = this.BagSizePercent;
            
            for (int iter = 0; iter < this.Size; iter++)
            {
                DataStore baggedData = sampler.GetData(iter);
                T tree = new T();

                if (this.NumberOfRandomAttributes > -1)
                    tree.NumberOfRandomAttributes = this.NumberOfRandomAttributes;

                double error = tree.Learn(baggedData, attributes);
                this.AddTree(tree, error);
            }

            ClassificationResult trainResult = this.Classify(data, data.Weights);
            return 1 - trainResult.Accuracy;
        }

        protected void AddTree(IRandomForestTree tree, double error)
        {
            IRandomForestMember newMember = new RandomForestMember()
            {
                Tree = tree,
                Error = error
            };

            this.trees.Add(newMember);
        }

        public long Compute(DataRecordInternal record)
        {
            int i = 0;
            var votes = new Dictionary<long, double>(this.trees.Count);
            foreach (var member in this)
            {
                i++;
                long result = member.Tree.Compute(record);

                if (votes.ContainsKey(result))
                    votes[result] += (1 - member.Error);
                else
                    votes.Add(result, (1 - member.Error));

                if (i >= this.Size)
                    break;
            }

            return votes.Count > 0 ? votes.FindMaxValueKey() : -1;
        }

        public IEnumerator<IRandomForestMember> GetEnumerator()
        {
            return this.trees.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        //TODO Move to some global blace this method will always be used
        public ClassificationResult Classify(DataStore testData, decimal[] weights = null)
        {
            ClassificationResult result = new ClassificationResult(testData, testData.DataStoreInfo.GetDecisionValues());

            result.QualityRatio = this.AverageNumberOfAttributes;
            result.EnsembleSize = this.Size;

            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
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
                    result.AddResult(objectIndex, prediction, record[testData.DataStoreInfo.DecisionFieldId], (double)weights[objectIndex]);
                }
                );
            }

            int i = 0;
            this.attributeLengthSum = 0;
            foreach (var member in this)
            {
                i++;
                this.attributeLengthSum += ((DecisionTreeNode)member.Tree.Root).GetChildUniqueKeys().Count;
                if (i >= this.Size)
                    break;
            }

            return result;
        }
    }

    public class RoughForest<T> : RandomForest<T>
        where T : IRandomForestTree, new()
    {        
        private Dictionary<int, int> attributeCount;

        public decimal Epsilon { get; set; }
        public int NumberOfPermutationsPerTree { get; set; }
        public string ReductGeneratorFactory { get; set; }
        public virtual PermutationCollection PermutationCollection { get; set; }

        public RoughForest()
            : base()
        {
            this.attributeCount = new Dictionary<int, int>();
            this.Epsilon = Decimal.MinValue;
            this.NumberOfPermutationsPerTree = 20;
            this.ReductGeneratorFactory = ReductFactoryKeyHelper.ApproximateReductMajorityWeights;
        }

        protected virtual IReduct CalculateReduct(DataStore data)
        {
            PermutationCollection permutations = null;
            if(this.PermutationCollection != null)
                permutations = this.PermutationCollection;
            else
                permutations = new PermutationGenerator(data).Generate(this.NumberOfPermutationsPerTree);

            decimal localEpsilon = Decimal.MinValue;
            if (this.Epsilon >= 0)
                localEpsilon = this.Epsilon;
            else
                localEpsilon = (decimal)((double)RandomSingleton.Random.Next(0, 20) / 100.0);

            Args parms = new Args(5);
            parms.SetParameter(ReductGeneratorParamHelper.TrainData, data);
            parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, this.ReductGeneratorFactory);
            parms.SetParameter(ReductGeneratorParamHelper.Epsilon, localEpsilon);
            parms.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permutations);
            parms.SetParameter(ReductGeneratorParamHelper.UseExceptionRules, false);

            IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
            generator.Run();

            IReductStoreCollection reductStoreCollection = generator.GetReductStoreCollection();

            int bestScore = Int32.MaxValue;
            IReduct bestReduct = null;

            List<IReduct> reducts = new List<IReduct>(this.NumberOfPermutationsPerTree);
            foreach (var store in reductStoreCollection)
                foreach (var reduct in store)
                {
                    reducts.Add(reduct);
                }

            //reducts.Sort(new ReductLengthComparer());
            reducts.Sort(new ReductRuleNumberComparer());

            foreach (var reduct in reducts)
            {
                int count = 0;
                foreach (int attr in reduct.Attributes)
                {
                    if (this.attributeCount.ContainsKey(attr))
                        count += this.attributeCount[attr];
                }

                if (count < bestScore)
                {
                    bestScore = count;
                    bestReduct = reduct;
                }

                if (bestScore == 0)
                    break;
            }
            return bestReduct;
        }

        public override double Learn(DataStore data, int[] attributes)
        {
            DataSampler sampler = (this.DataSampler != null) ? this.DataSampler : new DataSampler(data);
            if (this.BagSizePercent != -1)
                sampler.BagSizePercent = this.BagSizePercent;

            for (int iter = 0; iter < this.Size; iter++)
            {
                DataStore baggedData = sampler.GetData(iter);

                IReduct reduct = this.CalculateReduct(baggedData);
                foreach (int attr in reduct.Attributes)
                {
                    if (!this.attributeCount.ContainsKey(attr))
                        this.attributeCount[attr] = 1;
                    else
                        this.attributeCount[attr] += 1;
                }

                T tree = new T();
                tree.NumberOfRandomAttributes = -1;
                double error = tree.Learn(baggedData, reduct.Attributes.ToArray());
                this.AddTree(tree, error);
            }

            ClassificationResult trainResult = this.Classify(data, data.Weights);
            return 1 - trainResult.Accuracy;
        }
    }

    public class DummyForest<T> : RoughForest<T>
        where T : IRandomForestTree, new()
    {
        protected bool firstReduct = true;
        protected int[][] attributes = null;
        protected int localIterationNum = 0;

        protected override IReduct CalculateReduct(DataStore data)
        {
            if(this.firstReduct == true)
            {
                if(this.PermutationCollection != null)
                {
                    this.attributes = new int[this.PermutationCollection.Count][];
                    int i = 0;
                    foreach(var permutation in this.PermutationCollection)
                        this.attributes[i++] = permutation.ToArray();
                }
                else
                {
                    this.attributes = new int[this.Size][];
                    for(int i=0; i<this.Size; i++)
                    {
                        this.attributes[i] = data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray();
                        int len = this.attributes[i].Length;
                        this.attributes[i] = this.attributes[i].RandomSubArray(RandomSingleton.Random.Next(1, len));
                    }
                }

                this.firstReduct = false;
            }

            IReduct reduct = new ReductWeights(data, this.attributes[this.localIterationNum], 0, data.Weights);
            this.localIterationNum++;

            return reduct;
        }

        public override double Learn(DataStore data, int[] attributes)
        {
 	        double result = base.Learn(data, attributes);

            this.firstReduct = true;
            this.localIterationNum = 0;

            return result;
        }
    }

    public class SemiRoughForest<T> : DummyForest<T>
        where T : IRandomForestTree, new()
    {
        protected override IReduct CalculateReduct(DataStore data)
        {
            if(this.firstReduct == true)
            {
                if(this.PermutationCollection != null)
                {
                    this.attributes = new int[this.PermutationCollection.Count][];
                    int i = 0;
                    foreach(var permutation in this.PermutationCollection)
                        this.attributes[i++] = permutation.ToArray();
                }
                else
                {
                    this.attributes = new int[this.Size][];
                    for(int i=0; i<this.Size; i++)
                    {
                        this.attributes[i] = data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray();
                        int len = this.attributes[i].Length;
                        this.attributes[i] = this.attributes[i].RandomSubArray(RandomSingleton.Random.Next(1, len));
                    }
                }

                this.firstReduct = false;
            }

            PermutationCollection permutations = new PermutationCollection(new Permutation(this.attributes[this.localIterationNum]));
            IReduct r = new ReductWeights(data, this.attributes[this.localIterationNum], 0, data.Weights);
            InformationMeasureWeights m = new InformationMeasureWeights();
            decimal q = m.Calc(r);

            Args parms = new Args(6);
            parms.SetParameter<DataStore>(ReductGeneratorParamHelper.TrainData, data);
            parms.SetParameter<string>(ReductGeneratorParamHelper.FactoryKey, this.ReductGeneratorFactory);
            parms.SetParameter<decimal>(ReductGeneratorParamHelper.Epsilon, this.Epsilon);
            parms.SetParameter<PermutationCollection>(ReductGeneratorParamHelper.PermutationCollection, permutations);
            parms.SetParameter<bool>(ReductGeneratorParamHelper.UseExceptionRules, false);
            parms.SetParameter<decimal>(ReductGeneratorParamHelper.DataSetQuality, q);

            IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
            generator.Run();

            IReductStoreCollection reductStoreCollection = generator.GetReductStoreCollection();
            IReduct reduct = reductStoreCollection.First().First();

            this.localIterationNum++;

            return reduct;
        }
    }
}