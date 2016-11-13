﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infovision.Data;
using Infovision.Utils;
using System.Diagnostics;

namespace Infovision.Datamining
{
    /// <summary>
    /// Encodes the region of confusion matrix
    /// </summary>
    public enum ConfusionMatrixElement
    {
        //actual cats that were correctly classified as cats
        TruePositive = 0,

        //cats that were incorrectly marked as other animals
        FalseNegative = 1,

        //all the remaining animals that were incorrectly labeled as cats
        FalsePositive = 2,

        //all the remaining animals, correctly classified as non-cats
        TrueNegative = 3
    }

    //TODO add additional classification Info
    [Serializable]
    public class ClassificationResult : IFormattable
    {
        #region Members

        public static string OutputColumns = @"ds;m;t;f;eps;ens;acc;attr;numrul;dthm;dtha";

        private Dictionary<long, int> value2index;
        private long[] decisions;
        private int decCount;
        private int decCountPlusOne;
        private long[] predictionResults;
        private int[][] confusionTable;
        private double[][] confusionTableWeights;
        private int counter;
        private DataStore testData;        
        private readonly object syncRoot = new object();

        #endregion Members

        #region Properties

        public int Count
        {
            get { return this.counter; }
        }

        public DataStore TestData
        {
            get { return this.testData; }
            set { this.testData = value; }
        }

        public double Accuracy
        {
            get
            {
                return counter > 0 ? (double)this.Classified / (double)counter : 0;
            }
        }

        public double Error
        {
            get { return 1.0 - this.Accuracy; }
        }

        public double BalancedAccuracy
        {
            get
            {
                double sum = 0;
                for (int i = 1; i < decCountPlusOne; i++)
                {
                    int count = 0;
                    for (int j = 0; j < decCountPlusOne; j++)
                        count += confusionTable[i][j];

                    if (count > 0)
                    {
                        sum += (double)confusionTable[i][i] / (double)count;
                    }
                }

                return decCount > 0 ? sum / decCount : 0.0;
            }
        }

        //http://en.wikipedia.org/wiki/Precision_and_recall
        public double Recall
        {
            get
            {
                //TODO Implement Recall
                return 0.0;
            }
        }

        //https://en.wikipedia.org/wiki/Accuracy_and_precision
        public double Precision
        {
            get
            {
                //TODO Implement Precision
                return 0.0;
            }
        }

        public double Coverage
        {
            get
            {
                return this.Count > 0 ? (double)(this.Classified + this.Misclassified) / (double)this.Count : 0;
            }
        }

        public double Confidence
        {
            get
            {
                int classified = this.Classified;
                int misclassified = this.Misclassified;
                return ((classified + misclassified) != 0) ? (double)classified / (double)(classified + misclassified) : 0;
            }
        }

        public int Classified
        {
            get
            {
                int sum = 0;
                for (int i = 1; i < decCountPlusOne; i++)
                    sum += confusionTable[i][i];
                return sum;
            }
        }

        public int Misclassified
        {
            get
            {
                int sum = 0;
                for (int i = 1; i < decCountPlusOne; i++)
                    for (int j = 1; j < decCountPlusOne; j++)
                        if (i != j)
                            sum += confusionTable[i][j];
                return sum;
            }
        }

        public int Unclassified
        {
            get
            {
                int sum = 0;
                for (int i = 1; i < decCountPlusOne; i++)
                    sum += confusionTable[i][0];
                return sum;
            }
        }

        public double WeightClassified
        {
            get
            {
                double sum = 0;
                for (int i = 1; i < decCountPlusOne; i++)
                    sum += this.confusionTableWeights[i][i];
                return sum;
            }
        }

        public double WeightMisclassified
        {
            get
            {
                double sum = 0;
                for (int i = 1; i < decCountPlusOne; i++)
                    for (int j = 1; j < decCountPlusOne; j++)
                        if (i != j)
                            sum += this.confusionTableWeights[i][j];
                return sum;
            }
        }

        public double WeightUnclassified
        {
            get
            {
                double sum = 0;
                for (int i = 1; i < decCountPlusOne; i++)
                    sum += this.confusionTableWeights[i][0];
                return sum;
            }
        }

        public double AvgNumberOfAttributes { get; set; }
        public double NumberOfRules { get; set; }
        public double MaxTreeHeight { get; set; }
        public double AvgTreeHeight { get; set; }

        public long ClassificationTime { get; set; }
        public long ModelCreationTime { get; set; }

