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
    public abstract class DiscretizeSupervisedBase : DiscretizeBase, IDiscretizationSupervised
    {       
        #region Constructors

        public DiscretizeSupervisedBase()
            : base() { }

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
            if (data == null || data.Length == 0) return null;
            if (end - start < 2) return null;            

            if (start < 0) throw new ArgumentOutOfRangeException("start", "start < 0");
            if (end > data.Length) throw new ArgumentOutOfRangeException("end", "end > data.Length");
            

            var priorCount = CountLabels(labels, start, end, weights);
            if (priorCount.Count == 1) return null;
            
            var priorEntopy = Tools.Entropy(priorCount.Values.ToArray());

            var labelCountRight = new Dictionary<long, double>(priorCount);
            var labelCountLeft = new Dictionary<long, double>(priorCount);
            labelCountLeft.SetAll(0);

            double[] bestLeft = null, bestRight = null;                        
            int bestCutIndex = -1;
            long bestCutPoint = -1;
            int numCutPoints = 0;
            double instanceWeight = 0.0;
            double bestEntropy = priorEntopy;
            
            for (int i = start; i < end - 1; i++)
            {                
                if (weights == null)
                {
                    instanceWeight++;
                    labelCountLeft[labels[SortedIndices[i]]]++;
                    labelCountRight[labels[SortedIndices[i]]]--;
                }
                else
                {
                    instanceWeight += weights[SortedIndices[i]];
                    labelCountLeft[labels[SortedIndices[i]]] += weights[SortedIndices[i]];
                    labelCountRight[labels[SortedIndices[i]]] -= weights[SortedIndices[i]];
                }                

                if (data[SortedIndices[i]] < data[SortedIndices[i+1]])
                {
                    long currentCutPoint = (data[SortedIndices[i]] + data[SortedIndices[i + 1]]) / 2;
                    var currentEntropy = Tools.Entropy(labelCountLeft.Values.ToArray(), labelCountRight.Values.ToArray());

                    if (currentEntropy < bestEntropy)
                    {
                        bestEntropy = currentEntropy;
                        bestCutIndex = i;
                        bestCutPoint = currentCutPoint;

                        bestLeft = labelCountLeft.Values.ToArray();
                        bestRight = labelCountRight.Values.ToArray();
                    }

                    numCutPoints++;
                }
            }

            int numInstances = (end - start);

            if (bestCutIndex == -1) return null;
            if (priorEntopy - bestEntropy <= 0) return null;

            if ( ! StopCondition(priorCount.Values.ToArray(), bestLeft, bestRight, 
                    numCutPoints, numInstances, instanceWeight))
            {                
                //call recurently compute cuts on subarrays
                long[] left = this.ComputeCuts(data, labels, start, bestCutIndex + 1, weights);
                long[] right = this.ComputeCuts(data, labels, bestCutIndex + 1, end, weights);

                // Merge cutpoints and return
                if ((left == null) && (right == null))
                {
                    return new long[] { bestCutPoint };
                }

                long[] result;
                if (right == null)
                {
                    result = new long[left.Length + 1];
                    Array.Copy(left, 0, result, 0, left.Length);
                    result[left.Length] = bestCutPoint;
                    return result;
                }

                if (left == null)
                {
                    result = new long[right.Length + 1];
                    result[0] = bestCutPoint;
                    Array.Copy(right, 0, result, 1, right.Length);
                    return result;
                }
                
                result = new long[left.Length + right.Length + 1];
                Array.Copy(left, 0, result, 0, left.Length);
                result[left.Length] = bestCutPoint;
                Array.Copy(right, 0, result, left.Length + 1, right.Length);
                return result;
            }

            return null;
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
            if (this.Cuts == null)
                this.Cuts = new long[0];
        }

        #endregion
    }
}
