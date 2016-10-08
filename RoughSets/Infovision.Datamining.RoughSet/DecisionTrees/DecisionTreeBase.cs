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
        public int MinimumNumOfInstancesPerLeaf { get; set; }        
        public DataStore TrainingData { get; private set; }

        public int EnsembleSize { get { return 1; } }
        public double QualityRatio { get { return this.Root != null ? ((DecisionTreeNode)this.Root).GetChildUniqueKeys().Count : 0; } }

        protected IEnumerable<long> Decisions { get { return this.decisions; } }
        

        public DecisionTreeBase()
        {
            this.root = null; 
            this.decisionAttributeId = -1;
            this.NumberOfAttributesToCheckForSplit = -1;
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

        protected void CreateDecisionLeaf(DecisionTreeNode parent, long decisionValue)
        {
            parent.Output = decisionValue;
            //parent.AddChild(new DecisionTreeNode(this.decisionAttributeId, ComparisonType.EqualTo, decisionValue, parent));
        }

        protected void CreateDecisionLeaf(DecisionTreeNode parent, long decisionValue, double decisionWeight)
        {
            parent.Output = decisionValue;
            //parent.AddChild(new DecisionTreeNode(this.decisionAttributeId, ComparisonType.EqualTo, decisionValue, parent, decisionWeight));
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
                    this.CreateDecisionLeaf(currentParent, decision.Key, decision.Value);
                    continue;
                }

                var singleDecision = currentEqClassCollection.GetSingleDecision();
                if (singleDecision.Key != -1)
                {
                    this.CreateDecisionLeaf(currentParent, singleDecision.Key, singleDecision.Value);
                    continue;
                }

                if (this.Epsilon >= 0.0)
                {
                    double m = this.MeasureSum(this.root);
                    if ((1.0 - this.Epsilon) * this.mA <= m)
                    {
                        this.CreateDecisionLeaf(currentParent, currentEqClassCollection.DecisionWeights.FindMaxValueKey());
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
                    DecisionTreeNode newNode = new DecisionTreeNode(this.GetId(), maxAttribute, ComparisonType.EqualTo, kvp.Key, currentParent);
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

        private bool IsNextNodeALeafNode(IDecisionTreeNode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            
            if (node.Children == null)
                throw new InvalidOperationException("node.Children == null");

            if (node.Children[0].Attribute == this.decisionAttributeId)
                return true;

            return false;
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
                    //return current.Value;

                //if (this.IsNextNodeALeafNode(current))
                //    return this.GetDecision(current);

                current = current.Children.Where(x => x.Compute(record[x.Attribute])).FirstOrDefault();
            }

            return -1; //unclassified
        }

        //TODO Change return value to a internal private class that can store additional values like cut threshold for continuous attributes
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
                        DataFieldInfo attributeInfo = this.TrainingData.DataStoreInfo.GetFieldInfo(localAttributes[i]);
                        
                        if (attributeInfo.IsSymbolic)
                        {
                            var attributeEqClasses = EquivalenceClassCollection.Create(
                                localAttributes[i], eqClassCollection);

                            scores[i] = Tuple.Create<int, double, EquivalenceClassCollection>(
                                    localAttributes[i],
                                    this.GetSplitScore(attributeEqClasses, currentScore),
                                    attributeEqClasses);
                        }
                        else //Is continuous
                        {
                            int[] indices = eqClassCollection.Indices;
                            long[] outputs = this.TrainingData.GetDecisionValue(indices);
                            long[] values = this.TrainingData.GetFieldValue(indices, localAttributes[i]);
                            
                            //TODO improve
                            Array.Sort(values.ToArray(), indices);
                            Array.Sort(values, outputs);

                            List<long> thresholds = new List<long>(values.Length);

                            for (int k = 0; k < values.Length - 1; k++)
                                if (values[k] != values[k + 1])
                                    thresholds.Add((values[k] + values[k + 1]) / 2);

                            //this is not good as it use as many cuts as different number of decision classes
                            //var discretization = new Discretization<long, long>();
                            //discretization.UseKononenko = false;
                            //discretization.UseBetterEncoding = false;
                            //discretization.Compute(values, outputs, true, null);

                            long[] threshold = thresholds.ToArray();
                            thresholds.Clear();

                            double bestGain = Double.NegativeInfinity;
                            long bestThreshold = threshold[0];

                            for (int k = 0; k < threshold.Length; k++)
                            {
                                int[] idx1 = indices.Where(idx => (values[idx] <= threshold[k])).ToArray();
                                int[] idx2 = indices.Where(idx => (values[idx] > threshold[k])).ToArray();

                                long[] output1 = new long[idx1.Length];
                                long[] output2 = new long[idx2.Length];

                                for (int j = 0; j < idx1.Length; j++)
                                    output1[j] = outputs[idx1[j]];

                                for (int j = 0; j < idx2.Length; j++)
                                    output2[j] = outputs[idx2[j]];

                                double p1 = output1.Length / (double)outputs.Length;
                                double p2 = output2.Length / (double)outputs.Length;

                                double gain = -p1 * Tools.Entropy(output1, this.decisions) + 
                                              -p2 * Tools.Entropy(output2, this.decisions);

                                if (gain > bestGain)
                                {
                                    bestGain = gain;
                                    bestThreshold = threshold[k];
                                }

                            }

                            //TODO Create EquivalenceClassCollection with numeric attributes

                            var attributeEqClasses = EquivalenceClassCollection.Create(
                                localAttributes[i], eqClassCollection);

                            scores[i] = Tuple.Create<int, double, EquivalenceClassCollection>(
                                    localAttributes[i],
                                    bestGain,
                                    attributeEqClasses);
                        }
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

        protected virtual double GetSplitScoreContinuous()
        {
            throw new NotImplementedException();
        }

        private double MeasureSum(IDecisionTreeNode node)
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