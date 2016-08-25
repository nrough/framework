using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Datamining;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    public class RandomForest<T> : ILearner, IEnumerable<IRandomForestMember>
        where T : IDecisionTree, new()
    {
        private List<IRandomForestMember> trees;
        private int attributeLengthSum;

        public int Size { get; set; }
        public int BagSizePercent { get; set; }
        public int NumberOfAttributesToCheckForSplit { get; set; }
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
            this.NumberOfAttributesToCheckForSplit = -1;

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

                if (this.NumberOfAttributesToCheckForSplit > 0)
                    tree.NumberOfAttributesToCheckForSplit = this.NumberOfAttributesToCheckForSplit;

                double error = tree.Learn(baggedData, attributes);
                this.AddTree(tree, error);
            }

            ClassificationResult trainResult = this.Classify(data, data.Weights);
            return 1 - trainResult.Accuracy;
        }

        protected void AddTree(IDecisionTree tree, double error)
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
                this.attributeLengthSum += ((DecisionTreeNode)member.Tree.Root).GetChildUniqueKeys().Count;
                if (i >= this.Size)
                    break;
            }

            return result;
        }
    }
}
