using Infovision.Core;
using Infovision.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Discretization
{
    [Serializable]
    public class DiscretizeSupervisedBase : DiscretizeBase, IDiscretizerSupervised
    {
        protected static long[] emptyInstance = new long[] { };
        protected static long[] leftInstance = new long[] { 1 };
        protected static long[] rightInstance = new long[] { 2 };

        #region Properties

        public bool SortCuts { get; set; } = true;

        #endregion

        #region Constructors

        public DiscretizeSupervisedBase()
            : base()
        {
            this.NumberOfBuckets = -1;
        }

        #endregion

        #region Methods

        /// <summary>
        /// This method recursively calculates best cuts
        /// </summary>
        /// <param name="data">Values to be discretized</param>
        /// <param name="labels">Labels</param>
        /// <param name="start">Starting index</param>
        /// <param name="end">Last index plus one, number of elements to analyze</param>
        /// <param name="weights">Object weights, can be null, then 1.0 is used as object weight</param>
        /// <returns></returns>
        public virtual long[] ComputeCuts(long[] data, long[] labels, int start, int end, double[] weights)
        {
            if (start < 0) throw new ArgumentOutOfRangeException("start", "start < 0");
            if (end > data.Length) throw new ArgumentOutOfRangeException("end", "end > data.Length");
            if (data == null || data.Length == 0) return null;

            var result = new List<long>();
            var queue = new Queue<Tuple<int, int>>();
            queue.Enqueue(new Tuple<int, int>(start, end));

            while (queue.Count != 0)
            {
                var current = queue.Dequeue();
                                
                if (current.Item2 - current.Item1 < 2) continue;

                var priorCount = CountLabels(
                    labels, current.Item1, current.Item2, weights);
                if (priorCount.Count < 2) continue;

                var priorEqClasses = new EquivalenceClassCollection(
                    labels, weights, current.Item1, current.Item2, this.SortedIndices);
                if (priorEqClasses.DecisionWeight.Count < 2) continue;

                var priorEntropy = Tools.Entropy(priorCount.Values.ToArray());                
                var labelCountRight = new Dictionary<long, double>(priorCount);
                var labelCountLeft = new Dictionary<long, double>(priorCount);
                labelCountLeft.SetAll(0);

                var priorEntropy2 = ImpurityMeasure.Entropy(priorEqClasses);

                //var labelCountRight2 = (EquivalenceClassCollection)priorCount2.Clone();                
                //var labelCountLeft2 = (EquivalenceClassCollection)priorCount2.Clone();
                //labelCountLeft2.Clear();

                var splitEqClasses = new EquivalenceClassCollection(new int[] { -1 });
                splitEqClasses.WeightSum = priorEqClasses.WeightSum;
                splitEqClasses.NumberOfObjects = priorEqClasses.NumberOfObjects;
                splitEqClasses.DecisionWeight = new Dictionary<long, double>(priorEqClasses.DecisionWeight);
                splitEqClasses.DecisionCount = new Dictionary<long, int>(priorEqClasses.DecisionCount);
                splitEqClasses.Add(new EquivalenceClass(leftInstance));
                splitEqClasses.Add(new EquivalenceClass(rightInstance, priorEqClasses.First()));

                EquivalenceClassCollection bestSplit = null;
                double[] bestLeft = null, bestRight = null;
                int bestCutIndex = -1;
                long bestCutPoint = -1;
                int numCutPoints = 0;
                double instanceWeight = 0.0;
                double bestEntropy = priorEntropy;
                //double bestEntropy = priorEntropy2;

                for (int i = current.Item1; i < current.Item2 - 1; i++)
                {
                    if (weights == null || this.UseWeights == false)
                    {
                        instanceWeight++;
                        labelCountLeft[labels[SortedIndices[i]]]++;
                        labelCountRight[labels[SortedIndices[i]]]--;                        

                        splitEqClasses.RemoveInstance(rightInstance, labels[SortedIndices[i]], 1, SortedIndices[i]);
                        splitEqClasses.AddInstance(leftInstance, labels[SortedIndices[i]], 1, SortedIndices[i]);
                        
                    }
                    else
                    {
                        instanceWeight += weights[SortedIndices[i]];
                        labelCountLeft[labels[SortedIndices[i]]] += weights[SortedIndices[i]];
                        labelCountRight[labels[SortedIndices[i]]] -= weights[SortedIndices[i]];                        

                        splitEqClasses.RemoveInstance(rightInstance, labels[SortedIndices[i]], weights[SortedIndices[i]], SortedIndices[i]);
                        splitEqClasses.AddInstance(leftInstance, labels[SortedIndices[i]], weights[SortedIndices[i]], SortedIndices[i]);
                        
                    }

                    if (data[SortedIndices[i]] < data[SortedIndices[i + 1]])
                    {
                        long currentCutPoint = (data[SortedIndices[i]] + data[SortedIndices[i + 1]]) / 2;
                        var currentEntropy = Tools.Entropy(labelCountLeft.Values.ToArray(), labelCountRight.Values.ToArray());

                        var currentEntropy2 = ImpurityMeasure.Entropy(splitEqClasses);

                        if (currentEntropy < bestEntropy)
                        //if (currentEntropy2 < bestEntropy)
                        {
                            bestEntropy = currentEntropy;
                            //bestEntropy = currentEntropy2;
                            bestCutIndex = i;
                            bestCutPoint = currentCutPoint;

                            bestLeft = labelCountLeft.Values.ToArray();
                            bestRight = labelCountRight.Values.ToArray();

                            bestSplit = (EquivalenceClassCollection)splitEqClasses.Clone();
                        }

                        numCutPoints++;
                    }
                }            
                
                if (bestCutIndex == -1) continue;
                if (priorEntropy - bestEntropy <= 0) continue;
                //if (priorEntropy2 - bestEntropy <= 0) continue;

                if (!StopCondition(priorCount.Values.ToArray(), bestLeft, bestRight,
                        numCutPoints, current.Item2 - current.Item1, instanceWeight))
                //if (!StopCondition(priorEqClasses, bestSplit, numCutPoints))
                {
                    result.Add(bestCutPoint);

                    queue.Enqueue(new Tuple<int, int>(current.Item1, bestCutIndex + 1));
                    queue.Enqueue(new Tuple<int, int>(bestCutIndex + 1, current.Item2));
                }

                if (this.NumberOfBuckets > 0 && result.Count >= this.NumberOfBuckets)
                    break;
            }

            if(result.Count == 0)
                return null;

            if(SortCuts && result.Count > 1)
                result.Sort();

            return result.ToArray();
        }

        protected virtual bool StopCondition(
            EquivalenceClassCollection priorEqClasses, 
            EquivalenceClassCollection splitEqClasses,
            int numOfPossibleCuts)
        {
            if(priorEqClasses.NumberOfObjects == 0)            
                return true;

            if (numOfPossibleCuts < 1)
                return true;

            return false;
        }

        protected virtual bool StopCondition(
            double[] priorCount, double[] left, double[] right, 
            int numOfPossibleCuts, int numOfInstances, double instanceWeight)
        {
            if (numOfInstances == 0)
                return true;

            if (numOfPossibleCuts < 1)
                return true;

            return false;
        }

        public override void Compute(long[] data, long[] labels, double[] weights)
        {
            if (data == null) throw new ArgumentNullException("data", "data == null");
            if (labels != null && labels.Length != data.Length)
                throw new ArgumentException("labels.Length != data.Length", "labels");
            if(weights != null && weights.Length != data.Length)
                throw new ArgumentException("weights.Length != data.Length", "weights");

            if (!this.IsDataSorted)
                this.SortIndices(data);

            this.Cuts = this.ComputeCuts(data, labels, 0, data.Length, weights);
            this.Cleanup();
        }

        #endregion
    }
}
