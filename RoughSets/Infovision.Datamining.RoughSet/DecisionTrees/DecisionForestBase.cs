﻿using Infovision.Data;
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
        public int BagSizePercent { get; set; }
        public DataSampler DataSampler { get; set; }

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

        public virtual double QualityRatio { get { return this.AverageNumberOfAttributes; } }
        public virtual int EnsembleSize { get { return this.Size; } }

        public DecisionForestBase()
        {
            this.Size = 100;
            this.NumberOfTreeProbes = 20;
            this.BagSizePercent = 100;
            this.Epsilon = Double.MinValue;

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

        protected virtual Tuple<T, double> LearnDecisionTree(DataStore data, int[] attributes, int iteration)
        {
            Tuple<T, double> bestTree = null;
            int minNumberOfLeaves = int.MaxValue;
            for (int probe = 0; probe < this.NumberOfTreeProbes; probe++)
            {
                T tree = this.InitDecisionTree();
                double error = 1.0 - tree.Learn(data, attributes).Accuracy;
                int numOfLeaves = DecisionTreeHelper.CountLeaves(tree.Root);

                if (numOfLeaves < minNumberOfLeaves)
                {                    
                    minNumberOfLeaves = numOfLeaves;
                    bestTree = new Tuple<T, double>(tree, error);
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
                DataStore baggedData = this.DataSampler.GetData(iter);
                this.AddTree(this.LearnDecisionTree(baggedData, attributes, iter));
            }

            s.Stop();
          
            ClassificationResult trainResult = Classifier.Instance.Classify(this, data, data.Weights);            
            trainResult.ModelCreationTime = s.ElapsedMilliseconds;
            this.learningResult = trainResult;

            return trainResult;
        }              

        public long Compute(DataRecordInternal record)
        {
            int i = 0;
            var votes = new Dictionary<long, double>(System.Math.Min(this.trees.Count, this.Size));
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