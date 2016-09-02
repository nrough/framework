using Infovision.Data;
using Infovision.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    public abstract class DecisionForestBase<T> : ILearner, IPredictionModel, IEnumerable<Tuple<T, double>>
        where T : IDecisionTree, new()
    {
        private List<Tuple<T, double>> trees;
        private int attributeLengthSum;
        KeyValuePair<T, double> test;
        public int Size { get; set; }
        public int NumberOfTreeProbes { get; set; }
        public decimal Epsilon { get; set; }
        public int BagSizePercent { get; set; }
        public DataSampler DataSampler { get; set; }
        public virtual double AverageNumberOfAttributes
        {
            get { return (double)this.attributeLengthSum / (double)this.Size; }
        }
        public virtual double QualityRatio { get { return this.AverageNumberOfAttributes; } }
        public virtual int EnsembleSize { get { return this.Size; } }

        public DecisionForestBase()
        {
            this.Size = 100;
            this.NumberOfTreeProbes = 20;
            this.BagSizePercent = 100;
            this.Epsilon = Decimal.MinValue;

            this.trees = new List<Tuple<T, double>>(this.Size);
        }

        protected virtual void InitDataSampler(DataStore data)
        {
            if (this.DataSampler == null)
            {
                this.DataSampler = new DataSampler(data);
                if (this.BagSizePercent != -1)
                    this.DataSampler.BagSizePercent = this.BagSizePercent;
            }
        }

        protected virtual T InitDecisionTree()
        {
            T tree = new T();

            if (this.Epsilon >= Decimal.Zero)
                tree.Epsilon = this.Epsilon;

            return tree;
        }

        protected virtual Tuple<T, double> LearnDecisionTree(DataStore data, int[] attributes, int iteration)
        {
            Tuple<T, double> bestTree = null;
            int minNumberOfLeaves = int.MaxValue;
            for (int probe = 0; probe < this.NumberOfTreeProbes; probe++)
            {
                T tree = this.InitDecisionTree();

                double error = tree.Learn(data, attributes);
                int numOfLeaves = DecisionTreeHelper.CountLeaves(tree.Root);
                if (numOfLeaves < minNumberOfLeaves)
                {
                    minNumberOfLeaves = numOfLeaves;
                    bestTree = new Tuple<T, double>(tree, error);
                }
            }
            
            return bestTree;
        }

        public virtual double Learn(DataStore data, int[] attributes)
        {
            this.InitDataSampler(data);

            Stopwatch s = new Stopwatch();
            s.Start();  

            for (int iter = 0; iter < this.Size; iter++)
            {
                DataStore baggedData = this.DataSampler.GetData(iter);
                this.AddTree(this.LearnDecisionTree(baggedData, attributes, iter));
            }

            s.Stop();

            int i = 0;
            this.attributeLengthSum = 0;
            foreach (var member in this)
            {
                i++;
                this.attributeLengthSum += ((DecisionTreeNode)member.Item1.Root).GetChildUniqueKeys().Count;
                if (i >= this.Size)
                    break;
            }

            ClassificationResult trainResult = Classifier.Instance.Classify(this, data, data.Weights);
            //this.Classify(data, data.Weights);
            trainResult.ModelCreationTime = s.ElapsedMilliseconds;
            return 1 - trainResult.Accuracy;
        }

        
        /*
        public ClassificationResult Classify(DataStore testData, decimal[] weights = null)
        {
            ClassificationResult result = new ClassificationResult(testData, testData.DataStoreInfo.GetDecisionValues());

            result.QualityRatio = this.AverageNumberOfAttributes;
            result.EnsembleSize = this.Size;

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
                    result.AddResult(objectIndex, prediction, record[testData.DataStoreInfo.DecisionFieldId], (double)weights[objectIndex]);
                }
                );
            }

            int i = 0;
            this.attributeLengthSum = 0;
            foreach (var member in this)
            {
                i++;
                this.attributeLengthSum += ((DecisionTreeNode)member.Item1.Root).GetChildUniqueKeys().Count;
                if (i >= this.Size)
                    break;
            }

            return result;
        }
        */

        public long Compute(DataRecordInternal record)
        {
            int i = 0;
            var votes = new Dictionary<long, double>(this.trees.Count);
            foreach (var member in this)
            {
                i++;
                long result = member.Item1.Compute(record);

                if (votes.ContainsKey(result))
                    votes[result] += (1 - member.Item2);
                else
                    votes.Add(result, (1 - member.Item2));

                if (i >= this.Size)
                    break;
            }

            return votes.Count > 0 ? votes.FindMaxValueKey() : -1;
        }

        protected void AddTree(T tree, double error)
        {
            this.trees.Add(new Tuple<T, double>(tree, error));
        }

        protected void AddTree(Tuple<T, double> tree)
        {
            this.trees.Add(tree);
        }

        public IEnumerator<Tuple<T, double>> GetEnumerator()
        {
            return this.trees.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
