using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;
using Infovision.Datamining;
using Infovision.Datamining.Filters.Supervised.Attribute;
using Infovision.Statistics;
using System.Diagnostics;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    /// <summary>
    /// Base class for decision tree implementations
    /// </summary>
    public abstract class DecisionTreeBase : IDecisionTree, IPredictionModel
    {
        protected Dictionary<int, List<long>> thresholds;

        private object syncRoot = new object();
        private DecisionTreeNode root;
        private int decisionAttributeId;
        private double mA;
        private long[] decisions;
        private int nextId;
        
        public IDecisionTreeNode Root { get { return this.root; } }
        public int NumberOfAttributesToCheckForSplit { get; set; }
        public double Epsilon { get; set; }
        public int MaxHeight { get; set; }
        public int MinimumNumOfInstancesPerLeaf { get; set; }        
        public DataStore TrainingData { get; private set; }

        public int EnsembleSize { get { return 1; } }
        public double QualityRatio { get { return this.Root != null ? ((DecisionTreeNode)this.Root).GetChildUniqueKeys().Count : 0; } }

        protected IEnumerable<long> Decisions { get { return this.decisions; } }

        protected class SplitInfo
        {
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
        {
            this.root = null; 
            this.decisionAttributeId = -1;
            this.NumberOfAttributesToCheckForSplit = -1;
            this.MaxHeight = -1;
            this.Epsilon = -1.0;
            this.MinimumNumOfInstancesPerLeaf = 2;
            this.nextId = 1;
        }

        public int GetId()
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
                this.root = new DecisionTreeNode(-1, -1, ComparisonType.EqualTo, -1, null);
                this.decisionAttributeId = data.DataStoreInfo.DecisionFieldId;
                this.decisions = new long[data.DataStoreInfo.NumberOfDecisionValues];

                int i = 0;
                foreach (long decisionValue in data.DataStoreInfo.DecisionInfo.InternalValues())
                    this.decisions[i++] = decisionValue;

                if (this.Epsilon >= 0.0)
                    this.mA = InformationMeasureWeights.Instance.Calc(
                        EquivalenceClassCollection.Create(attributes, data, data.Weights));

                this.TrainingData = data;
                this.thresholds = new Dictionary<int, List<long>>();

               
                foreach (DataFieldInfo field in data.DataStoreInfo.GetFields(FieldTypes.Standard).Where(f => f.IsNumeric))
                {                                        
                    var thresholdList = new List<long>();

                    long[] values = this.TrainingData.GetColumnInternal(field.Id);
                    Array.Sort(values);

                    for (int k = 0; k < values.Length - 1; k++)
                        if (values[k] != values[k + 1])
                            thresholdList.Add((values[k] + values[k + 1]) / 2);

                    this.thresholds[field.Id] = thresholdList;
                }
            }
        }

        public virtual ClassificationResult Learn(DataStore data, int[] attributes)
        {            
            this.Init(data, attributes);
            EquivalenceClassCollection eqClassCollection = EquivalenceClassCollection.Create(new int[] { }, data, data.Weights);
            if (this.Epsilon >= 0.0)
                this.root.Measure = InformationMeasureWeights.Instance.Calc(eqClassCollection);
            this.GenerateSplits(eqClassCollection, this.root, attributes);

            return Classifier.DefaultClassifer.Classify(this, data, data.Weights);
        }

        protected void SetNodeOutput(DecisionTreeNode node, long decisionValue)
        {
            node.Output = decisionValue;
        }

        protected void SetNodeOutput(DecisionTreeNode node, long decisionValue, double decisionWeight)
        {
            node.Output = decisionValue;
        }

        private bool CheckStopCondition(EquivalenceClassCollection currentEqClassCollection, int[] currentAttributes, ref bool isConverged, IDecisionTreeNode parentNode)
        {
            if (currentEqClassCollection.NumberOfObjects == 0)
                return true;

            if (isConverged)
                return true;

            if (currentAttributes.Length == 0)
                return true;

            if(this.MaxHeight != -1 && this.MaxHeight <= parentNode.Level)
                return true;

            if (currentEqClassCollection.HasSingleDecision())
                return true;

            if (this.Epsilon >= 0.0)
            {
                double m = DecisionTreeBase.MeasureSum(this.root);
                if ((1.0 - this.Epsilon) * this.mA <= m)
                {
                    isConverged = true;
                    return true;
                }
            }

            return false;
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

                //Safe current majority class
                var decision = currentEqClassCollection.DecisionWeights.FindMaxValuePair();
                this.SetNodeOutput(currentParent, decision.Key, decision.Value);

                if (this.CheckStopCondition(currentEqClassCollection, currentAttributes, ref isConverged, currentParent))
                    continue;

                var nextSplit = this.GetNextSplit(currentEqClassCollection, currentAttributes);

                //No attribute was found
                if (nextSplit.SplitType == SplitType.None)
                    continue;

                var subEqClasses = EquivalenceClassCollection.Split(nextSplit.AttributeId, nextSplit.EquivalenceClassCollection);
                
                if (nextSplit.SplitType == SplitType.Binary && subEqClasses.Count != 2)
                    throw new InvalidOperationException("Binary split must have two branches.");
                               
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
                    
                    if (this.Epsilon >= 0.0)
                        newNode.Measure = InformationMeasureWeights.Instance.Calc(kvp.Value);

                    var newSplitInfo = Tuple.Create<EquivalenceClassCollection, DecisionTreeNode, int[]>(
                            kvp.Value, 
                            newNode, 
                            currentAttributes);

                    queue.Enqueue(newSplitInfo);
                }
            }
        }        

        private long GetDecision(IDecisionTreeNode node)
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

            IDecisionTreeNode current = this.Root;
            while (current != null)
            {
                if (current.IsLeaf)
                    return current.Output;                    

                current = current.Children.Where(x => x.Compute(record[x.Attribute])).FirstOrDefault();
            }

            return -1; //unclassified
        }
        
        protected virtual SplitInfo GetNextSplit(EquivalenceClassCollection eqClassCollection, int[] attributesToTest)
        {
            double currentScore = this.GetCurrentScore(eqClassCollection);
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

            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = InfovisionConfiguration.MaxDegreeOfParallelism
            };

            SplitInfo[] scores = new SplitInfo[localAttributes.Length];

            Parallel.ForEach(rangePartitioner, options, (range) =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                    scores[i] = this.GetSplitInfo(localAttributes[i], eqClassCollection, currentScore);
            });

            double maxGain = Double.NegativeInfinity;
            int maxIndex = -1;
            for (int i = 0; i < scores.Length; i++)
            {
                if(maxGain < scores[i].Gain)
                {
                    maxGain = scores[i].Gain;
                    maxIndex = i;
                }
            }

            if (maxIndex == -1)
                return new SplitInfo(-1, Double.NegativeInfinity, null, SplitType.None, ComparisonType.None, 0);
                
            return scores[maxIndex];
        }

        protected virtual double GetCurrentScore(EquivalenceClassCollection eqClassCollection)
        {
            throw new NotImplementedException();
        }

        protected virtual SplitInfo GetSplitInfo(int attributeId, EquivalenceClassCollection data, double currentScore)
        {
            DataFieldInfo attributeInfo = this.TrainingData.DataStoreInfo.GetFieldInfo(attributeId);

            if (attributeInfo.IsSymbolic)
            {
                return this.GetSplitInfoSymbolic(attributeId, data, currentScore);
            }
            else if (attributeInfo.IsNumeric)
            {
                return this.GetSplitInfoNumeric(attributeId, data, currentScore);
            }
            
            throw new NotImplementedException("Only symbolic and numeric attribute types are currently supported.");                            
        }

        protected virtual SplitInfo GetSplitInfoSymbolic(int attributeId, EquivalenceClassCollection data, double currentScore)
        {
            throw new NotImplementedException();
        }

        protected virtual SplitInfo GetSplitInfoNumeric(int attributeId, EquivalenceClassCollection data, double currentScore)
        {
            throw new NotImplementedException();
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
              
        public IEnumerator<IDecisionTreeNode> GetEnumerator()
        {
            if (this.Root == null)
                yield break;

            var stack = new Stack<IDecisionTreeNode>(new[] { this.Root });
            while (stack.Count != 0)
            {
                IDecisionTreeNode current = stack.Pop();
                yield return current;

                if (current.Children != null)
                    foreach(IDecisionTreeNode node in current.Children)                    
                        stack.Push(node);
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
 
}