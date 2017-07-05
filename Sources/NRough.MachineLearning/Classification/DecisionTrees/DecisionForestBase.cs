// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

using NRough.Data;
using NRough.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRough.MachineLearning.Classification.Ensembles;
using NRough.Core.CollectionExtensions;

namespace NRough.MachineLearning.Classification.DecisionTrees
{
    public abstract class DecisionForestBase<TTree> : EnsembleBase, ILearner, IClassificationModel
        where TTree : IDecisionTree, new()
    {
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

        public int NumberOfTreeProbes { get; set; }
        public double Gamma { get; set; }
        public int BagSizePercent { get; set; }
        public DataSampler DataSampler { get; set; }
        public DecisionForestVoteType VoteType { get; set; } 

        public virtual double AverageNumberOfAttributes
        {
            get
            {
                int i = 0;
                int attributeLengthSum = 0;
                foreach (var weakClassifier in weakClassifiers)
                {
                    i++;
                    IDecisionTree tree = weakClassifier.Model as IDecisionTree;
                    attributeLengthSum += ((DecisionTreeNode)tree.Root).GetChildUniqueKeys().Count;
                    if (i >= this.Size)
                        break;
                }

                return (double)attributeLengthSum / (double)this.Size;
            }
        }               

        public DecisionForestBase()
            : base()
        {
            Init();
        }

        public DecisionForestBase(string modelName)
            : base(modelName)
        {
            Init();
        }

        private void Init()
        {
            Iterations = 100;
            Size = 100;
            NumberOfTreeProbes = 1;
            BagSizePercent = 100;
            Gamma = -1.0;
            VoteType = DecisionForestVoteType.Unified;
        }

        public override void Reset()
        {
            base.Reset();

            this.learningResult = null;
            this.DefaultOutput = null;
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

        protected virtual TTree InitDecisionTree()
        {
            TTree tree = new TTree();
            if (Gamma >= 0.0)
                tree.Gamma = Gamma;

            if (DefaultOutput != null)
                tree.DefaultOutput = DefaultOutput;

            return tree;
        }

        protected virtual TTree LearnDecisionTree(DataStore data, int[] attributes, int iteration)
        {
            TTree bestTree = default(TTree);
            int minNumberOfLeaves = int.MaxValue;
            for (int probe = 0; probe < this.NumberOfTreeProbes; probe++)
            {
                TTree tree = this.InitDecisionTree();
                double error = tree.Learn(data, attributes).Error;
                int numOfLeaves = DecisionTreeHelper.CountLeaves(tree.Root);

                if (numOfLeaves < minNumberOfLeaves)
                {                    
                    minNumberOfLeaves = numOfLeaves;
                    bestTree = tree;
                }
            }
            
            return bestTree;
        }

        public override ClassificationResult Learn(DataStore data, int[] attributes)
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
                        double error = Classifier.Default.Classify(decisionTree, oobData).Error;
                        this.AddTree(decisionTree, 1.0 - error);
                        break;

                    default:
                        throw new NotImplementedException(String.Format("VoteType = {0} is not implemented", this.VoteType));
                }
                
            }

            s.Stop();

            ClassificationResult trainResult = Classifier.Default.Classify(this, data, data.Weights);            
            trainResult.ModelCreationTime = s.ElapsedMilliseconds;
            this.learningResult = trainResult;

            return trainResult;
        }

        public override void SetClassificationResultParameters(ClassificationResult result)
        {
            base.SetClassificationResultParameters(result);

            result.AvgNumberOfAttributes = this.AverageNumberOfAttributes;
            result.EnsembleSize = this.Size;
            result.Epsilon = this.Gamma;
            
            result.AvgTreeHeight = 0;
            result.MaxTreeHeight = 0;
            result.NumberOfRules = 0;
            foreach (var weakClassifier in weakClassifiers)
            {
                IDecisionTree tree = weakClassifier.Model as IDecisionTree;
                result.NumberOfRules += DecisionTreeMetric.GetNumberOfRules(tree);
                result.MaxTreeHeight += DecisionTreeMetric.GetHeight(tree);
                result.AvgTreeHeight += DecisionTreeMetric.GetAvgHeight(tree);
            }

            result.NumberOfRules /= weakClassifiers.Count;
            result.MaxTreeHeight /= weakClassifiers.Count;
            result.AvgTreeHeight /= weakClassifiers.Count;
        }

        public override long Compute(DataRecordInternal record)
        {            
            var votes = new Dictionary<long, double>(System.Math.Min(weakClassifiers.Count, this.Size));

            int i = 0;
            foreach (var weakClassifier in weakClassifiers)
            {
                i++;
                long result = weakClassifier.Model.Compute(record);

                if (votes.ContainsKey(result))
                    votes[result] += weakClassifier.Weight;
                else
                    votes.Add(result, weakClassifier.Weight);                

                if (i >= this.Size)
                    break;
            }

            return votes.Count > 0 ? votes.FindMaxValueKey() : Classifier.UnclassifiedOutput;
        }

        protected void AddTree(TTree tree, double vote)
        {
            this.AddClassfier((IClassificationModel)tree, vote);
        }
    }
}
