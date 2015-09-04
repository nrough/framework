﻿using System;
using System.Collections.Generic;
using System.Text;
using Infovision.Data;
using Infovision.Utils;

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
    public class ClassificationResult
    {
        #region ConfusionMatrixKey

        [Serializable]
        private class ConfusionMatrixKey
        {
            public long Predicted { get; set; }
            public long Actual { get; set; }

            public ConfusionMatrixKey(long predicted, long actual)
            {
                this.Predicted = predicted;
                this.Actual = actual;
            }

            #region System.Object Methods

            public override bool Equals(object obj)
            {
                if (obj == null)
                {
                    return false;
                }

                ConfusionMatrixKey p = obj as ConfusionMatrixKey;
                if (p == null)
                {
                    return false;
                }

                return p.Predicted == this.Predicted 
                    && p.Actual == this.Actual;
            }
            public override int GetHashCode()
            {
                return HashHelper.GetHashCode<Int64, Int64>(this.Predicted, this.Actual);
            }

            #endregion
        }

        #endregion

        #region Globals

        private Dictionary<Int64, Int64> classificationMap;
        private Dictionary<ConfusionMatrixKey, int> confusionMatrix = new Dictionary<ConfusionMatrixKey, int>();
        private Dictionary<Int64, int> decisionActualCount;
        
        private int numberOfClassified = 0;
        private int numberOfMisclassified = 0;
        private int numberOfUnclassified = 0;

        //TODO use classificationInfo instead
        private double qualityRatio = 0;

        #endregion

        #region Constructors

        public ClassificationResult(DataStore dataStore)
        {
            classificationMap = new Dictionary<Int64, Int64>(dataStore.DataStoreInfo.NumberOfRecords);
            decisionActualCount = new Dictionary<Int64, int>(dataStore.DataStoreInfo.NumberOfDecisionValues);
            foreach (long decisionValue in dataStore.DataStoreInfo.GetDecisionValues())
            {
                decisionActualCount[decisionValue] = 0;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Number of elements beeing classified e.g. total number of objects in the test dataset
        /// </summary>
        public int Count
        {
            get { return classificationMap.Count; }
        }

        public double Accuracy
        {
            get 
            {
                return (this.Count != 0) ? (double)numberOfClassified / (double)this.Count : 0;
            }
        }

        public double BalancedAccuracy
        {
            get
            {
                double sum = 0;
                int numberOfDecisions = 0;
                foreach (KeyValuePair<Int64, int> kvp in this.decisionActualCount)
                {
                    if (kvp.Value > 0)
                    {
                        sum += ((double)this.GetConfusionTable(kvp.Key, ConfusionMatrixElement.TruePositive) / (double)kvp.Value);
                        numberOfDecisions++;
                    }
                }

                return sum / (double) numberOfDecisions;
            }
        }

        //TODO Implement Precision and Recall
        //https://en.wikipedia.org/wiki/Accuracy_and_precision
        //http://en.wikipedia.org/wiki/Precision_and_recall

        public double Coverage
        {
            get
            {
                return (this.Count != 0) ? (double)(numberOfClassified + numberOfMisclassified) / (double)this.Count : 0; ;
            }
        }

        public double Confidence
        {
            get
            {
                return ((numberOfClassified + numberOfMisclassified) != 0) ? (double)numberOfClassified / (double)(numberOfClassified + numberOfMisclassified) : 0;
            }
        }

        public int NumberOfClassified
        {
            get { return numberOfClassified; }
        }

        public int NumberOfMisclassified
        {
            get { return numberOfMisclassified; }
        }

        public int NumberOfUnclassifed
        {
            get { return numberOfUnclassified; }
        }

        //TODO use classification info instead
        public double QualityRatio
        {
            get { return qualityRatio; }
            set { qualityRatio = value; }
        }

        #endregion

        #region Methods

        public virtual void AddResult(long objectId, long prediction)
        {
            classificationMap[objectId] = prediction;
        }
        
        public virtual void AddResult(long objectId, long prediction, long actual)
        {
            this.AddResult(objectId, prediction);
            this.AddConfusionMatrix(prediction, actual);
            
            if (prediction == actual)
            {
                numberOfClassified++;
            }
            else if (prediction != actual && prediction != -1)
            {
                numberOfMisclassified++;
            }
            else
            {
                numberOfUnclassified++;
            }
        }

        protected void AddConfusionMatrix(long prediction, long actual)
        {
            ConfusionMatrixKey key = new ConfusionMatrixKey(prediction, actual);
            if (confusionMatrix.ContainsKey(key))
            {
                confusionMatrix[key]++;
            }
            else
            {
                confusionMatrix[key] = 1;
            }

            decisionActualCount[actual] = decisionActualCount.ContainsKey(actual) ? decisionActualCount[actual] + 1: 1;
        }

        public long GetResult(long objectId)
        {
            return classificationMap[objectId];
        }

        public int GetDecisionCount(long decisionValue)
        {
            int count = 0;
            if (this.decisionActualCount.TryGetValue(decisionValue, out count))
            {
                return count;
            }
            return 0;
        }

        public int GetConfusionMatrix(long prediction, long actual)
        {
            int counter = 0;
            ConfusionMatrixKey key = new ConfusionMatrixKey(prediction, actual);
            if (confusionMatrix.TryGetValue(key, out counter))
            {
                return counter;            
            }
            return 0;
        }

        public int GetConfusionTable(long decisionValue, ConfusionMatrixElement confusionTableElement)
        {
            int result = 0;
            switch(confusionTableElement)
            {
                //actual cats that were correctly classified as cats
                case ConfusionMatrixElement.TruePositive :
                    result = this.GetConfusionMatrix(decisionValue, decisionValue);
                    break;

                //cats that were incorrectly marked as other animals
                case ConfusionMatrixElement.FalseNegative:
                    foreach (KeyValuePair<ConfusionMatrixKey, int> kvp in confusionMatrix)
                    {
                        if(kvp.Key.Actual == decisionValue 
                            && kvp.Key.Predicted != decisionValue)
                        {
                            result += kvp.Value;
                        }
                    }
                    break;

                //all the remaining animals that were incorrectly labeled as cats
                case ConfusionMatrixElement.FalsePositive:
                    foreach (KeyValuePair<ConfusionMatrixKey, int> kvp in confusionMatrix)
                    {
                        if(kvp.Key.Actual != decisionValue 
                            && kvp.Key.Predicted == decisionValue)
                        {
                            result += kvp.Value;
                        }
                    }
                    break;

                //all the remaining animals, correctly classified as non-cats
                case ConfusionMatrixElement.TrueNegative:
                    foreach (KeyValuePair<ConfusionMatrixKey, int> kvp in confusionMatrix)
                    {
                        if(kvp.Key.Actual != decisionValue
                            && kvp.Key.Predicted != decisionValue
                            && kvp.Key.Actual == kvp.Key.Predicted)
                        {
                            result += kvp.Value;
                        }
                    }
                    break;
            }
            return result;
        }

        public double DecisionApriori(long decisionValue)
        {
            return (double) this.DecisionTotal(decisionValue) / (double) this.Count;
        }

        public int DecisionTotal(long decisionValue)
        {
            int total = 0;
            foreach (KeyValuePair<ConfusionMatrixKey, int> kvp in confusionMatrix)
            {
                if(kvp.Key.Actual == decisionValue)
                {
                    total += kvp.Value;
                }
            }

            return total;
        }

        public string ResultHeader()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("NumberOfClassified");
            stringBuilder.Append('\t');
            stringBuilder.Append("NumberOfMisclassified");
            stringBuilder.Append('\t');
            stringBuilder.Append("NumberOfUnclassified");
            stringBuilder.Append('\t');
            stringBuilder.Append("Accuracy");
            stringBuilder.Append('\t');
            stringBuilder.Append("BalancedAccuracy");
            stringBuilder.Append('\t');
            stringBuilder.Append("Confidence");
            stringBuilder.Append('\t');
            stringBuilder.Append("Coverage");
            stringBuilder.Append('\t');
            stringBuilder.Append("AverageReductLength");
            stringBuilder.Append('\t');

            return stringBuilder.ToString();
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Digits(this.numberOfClassified);
            stringBuilder.Append('\t');
            stringBuilder.Digits(this.numberOfMisclassified);
            stringBuilder.Append('\t');
            stringBuilder.Digits(this.numberOfUnclassified);
            stringBuilder.Append('\t');
            stringBuilder.AppendFormat("{0:0.0000}", this.Accuracy);
            stringBuilder.Append('\t');
            stringBuilder.AppendFormat("{0:0.0000}", this.BalancedAccuracy);
            stringBuilder.Append('\t');
            stringBuilder.AppendFormat("{0:0.0000}", this.Confidence);
            stringBuilder.Append('\t');
            stringBuilder.AppendFormat("{0:0.0000}", this.Coverage);
            stringBuilder.Append('\t');
            stringBuilder.AppendFormat("{0:0.0000}", this.QualityRatio);
            stringBuilder.Append('\t');

            return stringBuilder.ToString();
        }

        #endregion
    }
}