        public int ExceptionRuleHitCounter { get; set; }
        public int StandardRuleHitCounter { get; set; }
        public int ExceptionRuleLengthSum { get; set; }
        public int StandardRuleLengthSum { get; set; }
                
        public double Alpha { get; set; }
        public double Beta { get; set; }
        public double Gamma { get; set; }
        public double Delta { get; set; }
        public double Epsilon { get; set; }

        public string DatasetName { get; set; }
        public string ModelName { get; set; }
        public int TestNum { get; set; }
        public int Fold { get; set; }        
        public int EnsembleSize { get; set; }

        public string Description { get; set; }

        #endregion Properties

        #region Constructors

        private ClassificationResult()
        {            
            this.ModelCreationTime = -1;
            this.ClassificationTime = -1;
            this.EnsembleSize = 1;
        }

        public ClassificationResult(DataStore dataStore, ICollection<long> decisionValues)
            : this()
        {
            this.DatasetName = dataStore.Name;
            this.TestData = dataStore;

            this.decCount = decisionValues.Count;
            this.decCountPlusOne = decisionValues.Count + 1;
            this.decisions = new long[decCountPlusOne];
            this.decisions[0] = -1;
            long[] decArray = decisionValues.ToArray();
            double[] decDistribution = new double[decArray.Length];
            for (int i = 0; i < decArray.Length; i++)
                decDistribution[i] = (int)dataStore.DataStoreInfo.DecisionInfo.Histogram.GetBinValue(decArray[i]);
            Array.Sort(decDistribution, decArray);
            decDistribution = null;
            Array.Copy(decArray, 0, decisions, 1, decCount);
            value2index = new Dictionary<long, int>(decCountPlusOne);
            value2index.Add(-1, 0);
            for (int i = 0; i < decArray.Length; i++)
                value2index.Add(decArray[i], i + 1);

            predictionResults = new long[dataStore.NumberOfRecords];
            confusionTable = new int[decCountPlusOne][];
            confusionTableWeights = new double[decCountPlusOne][];
            for (int i = 0; i < decCountPlusOne; i++)
            {
                confusionTable[i] = new int[decCountPlusOne];
                confusionTableWeights[i] = new double[decCountPlusOne];
            }
            this.Fold = dataStore.Fold;
        }

        #endregion Constructors

        #region Methods

        public void Reset()
        {
            lock (syncRoot)
            {
                for (int i = 0; i < predictionResults.Length; i++)
                    predictionResults[i] = -1;

                for (int i = 0; i < decCountPlusOne; i++)
                {
                    for (int j = 0; j < decCountPlusOne; j++)
                    {
                        confusionTable[i][j] = 0;
                        confusionTableWeights[i][j] = 0;
                    }
                }

                this.counter = 0;
            }
        }

        public virtual void AddResult(int objectIdx, long prediction, long actual, double weight = 1.0)
        {
            int actualDecIdx = value2index[actual];
            int predictionDecIdx = value2index[prediction];

            this.predictionResults[objectIdx] = prediction;

            lock (syncRoot)
            {
                confusionTable[actualDecIdx][predictionDecIdx]++;
                confusionTableWeights[actualDecIdx][predictionDecIdx] += weight;
                this.counter++;
            }
        }

        public long GetPrediction(int objectIdx)
        {
            return predictionResults[objectIdx];
        }

        public long GetActual(int objectIdx)
        {
            return testData.GetFieldValue(objectIdx, testData.DataStoreInfo.DecisionFieldId);
        }

        public int GetConfusionTable(long prediction, long actual)
        {
            return confusionTable[value2index[actual]][value2index[prediction]];
        }

        public int GetConfusionTable(long decisionValue, ConfusionMatrixElement confusionTableElement)
        {
            int result = 0;
            int decIdx = value2index[decisionValue];

            switch (confusionTableElement)
            {
                //actual cats that were correctly classified as cats
                case ConfusionMatrixElement.TruePositive:
                    result = confusionTable[decIdx][decIdx];
                    break;

                //cats that were incorrectly marked as other animals
                case ConfusionMatrixElement.FalseNegative:
                    for (int i = 0; i < decCountPlusOne; i++)
                        if (decIdx != i)
                            result += confusionTable[decIdx][i];
                    break;

                //all the remaining animals that were incorrectly labeled as cats
                case ConfusionMatrixElement.FalsePositive:
                    for (int i = 0; i < decCountPlusOne; i++)
                        if (decIdx != i)
                            result += confusionTable[i][decIdx];
                    break;

                //all the remaining animals, correctly classified as non-cats
                case ConfusionMatrixElement.TrueNegative:
                    for (int i = 0; i < decCountPlusOne; i++)
                        if (decIdx != i)
                            result += confusionTable[i][i];
                    break;
            }
            return result;
        }

