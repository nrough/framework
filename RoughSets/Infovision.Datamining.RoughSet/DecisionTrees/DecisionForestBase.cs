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
        private ClassificationResult learningResult;

        public ClassificationResult LearningResult
        {
            get
            {
                return this.learningResult;
            }

            protected set
            {
                this.learningResult = value;
            }
        }

        public int Size { get; set; }
        public int NumberOfTreeProbes { get; set; }
        public double Epsilon { get; set; }
        public long? DefaultOutput { get; set; }
        public int BagSizePercent { get; set; }
        public DataSampler DataSampler { get; set; }
        public DecisionForestVoteType VoteType { get; set; } 

        public virtual double AverageNumberOfAttributes
        {
            get
            {
                int i = 0;
                int attributeLengthSum = 0;
                foreach (var member in this)
                {
                    i++;
                    attributeLengthSum += ((DecisionTreeNode)member.Item1.Root).GetChildUniqueKeys().Count;
                    if (i >= this.Size)
                        break;
                }

                return (double)attributeLengthSum / (double)this.Size;
            }
        }               

        public DecisionForestBase()
        {
            this.Size = 100;
            this.NumberOfTreeProbes = 1;
            this.BagSizePercent = 100;
            this.Epsilon = -1.0;
            this.VoteType = DecisionForestVoteType.Unified;

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
            if (this.Epsilon >= 0.0)
                tree.Epsilon = this.Epsilon;
            return tree;
        }

        //protected virtual Tuple<T, double> LearnDecisionTree(DataStore data, int[] attributes, int iteration)
        protected virtual T LearnDecisionTree(DataStore data, int[] attributes, int iteration)
        {
            //Tuple<T, double> bestTree = null;
            T bestTree = default(T);
            int minNumberOfLeaves = int.MaxValue;
            for (int probe = 0; probe < this.NumberOfTreeProbes; probe++)
            {
                T tree = this.InitDecisionTree();
                double error = tree.Learn(data, attributes).Error;
                int numOfLeaves = DecisionTreeHelper.CountLeaves(tree.Root);

                if (numOfLeaves < minNumberOfLeaves)
                {                    
                    minNumberOfLeaves = numOfLeaves;
                    //bestTree = new Tuple<T, double>(tree, error);
                    bestTree = tree;
                }
            }
            
            return bestTree;
        }

        public virtual ClassificationResult Learn(DataStore data, int[] attributes)
        {            
            this.InitDataSampler(data);            

            Stopwatch s = new Stopwatch();
            s.Start();

            for (int iter = 0; iter < this.Size; iter++)
            {
                var baggedTuple = this.DataSampler.GetData(iter);
                DataStore baggedData = baggedTuple.Item1;
                DataStore oobData = baggedTuple.Item2;

                var decisionTree = this.LearnDecisionTree(baggedData, attributes, iter);                
                switch (this.VoteType)
                {
                    case DecisionForestVoteType.Unified:
                        this.AddTree(decisionTree, 1.0);
                        break;

                    case DecisionForestVoteType.ErrorBased:
                        double error = Classifier.DefaultClassifer.Classify(decisionTree, oobData).Error;
                        this.AddTree(decisionTree, 1.0 - error);
                        break;

                    default:
                        throw new NotImplementedException(String.Format("VoteType = {0} is not implemented", this.VoteType));
                }
                
            }

            s.Stop();

            ClassificationResult trainResult = Classifier.DefaultClassifer.Classify(this, data, data.Weights);            
            trainResult.ModelCreationTime = s.ElapsedMilliseconds;
            this.learningResult = trainResult;

            return trainResult;
        }

        public void SetClassificationResultParameters(ClassificationResult result)
        {
            result.QualityRatio = this.AverageNumberOfAttributes;
            result.EnsembleSize = this.Size;
            result.Epsilon = this.Epsilon;
            
            result.AvgTreeHeight = 0;
            result.MaxTreeHeight = 0;
            result.NumberOfRules = 0;
            foreach (var tree in this)
            {
                result.NumberOfRules += DecisionTreeBase.GetNumberOfRules(tree.Item1);
                result.MaxTreeHeight += DecisionTreeBase.GetHeight(tree.Item1);
                result.AvgTreeHeight += DecisionTreeBase.GetAvgHeight(tree.Item1);
            }

            result.NumberOfRules /= trees.Count;
            result.MaxTreeHeight /= trees.Count;
            result.AvgTreeHeight /= trees.Count;
        }

        public long Compute(DataRecordInternal record)
        {            
            var votes = new Dictionary<long, double>(System.Math.Min(this.trees.Count, this.Size));

            int i = 0;
            foreach (var member in this)
            {
                i++;
                long result = member.Item1.Compute(record);

                if (votes.ContainsKey(result))
                    votes[result] += member.Item2;
                else
                    votes.Add(result, member.Item2);                

                if (i >= this.Size)
                    break;
            }

            return votes.Count > 0 ? votes.FindMaxValueKey() : -1;
        }

        protected void AddTree(T tree, double vote)
        {
            this.trees.Add(new Tuple<T, double>(tree, vote));
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
