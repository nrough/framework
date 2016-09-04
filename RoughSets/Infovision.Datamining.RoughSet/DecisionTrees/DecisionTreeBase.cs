using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;
using System.Diagnostics;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    /// <summary>
    /// Base class for decision tree implementations
    /// </summary>
    public abstract class DecisionTreeBase : IDecisionTree, IPredictionModel
    {
        private DecisionTreeNode root;
        private int decisionAttributeId;
        private decimal mA;
        private long[] decisions;
        

        public ITreeNode Root { get { return this.root; } }
        public int NumberOfAttributesToCheckForSplit { get; set; }
        public decimal Epsilon { get; set; }
        protected IEnumerable<long> Decisions { get { return this.decisions; } }

        public int EnsembleSize { get { return 1; } }
        public double QualityRatio { get { return this.Root != null ? ((DecisionTreeNode)this.Root).GetChildUniqueKeys().Count : 0; } }
        
        public DecisionTreeBase()
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

        public virtual ClassificationResult Learn(DataStore data, int[] attributes)
        {
            Stopwatch s = new Stopwatch();
            s.Start();

            this.Init(data, attributes);
            EquivalenceClassCollection eqClassCollection = EquivalenceClassCollection.Create(new int[] { }, data, data.Weights);
            if (this.Epsilon >= Decimal.Zero)
                this.root.Measure = InformationMeasureWeights.Instance.Calc(eqClassCollection);
            this.GenerateSplits(eqClassCollection, this.root, attributes);

            s.Stop();

            ClassificationResult trainResult = Classifier.Instance.Classify(this, data, data.Weights);
            trainResult.ModelCreationTime = s.ElapsedMilliseconds;
            return trainResult;
        }

        protected void CreateLeaf(DecisionTreeNode parent, long decisionValue)
        {
            parent.AddChild(new DecisionTreeNode(this.decisionAttributeId, decisionValue, parent));
        }

        protected void CreateLeaf(DecisionTreeNode parent, long decisionValue, decimal decisionWeight)
        {
            parent.AddChild(new DecisionTreeNode(this.decisionAttributeId, decisionValue, decisionWeight, parent));
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

                currentAttributes = currentAttributes.RemoveValue(maxAttribute);

                foreach (var kvp in subEqClasses)
                {
                    DecisionTreeNode newNode = new DecisionTreeNode(maxAttribute, kvp.Key, currentParent);
                    currentParent.AddChild(newNode);

                    if (this.Epsilon >= Decimal.Zero)
                        newNode.Measure = InformationMeasureWeights.Instance.Calc(kvp.Value);

                    var newSplitInfo = Tuple.Create<EquivalenceClassCollection, DecisionTreeNode, int[]>(
                            kvp.Value, 
                            newNode, 
                            currentAttributes);

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

        protected virtual Tuple<int, double, EquivalenceClassCollection> GetNextSplit(
            EquivalenceClassCollection eqClassCollection, int[] attributesToTest)
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
 
}