﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;
using Infovision.Datamining.Roughset.DecisionTrees.Pruning;
using System.Diagnostics;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    public delegate int[] AttributeSelectionMethod(DataStore data, int[] attributes);
    
    /// <summary>
    /// Base class for decision tree implementations
    /// </summary>
    [Serializable]
    public abstract class DecisionTreeBase : ModelBase, IDecisionTree, IPredictionModel, ICloneable
    {        
        protected Dictionary<int, List<long>> thresholds = null;
        protected DecisionDistribution aprioriDistribution = null;

        private DecisionTreeNode root = null;
        private int decisionAttributeId = -1;
        private double mA = 1.0;
        private long[] decisions;
        private int nextId = 1;        

        private readonly object syncRoot = new object();

        public IDecisionTreeNode Root
        {
            get { return this.root; }
            protected set
            {
                DecisionTreeNode localRoot = value as DecisionTreeNode;
                if (localRoot != null)
                    this.root = localRoot;
                else
                    throw new InvalidOperationException("Root object must be DecisionTreeNode class or its extension");
            }
        }

        public int NumberOfAttributesToCheckForSplit { get; set; } = -1;
        public long? DefaultOutput { get; set; } = null;
        public bool UseLocalOutput { get; set; } = false;
        public double Gamma { get; set; } = -1.0;
        public int MaxHeight { get; set; } = -1;
        public int MinimumNumOfInstancesPerLeaf { get; set; } = 1;
        public DataStore TrainingData { get; protected set; }

        public ImpurityFunc ImpurityFunction { get; set; } = ImpurityFunctions.Entropy;
        public ImpurityNormalizeFunc ImpurityNormalize { get; set; } = ImpurityFunctions.DummyNormalize;
        public AttributeSelectionMethod AttributeSelection { get; set; } = DecisionTreeBase.DefaultAttributeSelection;

        public PruningType PruningType { get; set; } = PruningType.None;
        public int PruningCVFolds { get; set; } = 3;
        public PruningObjectiveType PruningObjective { get; set; } = PruningObjectiveType.MinimizeError;

        protected IEnumerable<long> Decisions { get { return this.decisions; } }        

        protected class SplitInfo
        {
            public static readonly SplitInfo NoSplit = new SplitInfo(
                -1, Double.NegativeInfinity, null, SplitType.None, ComparisonType.None, 0);

            public int AttributeId { get; set; }
            public double Gain { get; set; }
            public EquivalenceClassCollection EquivalenceClassCollection { get; set; }
            public ComparisonType ComparisonType { get; set; }
            public long Cut { get; set; }
            public SplitType SplitType { get; set; }
            
            public SplitInfo(int attributeId, 
                double gain, 
                EquivalenceClassCollection equivalenceClassCollection, 
                SplitType splitType,
                ComparisonType comparisonType,
                long cut)
            {
                this.AttributeId = attributeId;
                this.Gain = gain;
                this.EquivalenceClassCollection = equivalenceClassCollection;
                this.SplitType = splitType;
                this.ComparisonType = comparisonType;
                this.Cut = cut;
            }
        }
        
        public DecisionTreeBase()
            : base()
        {            
        }

        public DecisionTreeBase(string modelName)
            : base(modelName)
        {                        
        }        

        public virtual void Reset()
        {
            this.root = null;
            this.nextId = 1;
            this.thresholds = null;
            this.aprioriDistribution = null;
            this.mA = 0;
            this.decisions = null;
        }

        protected int GetId()
        {
            lock (syncRoot)
            {
                return this.nextId++;
            }
        }

        protected void Init(DataStore data, int[] attributes)
        {
            lock (syncRoot)
            {
                this.Reset();

                this.TrainingData = data;
                this.root = new DecisionTreeNode(-1, -1, ComparisonType.EqualTo, -1, null);
                this.decisionAttributeId = data.DataStoreInfo.DecisionFieldId;
                this.decisions = new long[data.DataStoreInfo.GetDecisionValues().Count];

                int i = 0;
                foreach (long decisionValue in data.DataStoreInfo.GetDecisionValues())
                    this.decisions[i++] = decisionValue;

                if (this.Gamma >= 0.0)
                    this.mA = InformationMeasureWeights.Instance.Calc(
                        EquivalenceClassCollection.Create(attributes, data, data.Weights));

                this.thresholds = new Dictionary<int, List<long>>();               
                foreach (DataFieldInfo field in attributes
                    .Select(k => data.DataStoreInfo.GetFieldInfo(k))
                    .Where(f => f.IsNumeric))
                {                                        
                    var thresholdList = new List<long>();

                    long[] values = this.TrainingData.GetColumnInternal(field.Id);
                    Array.Sort(values);

                    for (int k = 0; k < values.Length - 1; k++)
                        if (values[k] != values[k + 1])
                            thresholdList.Add((values[k] + values[k + 1]) / 2);

                    this.thresholds[field.Id] = thresholdList;
                }

                this.aprioriDistribution = EquivalenceClassCollection.Create(new int[] { }, data, data.Weights).DecisionDistribution;
                if(this.DefaultOutput == null)
                    this.DefaultOutput = this.aprioriDistribution.Output;
            }
        }

        protected virtual void CleanUp()
        {
            this.thresholds = null;
        }

        public virtual ClassificationResult Learn(DataStore data, int[] attributes)
        {
            if (this.PruningType != PruningType.None)
                return this.LearnAndPrune(data, attributes);

            this.Init(data, attributes);
            EquivalenceClassCollection eqClassCollection = EquivalenceClassCollection.Create(new int[] { }, data, data.Weights);
            if (this.Gamma >= 0.0)
                this.root.Measure = InformationMeasureWeights.Instance.Calc(eqClassCollection);
            this.GrowTree(eqClassCollection, this.root, attributes);

            this.CleanUp();

            return Classifier.DefaultClassifer.Classify(this, data, data.Weights);
        }

        private void CheckPruningConditions()
        {
            if (this.PruningCVFolds <= 1)
                throw new InvalidOperationException("this.PruningCVFolds <= 1");
            if (this.PruningObjective == PruningObjectiveType.None)
                throw new InvalidOperationException("this.PruningObjective == PruningObjectiveType.None");
            if (this.PruningCVFolds > this.TrainingData.NumberOfRecords)
                throw new InvalidOperationException("this.PruningCVFolds > data.NumberOfRecords");
        }

        private ClassificationResult LearnAndPrune(DataStore data, int[] attributes)
        {
            this.Init(data, attributes);

            this.CheckPruningConditions();

            DataStoreSplitter cvSplitter = new DataStoreSplitter(data, this.PruningCVFolds, false);
            DataStore trainSet = null, pruningSet = null;
            IDecisionTree bestModel = null;

            int bestNumOfRules = Int32.MaxValue;
            double bestError = Double.PositiveInfinity;
            int bestMaxHeight = Int32.MaxValue;
            double bestAvgHeight = Double.PositiveInfinity;

            for (int f = 0; f < this.PruningCVFolds; f++)
            {
                cvSplitter.Split(ref trainSet, ref pruningSet, f);
                
                var tmpTree = (DecisionTreeBase) this.Clone();                                
                tmpTree.PruningType = PruningType.None;
                tmpTree.Learn(trainSet, attributes);
                
                IDecisionTreePruning pruningMethod = DecisionTreePruningBase.Construct(this.PruningType, tmpTree, pruningSet); 
                pruningMethod.Prune();

                switch (this.PruningObjective)
                {
                    case PruningObjectiveType.MinimizeNumberOfLeafs:
                        int numOfRules = DecisionTreeBase.GetNumberOfRules(tmpTree);
                        if (numOfRules < bestNumOfRules)
                        {
                            bestNumOfRules = numOfRules;
                            bestModel = tmpTree;
                        }
                        break;

                    case PruningObjectiveType.MinimizeTreeMaxHeight:
                        int maxHeight = DecisionTreeBase.GetHeight(tmpTree);
                        if (maxHeight < bestMaxHeight)
                        {
                            bestMaxHeight = maxHeight;
                            bestModel = tmpTree;
                        }
                        break;

                    case PruningObjectiveType.MinimizeError:
                        ClassificationResult tmpResult = Classifier.DefaultClassifer.Classify(tmpTree, pruningSet);
                        if (tmpResult.Error < bestError)
                        {
                            bestError = tmpResult.Error;
                            bestModel = tmpTree;
                        }
                        break;

                    case PruningObjectiveType.MinimizeTreeAvgHeight:
                        double avgHeight = DecisionTreeBase.GetAvgHeight(tmpTree);
                        if (avgHeight < bestAvgHeight)
                        {
                            bestAvgHeight = avgHeight;
                            bestModel = tmpTree;
                        }
                        break;

                    default:
                        throw new NotImplementedException(String.Format("Pruning objective type {0} is not implemented", this.PruningObjective));
                }
            }

            this.Root = bestModel.Root;
            this.CleanUp();
            return Classifier.DefaultClassifer.Classify(this, data);
        }

        public virtual void SetClassificationResultParameters(ClassificationResult result)
        {
            result.AvgNumberOfAttributes = this.Root != null ? ((DecisionTreeNode)this.Root).GetChildUniqueKeys().Count : 0;
            result.EnsembleSize = 1;
            result.Gamma = this.Gamma;
            result.MaxTreeHeight = DecisionTreeBase.GetHeight(this);
            result.AvgTreeHeight = DecisionTreeBase.GetAvgHeight(this);
            result.NumberOfRules = DecisionTreeBase.GetNumberOfRules(this);
            result.ModelName = this.ModelName;
        }        

        protected void SetNodeOutput(DecisionTreeNode node, Dictionary<long, double> outputWeights)
        {         
            node.OutputDistribution = new DecisionDistribution(outputWeights);
        }

        protected virtual bool CheckStopCondition(EquivalenceClassCollection currentEqClassCollection, int[] currentAttributes, ref bool isConverged, IDecisionTreeNode parentNode)
        {
            if (isConverged)
                return true;

            if (currentEqClassCollection.NumberOfObjects <= this.MinimumNumOfInstancesPerLeaf
                || currentEqClassCollection.NumberOfObjects == 0)
                return true;           

            if (currentAttributes.Length == 0)
                return true;

            if(this.MaxHeight != -1 && this.MaxHeight <= parentNode.Level)
                return true;

            if (currentEqClassCollection.HasSingleDecision())
                return true;

            if (this.Gamma >= 0.0)
            {                
                if (((1.0 - this.Gamma) * this.mA) <= MeasureSum(this.root))
                {
                    isConverged = true;
                    return true;
                }
            }

            return false;
        }

        protected virtual void GrowTree(EquivalenceClassCollection eqClassCollection, DecisionTreeNode parent, int[] attributes)
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
                                
                this.SetNodeOutput(currentParent, currentEqClassCollection.DecisionWeights);

                if (this.CheckStopCondition(currentEqClassCollection, currentAttributes, ref isConverged, currentParent))
                    continue;

                var nextSplit = this.GetNextSplit(currentEqClassCollection, attributes, currentAttributes, currentParent);
                
                if (nextSplit.SplitType == SplitType.None)
                    continue;

                var subEqClasses = EquivalenceClassCollection.Split(nextSplit.AttributeId, nextSplit.EquivalenceClassCollection);
                
                if (nextSplit.SplitType == SplitType.Binary && subEqClasses.Count != 2)
                    throw new InvalidOperationException("Binary split must have two branches.");

                //RemoveValue makes a copy of the array
                currentAttributes = currentAttributes.RemoveValue(nextSplit.AttributeId);
                bool binarySplitFlag = true;
                foreach (var kvp in subEqClasses)
                {                    
                    DecisionTreeNode newNode = null;
                    switch (nextSplit.SplitType)
                    {
                        case SplitType.Discreet:
                            newNode = new DecisionTreeNode(this.GetId(), nextSplit.AttributeId, nextSplit.ComparisonType, kvp.Key, currentParent);
                            break;

                        case SplitType.Binary:
                            newNode = new DecisionTreeNode(this.GetId(), nextSplit.AttributeId,
                                binarySplitFlag ? nextSplit.ComparisonType : nextSplit.ComparisonType.Complement(), 
                                nextSplit.Cut, currentParent);
                            binarySplitFlag = false;
                            break;

                        default:
                            throw new NotImplementedException(String.Format("Split type {0} is not implemented", nextSplit.SplitType));
                    }
                    
                    currentParent.AddChild(newNode);
                    
                    if (this.Gamma >= 0.0)
                        newNode.Measure = InformationMeasureWeights.Instance.Calc(kvp.Value);

                    var newSplitInfo = Tuple.Create<EquivalenceClassCollection, DecisionTreeNode, int[]>(
                            kvp.Value, 
                            newNode, 
                            currentAttributes);

                    queue.Enqueue(newSplitInfo);
                }
            }
        }

        public virtual long Compute(DataRecordInternal record)
        {
            if (this.Root == null)
                throw new InvalidOperationException("this.Root == null");

            IDecisionTreeNode current = this.Root;
            while (current != null)
            {
                if (current.IsLeaf)
                {
                    return current.Output;
                }

                IDecisionTreeNode next = current.Children.Where(x => x.Compute(record[x.Attribute])).FirstOrDefault();

                if (next == null && this.UseLocalOutput)
                    return current.Output;

                current = next;                
            }

            return -1; //unclassified
        }
        
        protected virtual SplitInfo GetNextSplit(EquivalenceClassCollection eqClassCollection, int[] origAttributes, int[] attributesToTest, IDecisionTreeNode parentTreeNode)
        {            
            int[] localAttributes = attributesToTest;

            if (this.NumberOfAttributesToCheckForSplit != -1)
            {
                int m = System.Math.Min(attributesToTest.Length, this.NumberOfAttributesToCheckForSplit);
                localAttributes = attributesToTest.RandomSubArray(m);
            }

            object tmpLock = new object();
            var rangePartitioner = Partitioner.Create(
                0, 
                localAttributes.Length, 
                System.Math.Max(1, localAttributes.Length / InfovisionConfiguration.MaxDegreeOfParallelism));
            
            double currentScore = this.CalculateImpurityBeforeSplit(eqClassCollection);
            SplitInfo[] scores = new SplitInfo[localAttributes.Length];

            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = InfovisionConfiguration.MaxDegreeOfParallelism
            };

            Parallel.ForEach(rangePartitioner, options, (range) =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                    scores[i] = this.GetSplitInfo(localAttributes[i], eqClassCollection, currentScore, parentTreeNode);
            });            

            double maxGain = Double.NegativeInfinity;
            int maxIndex = -1;
            for (int i = 0; i < scores.Length; i++)
            {
                //select best gain among attributes with more than a single value
                if (maxGain < scores[i].Gain 
                    && scores[i].EquivalenceClassCollection.Count > 1)
                {
                    maxGain = scores[i].Gain;
                    maxIndex = i;
                }
            }

            if (maxIndex == -1)
                return SplitInfo.NoSplit;

            return scores[maxIndex];
        }

        protected virtual double CalculateImpurityAfterSplit(EquivalenceClassCollection equivalenceClasses)
        {
            return this.ImpurityFunction(equivalenceClasses);
        }

        protected virtual double CalculateImpurityBeforeSplit(EquivalenceClassCollection equivalenceClasses)
        {
            if (equivalenceClasses.Count != 1)
                throw new ArgumentException("eqClassCollection.Count != 1", "eqClassCollection");

            return this.ImpurityFunction(equivalenceClasses);
        }

        protected virtual SplitInfo GetSplitInfo(int attributeId, EquivalenceClassCollection data, double currentScore, IDecisionTreeNode parentTreeNode)
        {
            DataFieldInfo attributeInfo = this.TrainingData.DataStoreInfo.GetFieldInfo(attributeId);

            if (attributeInfo.IsSymbolic)
            {
                return this.GetSplitInfoSymbolic(attributeId, data, currentScore, parentTreeNode);
            }
            else if (attributeInfo.IsNumeric)
            {
                return this.GetSplitInfoNumeric(attributeId, data, currentScore, parentTreeNode);
            }
            
            throw new NotImplementedException("Only symbolic and numeric attribute types are currently supported.");                            
        }
            
        protected virtual SplitInfo GetSplitInfoSymbolic(int attributeId, EquivalenceClassCollection data, double parentMeasure, IDecisionTreeNode parentTreeNode)
        {
            if (parentMeasure < 0)
                throw new ArgumentException("currentScore < 0", "parentMeasure");

            var eq = EquivalenceClassCollection.Create(attributeId, data);
            double gain = this.CalculateImpurityAfterSplit(eq);            
            return new SplitInfo(attributeId,
                this.ImpurityNormalize(parentMeasure - gain, eq), eq, 
                SplitType.Discreet, ComparisonType.EqualTo, 0);
        }

        protected virtual SplitInfo GetSplitInfoNumeric(int attributeId, EquivalenceClassCollection data, double currentScore, IDecisionTreeNode parentTreeNode)
        {
            int attributeIdx = this.TrainingData.DataStoreInfo.GetFieldIndex(attributeId);
            int[] indices = data.Indices;
            long[] values = this.TrainingData.GetFieldIndexValue(indices, attributeIdx);
            Array.Sort(values, indices);

            List<long> thresholds = new List<long>(values.Length);

            if (values.Length > 0)
                thresholds.Add(values[0]);

            for (int k = 0; k < values.Length - 1; k++)
                if (values[k] != values[k + 1])
                    thresholds.Add((values[k] + values[k + 1]) / 2);

            if (thresholds.Count == 0)
                return new SplitInfo(attributeId, Double.NegativeInfinity, 
                    null, SplitType.None, ComparisonType.None, long.MaxValue);

            double maxGain = Double.NegativeInfinity;
            EquivalenceClassCollection bestEq = null;
            long bestThreshold = thresholds.ElementAt(0);
            foreach (var threshold in thresholds)
            {
                var splitEq = EquivalenceClassCollection.CreateBinaryPartition(this.TrainingData, attributeId, indices, threshold);

                double gain = this.CalculateImpurityAfterSplit(splitEq);
                if (gain > maxGain)
                {
                    maxGain = gain;
                    bestThreshold = threshold;
                    bestEq = splitEq;
                }
            }            

            return new SplitInfo(attributeId,
                this.ImpurityNormalize(currentScore - maxGain, bestEq),
                bestEq, SplitType.Binary, ComparisonType.LessThanOrEqualTo, bestThreshold);
        }

        private static int[] DefaultAttributeSelection(DataStore data, int[] attributes)
        {
            return attributes;
        }

        public static int GetNumberOfRules(IDecisionTree tree)
        {
            int count = 0;
            TreeNodeTraversal.TraversePostOrder(tree.Root, n =>
            {
                if (n.IsLeaf)
                    count++;
            });
            return count;
        }

        public static int GetHeight(IDecisionTree tree)
        {            
            int maxHeight = 0;
            TreeNodeTraversal.TraversePostOrder(tree.Root, n =>
            {
                if (n.IsLeaf && n.Level > maxHeight)
                    maxHeight = n.Level;
            });
            return maxHeight;
        }

        public static double GetAvgHeight(IDecisionTree tree)
        {            
            double sumHeight = 0.0;
            double count = 0.0;
            TreeNodeTraversal.TraversePostOrder(tree.Root, n =>
            {
                if (n.IsLeaf)
                {
                    sumHeight += n.Level;
                    count++;
                }
            });

            return count > 0 ? sumHeight / count : 0.0;
        }

        public static int GetNumberOfAttributes(IDecisionTree tree)
        {
            return (tree.Root != null) && (tree.Root is DecisionTreeNode) 
                ? ((DecisionTreeNode)tree.Root).GetChildUniqueKeys().Count 
                : 0;
        }

        private static double MeasureSum(IDecisionTreeNode node)
        {
            double sum = 0;
            TreeNodeTraversal.TraversePostOrder(node, n => 
            {
                if (n.IsLeaf)
                    sum += n.Measure;
            });
            return sum;
        }

        public IEnumerator<IDecisionTreeNode> GetEnumeratorTopDown()
        {
            return TreeNodeTraversal.GetEnumeratorTopDown(this.Root);            
        }

        public IEnumerator<IDecisionTreeNode> GetEnumeratorBottomUp()
        {
            return TreeNodeTraversal.GetEnumeratorTopDown(this.Root);            
        }

        public IEnumerator<IDecisionTreeNode> GetEnumerator()
        {
            return this.GetEnumeratorTopDown();            
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
 
}