        public double GetConfusionTableWeights(long decisionValue, ConfusionMatrixElement confusionTableElement)
        {
            double result = 0;
            int decIdx = value2index[decisionValue];
            switch (confusionTableElement)
            {
                //actual cats that were correctly classified as cats
                case ConfusionMatrixElement.TruePositive:
                    result = confusionTableWeights[decIdx][decIdx];
                    break;

                //cats that were incorrectly marked as other animals
                case ConfusionMatrixElement.FalseNegative:
                    for (int i = 0; i < decCountPlusOne; i++)
                        if (decIdx != i)
                            result += confusionTableWeights[decIdx][i];
                    break;

                //all the remaining animals that were incorrectly labeled as cats
                case ConfusionMatrixElement.FalsePositive:
                    for (int i = 0; i < decCountPlusOne; i++)
                        if (decIdx != i)
                            result += confusionTableWeights[i][decIdx];
                    break;

                //all the remaining animals, correctly classified as non-cats
                case ConfusionMatrixElement.TrueNegative:
                    for (int i = 0; i < decCountPlusOne; i++)
                        if (decIdx != i)
                            result += confusionTableWeights[i][i];
                    break;
            }
            return result;
        }

        public double GetAUC(long decision)
        {
            double truePositiveRate =
                (double)this.GetConfusionTable(decision, ConfusionMatrixElement.TruePositive)
                / (double)(this.GetConfusionTable(decision, ConfusionMatrixElement.TruePositive)
                   + this.GetConfusionTable(decision, ConfusionMatrixElement.FalseNegative));

            double falsePositiveRate =
                (double)this.GetConfusionTable(decision, ConfusionMatrixElement.FalsePositive)
                / (double)(this.GetConfusionTable(decision, ConfusionMatrixElement.FalsePositive)
                    + this.GetConfusionTable(decision, ConfusionMatrixElement.TrueNegative));

            return (1.0 + truePositiveRate - falsePositiveRate) / 2.0;
        }        

        public double GetAUCWeights(long decision)
        {
            double truePositiveRate =
                (double)this.GetConfusionTableWeights(decision, ConfusionMatrixElement.TruePositive)
                / (double)(this.GetConfusionTableWeights(decision, ConfusionMatrixElement.TruePositive)
                   + this.GetConfusionTableWeights(decision, ConfusionMatrixElement.FalseNegative));

            double falsePositiveRate =
                (double)this.GetConfusionTableWeights(decision, ConfusionMatrixElement.FalsePositive)
                / (double)(this.GetConfusionTableWeights(decision, ConfusionMatrixElement.FalsePositive)
                    + this.GetConfusionTableWeights(decision, ConfusionMatrixElement.TrueNegative));

            return (1.0 + truePositiveRate - falsePositiveRate) / 2.0;
        }        

        public double DecisionApriori(long decisionValue)
        {
            return (double)this.DecisionTotal(decisionValue) / (double)this.TestData.NumberOfRecords;
        }

        public int DecisionTotal(long decisionValue)
        {
            int total = 0;
            int decIdx = value2index[decisionValue];
            for (int i = 1; i < decCountPlusOne; i++)
                total += confusionTable[decIdx][i];
            return total;
        }

        public static string ResultHeader()
        {
            return new ClassificationResult().ToString(
                "H;" + ClassificationResult.OutputColumns,
                new ClassificationResultFormatter('|'));            
        }

        public override string ToString()
        {
            return this.ToString(
                ClassificationResult.OutputColumns, 
                new ClassificationResultFormatter('|'));                       
        }
        
        public virtual string ToString(string format, IFormatProvider formatProvider)
        {
            if (formatProvider != null)
            {
                ICustomFormatter formatter = formatProvider.GetFormat(this.GetType()) as ICustomFormatter;
                if (formatter != null)
                    return formatter.Format(format, this, formatProvider);
            }

            if (String.IsNullOrEmpty(format))
                format = "G";

            switch (format)
            {
                //TODO print confision table
                case "C":
                    return this.ToString();

                case "T":
                case "G":
                default:
                    return this.ToString();
            }
        }

        #endregion Methods
    }
}