using NRough.Core.CollectionExtensions;
using NRough.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Classification
{
    public class ConfusionMatrix
    {
        private Dictionary<long, int> decisionValue2Index;
        private long[] decisions;
        private int[][] confusionTable;
        private double[][] confusionTableWeights;
        private readonly object syncRoot = new object();

        public double TruePositiveAvg
        {
            get
            {
                double sum = 0;
                for (int i = 1; i < decisions.Length; i++)
                    sum += confusionTable[i][i];
                return sum / (double)(decisions.Length - 1);
            }
        }

        public double TrueNegativeAvg
        {
            get
            {
                double sum = 0;
                for (int j = 1; j < decisions.Length; j++)
                    for (int i = 1; i < decisions.Length; i++)
                        if (j != i)
                            sum += confusionTable[j][i];
                return sum / (double)(decisions.Length - 1);
            }
        }

        public double FalsePositiveAvg
        {
            get
            {
                double sum = 0;
                for (int k = 1; k < decisions.Length; k++)
                    for (int j = 1; j < decisions.Length; j++)
                        for (int i = 1; i < decisions.Length; i++)
                            if (j != k && i != k)
                                sum += confusionTable[i][j];

                return sum / (double)(decisions.Length - 1);
            }
        }

        public double FalseNegativeAvg
        {
            get
            {
                double sum = 0;
                for (int j = 1; j < decisions.Length; j++)
                    for (int i = 0; i < decisions.Length; i++)
                        if (j != i)
                            sum += confusionTable[j][i];
                return sum / (double)(decisions.Length - 1);
            }
        }

        public ConfusionMatrix(long[] labels)
        {
            if (labels == null) throw new ArgumentNullException("labels");
            if (labels.Length == 0) throw new ArgumentException("labels.Length == 0", "labels");

            decisions = new long[labels.Length + 1];
            this.decisionValue2Index = new Dictionary<long, int>(decisions.Length);

            decisions[0] = Classifier.UnclassifiedOutput;
            this.decisionValue2Index.Add(Classifier.UnclassifiedOutput, 0);

            for (int i = 1; i < decisions.Length; i++)
            {
                this.decisions[i] = labels[i - 1];
                this.decisionValue2Index.Add(labels[i - 1], i);
            }

            this.confusionTable = new int[decisions.Length][];
            this.confusionTableWeights = new double[decisions.Length][];
            for (int i = 0; i < decisions.Length; i++)
            {
                this.confusionTable[i] = new int[decisions.Length];
                this.confusionTableWeights[i] = new double[decisions.Length];
            }
        }

        public int GetCountByIndex(int actualIdx, int predictedIdx)
        {
            return confusionTable[actualIdx][predictedIdx];
        }

        public int GetCount(long actual, long predicted)
        {
            int actualIdx = decisionValue2Index[actual];
            int predictedIdx = decisionValue2Index[predicted];
            return GetCountByIndex(actualIdx, predictedIdx);
        }

        public double GetWeightByIndex(int actualIdx, int predictedIdx)
        {
            return confusionTableWeights[actualIdx][predictedIdx];
        }

        public double GetWeight(long actual, long predicted)
        {
            int actualIdx = decisionValue2Index[actual];
            int predictedIdx = decisionValue2Index[predicted];
            return GetWeightByIndex(actualIdx, predictedIdx);
        }

        public virtual void AddResult(long actual, long prediction, int count, double weight)
        {
            int actualDecIdx = decisionValue2Index[actual];
            int predictionDecIdx = decisionValue2Index[prediction];
            lock (syncRoot)
            {
                confusionTable[actualDecIdx][predictionDecIdx] += count;
                confusionTableWeights[actualDecIdx][predictionDecIdx] += weight;
            }
        }
        
        public virtual void AddResult(long actual, long prediction, int count = 1)
        {
            AddResult(actual, prediction, count, (double)count);
        }

        public void Reset()
        {
            for (int i = 0; i < decisions.Length; i++)
            {
                for (int j = 0; j < decisions.Length; j++)
                {
                    confusionTable[i][j] = 0;
                    confusionTableWeights[i][j] = 0;
                }
            }
        }

        /// <summary>
        /// actual cats that were correctly classified as cats
        /// </summary>
        /// <param name="decision"></param>
        /// <returns></returns>
        public int TruePositive(long decision)
        {
            int decIdx = decisionValue2Index[decision];
            return confusionTable[decIdx][decIdx];
        }

        /// <summary>
        /// cats that were incorrectly marked as other animals
        /// </summary>
        /// <param name="decision"></param>
        /// <returns></returns>
        public int FalseNegative(long decision)
        {
            int result = 0;
            int decIdx = decisionValue2Index[decision];
            for (int i = 0; i < decisions.Length; i++)
                if (decIdx != i)
                    result += confusionTable[decIdx][i];
            return result;
        }

        /// <summary>
        /// all the remaining animals that were incorrectly labeled as cats
        /// </summary>
        /// <param name="decision"></param>
        /// <returns></returns>
        public int FalsePositive(long decision)
        {
            int result = 0;
            int decIdx = decisionValue2Index[decision];
            for (int i = 1; i < decisions.Length; i++)
                if (decIdx != i)
                    result += confusionTable[i][decIdx];
            return result;
        }

        /// <summary>
        /// all the remaining animals, correctly classified as non-cats
        /// </summary>
        /// <param name="decision"></param>
        /// <returns></returns>
        public int TrueNegative(long decision)
        {
            int result = 0;
            int decIdx = decisionValue2Index[decision];

            for (int j = 1; j < decisions.Length; j++)
            for (int i = 1; i < decisions.Length; i++)
                if (decIdx != i && decIdx != j)
                    result += confusionTable[j][i];

            return result;
        }

        /// <summary>
        /// actual cats that were correctly classified as cats
        /// </summary>
        /// <param name="decision"></param>
        /// <returns></returns>
        public double TruePositiveWeight(long decision)
        {
            int decIdx = decisionValue2Index[decision];
            return confusionTableWeights[decIdx][decIdx];
        }

        /// <summary>
        /// cats that were incorrectly marked as other animals
        /// </summary>
        /// <param name="decision"></param>
        /// <returns></returns>
        public double FalseNegativeWeight(long decision)
        {
            double result = 0;
            int decIdx = decisionValue2Index[decision];
            for (int i = 0; i < decisions.Length; i++)
                if (decIdx != i)
                    result += confusionTableWeights[decIdx][i];
            return result;
        }

        /// <summary>
        /// all the remaining animals that were incorrectly labeled as cats
        /// </summary>
        /// <param name="decision"></param>
        /// <returns></returns>
        public double FalsePositiveWeight(long decision)
        {
            double result = 0;
            int decIdx = decisionValue2Index[decision];
            for (int i = 0; i < decisions.Length; i++)
                if (decIdx != i)
                    result += confusionTableWeights[i][decIdx];
            return result;
        }

        /// <summary>
        /// all the remaining animals, correctly classified as non-cats
        /// </summary>
        /// <param name="decision"></param>
        /// <returns></returns>
        public double TrueNegativeWeight(long decision)
        {
            double result = 0;
            int decIdx = decisionValue2Index[decision];
            for (int i = 0; i < decisions.Length; i++)
                if (decIdx != i)
                    result += confusionTableWeights[i][i];
            return result;
        }

        public string[][] Output(DataStoreInfo metadata = null)
        {
            string[][] result = new string[decisions.Length + 1][];
            for (int i = 0; i <= decisions.Length; i++)
                result[i] = new string[decisions.Length];
            result.SetAll("");

            result[decisions.Length][0] = "unclass";
            for (int i = 1; i < decisions.Length; i++)
            {
                result[i][0] = metadata == null 
                             ? decisions[i].ToString() 
                             : metadata.DecisionInfo.Internal2External(decisions[i]).ToString();

                result[0][i] = result[i][0];
            }

            for (int j = 1; j < decisions.Length; j++)
            {
                for (int i = 1; i < decisions.Length; i++)
                {
                    result[i][j] = GetCount(decisions[j], decisions[i]).ToString();
                }
                result[decisions.Length][j] = GetCount(decisions[j], Classifier.UnclassifiedOutput).ToString();
            }

            return result;
        }

        public override string ToString()
        {
            return Output().ToStr2d(" ", Environment.NewLine);
        }

    }
}
