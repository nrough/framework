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
        int NumberOfRandomAttributes { get; set;}
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

        public double Learn(DataStore data, int[] attributes)
        {
            this.root = new DecisionTreeNode(-1, -1);
            this.decisionAttributeId = data.DataStoreInfo.DecisionFieldId;
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

        public long Compute(DataRecordInternal record)
        {
            if (this.Root == null)
                throw new InvalidOperationException("this.Root == null");

            ITreeNode current = this.Root;
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
                    var prediction = this.Compute(record);
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
                    var prediction = this.Compute(record);
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
        
        public int Size { get; set; }
        public int BagSizePercent { get; set; }
        public int NumberOfRandomAttributes { get; set; }

        public RandomForest()
        {
            this.Size = 500;
            this.BagSizePercent = 100;
            this.NumberOfRandomAttributes = -1;

            this.trees = new List<IRandomForestMember>(this.Size);            
        }

        public virtual double Learn(DataStore data, int[] attributes)
        {
            DataSampler sampler = new DataSampler(data);
            if(this.BagSizePercent != -1)
                sampler.BagSizePercent = this.BagSizePercent;
            for (int iter = 0; iter < this.Size; iter++)
            {
                DataStore baggedData = sampler.GetData(iter);
                T tree = new T();
                tree.NumberOfRandomAttributes 
                    = (this.NumberOfRandomAttributes > 0) 
                    ? this.NumberOfRandomAttributes
                    : (int)System.Math.Max(1, data.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard) * 0.1);

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
            var votes = new Dictionary<long, double>(this.trees.Count);
            foreach (var member in this)
            {
                long result = member.Tree.Compute(record);

                if (votes.ContainsKey(result))
                    votes[result] += (1 - member.Error);
                else
                    votes.Add(result, (1 - member.Error));
            }
            
            return votes.FindMaxValueKey();
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

            return result;
        }
    }

    public class RoughForest<T> : RandomForest<T>
        where T : IRandomForestTree, new()
    {
       
        private int reductLengthSum;

        public double AverageReductLength 
        {
            get
            {
                return (double) this.reductLengthSum / (double) this.Size;
            }
        }

        public decimal Epsilon { get; set; }
        public int NumberOfPermutationsPerTree { get; set; }

        public RoughForest()
            : base()
        {
            this.Epsilon = 0.2m;
            this.NumberOfPermutationsPerTree = 20;
        }

        public override double Learn(DataStore data, int[] attributes)
        {
            this.reductLengthSum = 0;
            DataSampler sampler = new DataSampler(data);
            if (this.BagSizePercent != -1)
                sampler.BagSizePercent = this.BagSizePercent;
            
            for (int iter = 0; iter < this.Size; iter++)
            {
                DataStore baggedData = sampler.GetData(iter);

                WeightGenerator weightGenerator = new WeightGeneratorMajority(baggedData);                
                PermutationCollection permuations = new PermutationGenerator(baggedData).Generate(this.NumberOfPermutationsPerTree);

                Args parms = new Args();
                parms.SetParameter(ReductGeneratorParamHelper.TrainData, baggedData);
                parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajorityWeights);
                parms.SetParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
                parms.SetParameter(ReductGeneratorParamHelper.Epsilon, this.Epsilon);
                parms.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permuations);
                parms.SetParameter(ReductGeneratorParamHelper.UseExceptionRules, false);

                IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
                generator.Run();

                IReductStoreCollection reductStoreCollection = generator.GetReductStoreCollection();
                IReductStoreCollection reductsFiltered = null;

                if (reductStoreCollection.ReductPerStore)
                    reductsFiltered = reductStoreCollection.FilterInEnsemble(1, new ReductStoreLengthComparer(false));
                else    
                    reductsFiltered = reductStoreCollection.Filter(1, new ReductLengthComparer());

                IReduct reduct = reductsFiltered.FirstOrDefault().FirstOrDefault();
                this.reductLengthSum += reduct.Attributes.Count;
                
                T tree = new T();
                tree.NumberOfRandomAttributes = -1;
                double error = tree.Learn(baggedData, reduct.Attributes.ToArray());

                this.AddTree(tree, error);
            }
            
            ClassificationResult trainResult = this.Classify(data, data.Weights);
            return 1 - trainResult.Accuracy;
        }
    }
}