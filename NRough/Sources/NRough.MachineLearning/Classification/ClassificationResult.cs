using System;
using System.Collections.Generic;
using System.Linq;
using NRough.Data;
using NRough.Core;
using NRough.Core.Data;
using GenericParsing;
using System.Reflection;
using System.Linq.Dynamic;
using System.Data;
using NRough.Core.CollectionExtensions;
using LinqStatistics;
using System.IO;
using System.Globalization;
using NRough.Core.Helpers;

namespace NRough.MachineLearning.Classification
{
    

    [Serializable]
    public class ClassificationResult : IFormattable
    {
        #region Members

        public static string OutputColumns = @"ds;model;t;f;eps;ens;acc;recallmacro;,00000333precisionmacro;attr;numrul;dthm;dtha";
        
        private Dictionary<long, int> decisionValue2Index;
        private long[] decisions;
        private int decCount;
        private int decCountPlusOne;
        private int[][] confusionTable;
        private double[][] confusionTableWeights;

        private long[] predictionResults;
        private int counter;

        private double macroRecall;
        private double macroPrecision;
        private double macroFScore;

        private readonly object syncRoot = new object();

        #endregion Members

        #region Properties

        public ConfusionMatrix ConfusionMatrix { get; private set; }

        public int Count
        {
            get { return this.counter; }
        }

        public DataStore TestData { get; set; }

        [ClassificationResultValue("precisionmacro", "{0:0.0000}", false)]
        public double PrecisionMacro
        {
            get
            {
                return macroPrecision;                
            }
        }

        public double CalcPrecisionMacro()
        {
            double sum = 0.0;
            double count = 0.0;
            foreach (var dec in decisions.Where(d => d != Classifier.UnclassifiedOutput))
            {
                if (TP(dec) + FP(dec) != 0)
                {
                    sum += Precision(dec);
                    count += 1.0;
                }
            }
                
            return count > 0 ? sum / count : 0.0;
        }

        [ClassificationResultValue("precisionmicro", "{0:0.0000}", false)]
        public double PrecisionMicro
        {
            get
            {
                double a = 0.0, b = 0.0;
                foreach (var dec in decisions)
                {
                    a += TP(dec);
                    b += (TP(dec) + FP(dec));
                }
                return b > 0 ? a / b : 1.0;
            }
        }

        [ClassificationResultValue("recallmacro", "{0:0.0000}", false)]
        public double RecallMacro
        {
            get
            {
                return macroRecall;                
            }
        }

        public double CalcRecallMacro()
        {
            double sum = 0.0;
            double count = 0.0;

            foreach (var dec in decisions.Where(d => d != Classifier.UnclassifiedOutput))
            {
                if (TP(dec) + FN(dec) != 0)
                {
                    sum += Recall(dec);
                    count += 1.0;
                }                
            }
            return count > 0 ? sum / count : 0.0;
        }

        [ClassificationResultValue("recallmicro", "{0:0.0000}", false)]
        public double RecallMicro
        {
            get
            {
                double a = 0.0, b = 0.0;
                foreach (var dec in decisions)
                {
                    a += TP(dec);
                    b += (TP(dec) + FN(dec));
                }
                return b > 0 ? a / b : 1.0;
            }
        }

        [ClassificationResultValue("f1scoremacro", "{0:0.0000}", false)]
        public double F1scoreMacro
        {
            get
            {
                return macroFScore;
            }
        }

        public double CalcF1scoreMacro()
        {
            if (PrecisionMacro + RecallMacro == 0.0)
                return 0.0;

            return 2 * (PrecisionMacro * RecallMacro) / (PrecisionMacro + RecallMacro);
        }

        [ClassificationResultValue("f1scoremicro", "{0:0.0000}", false)]
        public double F1scoreMicro
        {
            get
            {
                return 2 * (PrecisionMicro * RecallMicro) / (PrecisionMicro + RecallMicro);
            }
        }

        [ClassificationResultValue("acc", "{0:0.0000}", true)]
        public double Accuracy
        {
            get
            {                
                return counter > 0 ? (double)Classified / (double)counter : 0;
            }
        }

        [ClassificationResultValue("err", "{0:0.0000}")]
        public double Error
        {
            get { return counter > 0 ? 1.0 - this.Accuracy : 0.0; }
        }

        [ClassificationResultValue("bal", "{0:0.0000}")]
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

        [ClassificationResultValue("cov", "{0:0.0000}")]
        public double Coverage
        {
            get
            {
                return this.Count > 0 ? (double)(this.Classified + this.Misclassified) / (double)this.Count : 0;
            }
        }

        [ClassificationResultValue("conf", "{0:0.0000}")]
        public double Confidence
        {
            get
            {
                int classified = this.Classified;
                int misclassified = this.Misclassified;
                return ((classified + misclassified) != 0) ? (double)classified / (double)(classified + misclassified) : 0;
            }
        }
        
        [ClassificationResultValue("cls", "{0,4}")]
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

        [ClassificationResultValue("mcls", "{0,4}")]
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

        [ClassificationResultValue("ucls", "{0,4}")]
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

        [ClassificationResultValue("wcls", "{0:0.0000}")]
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

        [ClassificationResultValue("wmcls", "{0:0.0000}")]
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

        [ClassificationResultValue("wucls", "{0:0.0000}")]
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

        [ClassificationResultValue("attr", "{0,6:0.00}", true)]
        public double AvgNumberOfAttributes { get; set; }

        [ClassificationResultValue("numrul", "{0,7:0.00}", true)]
        public double NumberOfRules { get; set; }

        [ClassificationResultValue("numexrul", "{0,7:0.00}", true)]
        public double NumberOfExceptionRules { get; set; }

        [ClassificationResultValue("dthm", "{0,5:0.00}", true)]
        public double MaxTreeHeight { get; set; }

        [ClassificationResultValue("dtha", "{0,5:0.00}", true)]
        public double AvgTreeHeight { get; set; }

        [ClassificationResultValue("mtime", "{0,6}")]
        public long ClassificationTime { get; set; }

        [ClassificationResultValue("clstime", "{0,6}")]
        public long ModelCreationTime { get; set; }

        [ClassificationResultValue("erulhit", "{0,5}")]
        public int ExceptionRuleHitCounter { get; set; }

        [ClassificationResultValue("erullen", "{0,7}")]
        public int StandardRuleHitCounter { get; set; }

        [ClassificationResultValue("srulhit", "{0,5}")]
        public int ExceptionRuleLengthSum { get; set; }

        [ClassificationResultValue("srullen", "{0,7}")]
        public int StandardRuleLengthSum { get; set; }

        [ClassificationResultValue("alpha", "{0,7:0.00}")]
        public double Alpha { get; set; }

        [ClassificationResultValue("beta", "{0,7:0.00}")]
        public double Beta { get; set; }

        [ClassificationResultValue("gamma", "{0,7:0.00}")]
        public double Gamma { get; set; }

        [ClassificationResultValue("delta", "{0,7:0.00}")]
        public double Delta { get; set; }

        [ClassificationResultValue("eps", "{0,5:0.00}", true)]
        public double Epsilon { get; set; }

        [ClassificationResultValue("ds", "{0,20}", true)]
        public string DatasetName { get; set; }

        [ClassificationResultValue("model", "{0,20}", true)]
        public string ModelName { get; set; }

        [ClassificationResultValue("t", "{0,2}", true)]
        public int TestNum { get; set; }

        [ClassificationResultValue("f", "{0,2}", true)]
        public int Fold { get; set; }

        [ClassificationResultValue("ens", "{0,3}", true)]
        public int EnsembleSize { get; set; }

        [ClassificationResultValue("desc", "{0}", false)]
        public string Description { get; set; }

        #endregion Properties

        #region Constructors

        private ClassificationResult()
        {                        
            this.EnsembleSize = 1;
        }        

        public ClassificationResult(DataStore dataStore, ICollection<long> decisionValues)
            : this()
        {            
            this.Fold = dataStore.Fold;
            this.TestData = dataStore;
            this.DatasetName = dataStore.Name;

            this.predictionResults = new long[dataStore.NumberOfRecords];

            ConfusionMatrix = new ConfusionMatrix(decisionValues.ToArray());
            
            List<long> localDecisionValues = decisionValues.ToList();
            localDecisionValues.Sort();
            this.decCount = localDecisionValues.Count;
            this.decCountPlusOne = this.decCount + 1;
            this.decisions = new long[decCountPlusOne];
            this.decisions[0] = Classifier.UnclassifiedOutput;
            this.decisionValue2Index = new Dictionary<long, int>(decCountPlusOne);
            for (int i = 1; i <= this.decCount; i++)
            {
                this.decisions[i] = localDecisionValues.ElementAt(i-1);
                this.decisionValue2Index.Add(localDecisionValues.ElementAt(i-1), i);
            }
            this.decisionValue2Index.Add(Classifier.UnclassifiedOutput, 0);          
            
            this.confusionTable = new int[decCountPlusOne][];
            this.confusionTableWeights = new double[decCountPlusOne][];
            for (int i = 0; i < decCountPlusOne; i++)
            {
                this.confusionTable[i] = new int[decCountPlusOne];
                this.confusionTableWeights[i] = new double[decCountPlusOne];
            }
            
        }

        #endregion Constructors

        #region Methods

        public void Reset()
        {
            lock (syncRoot)
            {
                predictionResults.SetAll(Classifier.UnclassifiedOutput);
                ConfusionMatrix.Reset();
                
                for (int i = 0; i < decCountPlusOne; i++)
                {
                    for (int j = 0; j < decCountPlusOne; j++)
                    {
                        confusionTable[i][j] = 0;
                        confusionTableWeights[i][j] = 0;
                    }
                }

                this.counter = 0;
                
                macroPrecision = 0.0;
                macroRecall = 0.0;
                macroFScore = 0.0;

                this.AvgNumberOfAttributes = 0.0;
                this.NumberOfRules = 0.0;
                this.MaxTreeHeight = 0.0;
                this.AvgTreeHeight = 0.0;
                this.ClassificationTime = 0;
                this.ModelCreationTime = 0;
                this.ExceptionRuleHitCounter = 0;
                this.StandardRuleHitCounter = 0;
                this.ExceptionRuleLengthSum = 0;
                this.StandardRuleLengthSum = 0;                                                
            }
        }

        public void Calc()
        {
            macroRecall = CalcRecallMacro();
            macroPrecision = CalcPrecisionMacro();
            macroFScore = CalcF1scoreMacro();
        }

        public virtual void AddResult(int objectIdx, long prediction, long actual, double weight = 1.0)
        {
            int actualDecIdx = decisionValue2Index[actual];
            int predictionDecIdx = decisionValue2Index[prediction];
            predictionResults[objectIdx] = prediction;
            lock (syncRoot)
            {                
                confusionTable[actualDecIdx][predictionDecIdx]++;
                confusionTableWeights[actualDecIdx][predictionDecIdx] += weight;
                counter++;

                Calc();
            }

            ConfusionMatrix.AddResult(actual, prediction, 1, weight);

        }

        public long GetPrediction(int objectIdx)
        {
            return predictionResults[objectIdx];
        }

        public long GetPrediction(long objectId)
        {
            return predictionResults[TestData.ObjectId2ObjectIndex(objectId)];
        }

        public long GetActual(int objectIdx)
        {
            return TestData.GetFieldValue(objectIdx, TestData.DataStoreInfo.DecisionFieldId);
        }

        public long GetActual(long objectId)
        {
            return TestData.GetFieldValue(TestData.ObjectId2ObjectIndex(objectId), TestData.DataStoreInfo.DecisionFieldId);
        }        

        /// <summary>
        /// actual cats that were correctly classified as cats
        /// </summary>
        /// <param name="decision"></param>
        /// <returns></returns>
        public int TP(long decision)
        {
            return ConfusionMatrix.TP(decision);

            /*
            int decIdx = decisionValue2Index[decision];
            return confusionTable[decIdx][decIdx];
            */
        }

        /// <summary>
        /// cats that were incorrectly marked as other animals
        /// </summary>
        /// <param name="decision"></param>
        /// <returns></returns>
        public int FN(long decision)
        {
            return ConfusionMatrix.FN(decision);
            /*
            int result = 0;
            int decIdx = decisionValue2Index[decision];
            for (int i = 0; i < decCountPlusOne; i++)
                if (decIdx != i)
                    result += confusionTable[decIdx][i];
            return result;
            */
        }

        /// <summary>
        /// all the remaining animals that were incorrectly labeled as cats
        /// </summary>
        /// <param name="decision"></param>
        /// <returns></returns>
        public int FP(long decision)
        {
            return ConfusionMatrix.FP(decision);

            /*
            int result = 0;
            int decIdx = decisionValue2Index[decision];
            for (int i = 0; i < decCountPlusOne; i++)
                if (decIdx != i)
                    result += confusionTable[i][decIdx];
            return result;
            */
        }

        /// <summary>
        /// all the remaining animals, correctly classified as non-cats
        /// </summary>
        /// <param name="decision"></param>
        /// <returns></returns>
        public int TN(long decision)
        {
            return ConfusionMatrix.TN(decision);
            /*
            int result = 0;
            int decIdx = decisionValue2Index[decision];
            for(int i = 0; i < decCountPlusOne; i++)
                if (decIdx != i)
                    result += confusionTable[i][i];
            return result;
            */
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
            for (int i = 0; i < decCountPlusOne; i++)
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
            for (int i = 0; i < decCountPlusOne; i++)
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
            for (int i = 0; i < decCountPlusOne; i++)
                if (decIdx != i)
                    result += confusionTableWeights[i][i];
            return result;
        }               

        public double AUC(long decision)
        {
            double truePositiveRate = (double)this.TP(decision) / 
                (double)(this.TP(decision) + this.FN(decision));

            double falsePositiveRate = (double)this.FP(decision) / 
                (double)(this.FP(decision) + this.TN(decision));

            return (1.0 + truePositiveRate - falsePositiveRate) / 2.0;
        }        

        public double AUCWeight(long decision)
        {
            double truePositiveRate = (double)this.TruePositiveWeight(decision) / 
                (double)(this.TruePositiveWeight(decision) + this.FalseNegativeWeight(decision));

            double falsePositiveRate = (double)this.FalsePositiveWeight(decision) / 
                (double)(this.FalsePositiveWeight(decision) + this.TrueNegativeWeight(decision));

            return (1.0 + truePositiveRate - falsePositiveRate) / 2.0;
        }

        public double Recall(long decision)
        {
            //http://stats.stackexchange.com/questions/1773/what-are-correct-values-for-precision-and-recall-in-edge-cases
            if (TP(decision) + FN(decision) == 0)
                return 1.0;

            return (double)TP(decision) / (double)(TP(decision) + FN(decision));
        }

        public double RecallWeight(long decision)
        {
            //http://stats.stackexchange.com/questions/1773/what-are-correct-values-for-precision-and-recall-in-edge-cases
            if (TruePositiveWeight(decision) + FalseNegativeWeight(decision) == 0.0)
                return 1.0;

            return TruePositiveWeight(decision) / (TruePositiveWeight(decision) + FalseNegativeWeight(decision));
        }

        public double Precision(long decision)
        {
            //http://stats.stackexchange.com/questions/1773/what-are-correct-values-for-precision-and-recall-in-edge-cases
            if (TP(decision) + FP(decision) == 0)
                return 1.0;

            return (double)TP(decision) / (double)(TP(decision) + FP(decision));
        }

        public double PrecisionWeight(long decision)
        {
            //http://stats.stackexchange.com/questions/1773/what-are-correct-values-for-precision-and-recall-in-edge-cases
            if (TruePositiveWeight(decision) + FalsePositiveWeight(decision) == 0.0)
                return 1.0;

            return TruePositiveWeight(decision) / (TruePositiveWeight(decision) + FalsePositiveWeight(decision));
        }

        public double FScore(long decision, double beta = 1.0)
        {            
            return (beta * beta + 1) * (Precision(decision) * Recall(decision)) 
                / (Precision(decision) + Recall(decision));            
        }
                
        public double FScoreWeight(long decision, double beta = 1.0)
        {
            return (beta * beta + 1) * (PrecisionWeight(decision) * RecallWeight(decision))
                / (PrecisionWeight(decision) + RecallWeight(decision));
        }

        public int DecisionTotal(long decisionValue)
        {
            int total = 0;
            int decIdx = decisionValue2Index[decisionValue];
            for (int i = 1; i < decCountPlusOne; i++)
                total += confusionTable[decIdx][i];
            return total;
        }

        public static string TableHeader(char separator = '|')
        {
            return new ClassificationResult().ToString(
                "H;" + ClassificationResult.OutputColumns,
                new ClassificationResultFormatter(separator));
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
                //TODO Latex output
                case "TEX":
                    return this.ToString();

                //TODO print confision table
                case "C":
                    return this.ToString();

                case "T":
                case "G":
                default:
                    return this.ToString();
            }
        }

        /// <summary>
        /// Method is used during result merging in cross validation.
        /// this object must be initialized with data store that contains all records, 
        /// while localResult is the result based on a single fold in CV process
        /// </summary>
        /// <param name="localResult">Classification result from classification of a single fold in cross validation</param>
        public virtual void MergeResult(ClassificationResult localResult)
        {
            if (this.decisionValue2Index.Count != localResult.decisionValue2Index.Count
                    || this.decisionValue2Index.Except(localResult.decisionValue2Index).Any())
                throw new ArgumentException("Decision sets must be equal in both classification results", "localResult");

            lock (syncRoot)
            {
                this.AvgNumberOfAttributes += localResult.AvgNumberOfAttributes;
                this.NumberOfRules += localResult.NumberOfRules;
                this.MaxTreeHeight += localResult.MaxTreeHeight;
                this.AvgTreeHeight += localResult.AvgTreeHeight;

                this.ClassificationTime += localResult.ClassificationTime;
                this.ModelCreationTime += localResult.ModelCreationTime;

                this.ExceptionRuleHitCounter += localResult.ExceptionRuleHitCounter;
                this.StandardRuleHitCounter += localResult.StandardRuleHitCounter;
                this.ExceptionRuleLengthSum += localResult.ExceptionRuleLengthSum;
                this.StandardRuleLengthSum += localResult.StandardRuleLengthSum;

                macroRecall += localResult.RecallMacro;
                macroPrecision += localResult.PrecisionMacro;
                macroFScore += localResult.F1scoreMacro;

                this.counter += localResult.counter;
                

                for (int i = 0; i < decCountPlusOne; i++)
                {
                    for (int j = 0; j < decCountPlusOne; j++)
                    {
                        confusionTable[i][j] += localResult.confusionTable[i][j];
                        confusionTableWeights[i][j] += localResult.confusionTableWeights[i][j];                        
                    }
                }

                foreach (var kvpActual in decisionValue2Index)
                {
                    foreach (var kvpPredicted in decisionValue2Index)
                    {
                        ConfusionMatrix.AddResult(
                            kvpActual.Key, kvpPredicted.Key,
                            localResult.ConfusionMatrix.GetCount(kvpActual.Key, kvpPredicted.Key),
                            localResult.ConfusionMatrix.GetWeight(kvpActual.Key, kvpPredicted.Key));
                    }
                }
            }

            foreach (var objectId in localResult.TestData.GetObjectIds())                
                this.predictionResults[this.TestData.ObjectId2ObjectIndex(objectId)] 
                    = localResult.GetPrediction(objectId);                        
        }

        public void AverageIndicators(int n)
        {
            AvgNumberOfAttributes /= (double)n;
            NumberOfRules /= (double)n;
            MaxTreeHeight /= (double)n;
            AvgTreeHeight /= (double)n;

            macroRecall /= (double)n;
            macroPrecision /= (double)n;
            macroFScore /= (double)n;

            ClassificationTime /= n;
            ModelCreationTime /= n;

            ExceptionRuleHitCounter /= n;
            StandardRuleHitCounter /= n;
            ExceptionRuleLengthSum /= n;
            StandardRuleLengthSum /= n;
        }

        /// <summary>
        /// Converts all columns to types defined by the <c>ClassificationResult</c> properties
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private static DataTable SetDataTableColumnTypes(DataTable dt)
        {
            //DataTable dtc = dt.Clone();

            PropertyInfo[] properties = typeof(ClassificationResult).GetProperties();
            foreach (var property in properties)
            {
                var resultValue = property.GetCustomAttributes(
                    typeof(ClassificationResultValueAttribute), true).FirstOrDefault() 
                    as ClassificationResultValueAttribute;

                if (resultValue != null)
                {
                    string columnName = String.IsNullOrEmpty(resultValue.Alias) ? property.Name : resultValue.Alias;
                    if(dt.Columns.Contains(columnName))
                        SetColumnType(dt, columnName, property.PropertyType);                                        
                }
            }
            /*
            foreach (DataRow row in dt.Rows)                
                dtc.ImportRow(row);
            return dtc;
            */

            return dt;
        }

        private static void SetColumnType(DataTable dt, string columnName, Type t)
        {
            if (dt == null)
                throw new ArgumentNullException("dt");
            if(t == null)
                throw new ArgumentNullException("t");            
            if (String.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");
            if (!dt.Columns.Contains(columnName))
                throw new ArgumentException(String.Format("Column {0} does not exist"), "columnName");
            
            DataColumn col = dt.Columns[columnName];
            if (col.DataType != t)
            {
                switch (Type.GetTypeCode(t))
                {
                    case TypeCode.Double :
                        col.ChangeType(t, (value) => {
                            double output = 0;
                            string text = value.ToString().Replace(',', '.');
                            Double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out output);
                            return output;
                        });
                        break;

                    default:
                        col.ChangeType(t, v => MiscHelper.ChangeType(v, t));                        
                        break;
                }
            }

            /*
            if (dt.Columns.Contains(columnName))
                dt.Columns[columnName].DataType = t;
            */

        }

        public static DataTable ReadResults(string fileName, char columnDelimiter, bool setColumnTypes = true, int expectedColumnCount = 0)
        {
            DataTable dt;

            using (StreamReader reader = File.OpenText(fileName))
            {
                //using (GenericParserAdapter gpa = new GenericParserAdapter(fileName))
                using (GenericParserAdapter gpa = new GenericParserAdapter(reader))
                {
                    gpa.ColumnDelimiter = columnDelimiter;
                    gpa.FirstRowHasHeader = true;
                    gpa.IncludeFileLineNumber = false;
                    gpa.FirstRowSetsExpectedColumnCount = true;
                    gpa.TrimResults = true;

                    //if (expectedColumnCount > 0)
                    //    gpa.ExpectedColumnCount = expectedColumnCount;

                    dt = gpa.GetDataTable();
                }
            }
            if (setColumnTypes)
                dt = SetDataTableColumnTypes(dt);
            return dt;
        }

        public static DataTable ReadResults(IEnumerable<string> fileNames, char columnDelimiter)
        {
            DataTable result = null;
            bool first = true;
            int expectedNumberOfColumns = 0;

            foreach (var fileName in fileNames)
            {                
                DataTable dt = ReadResults(fileName, columnDelimiter, first, expectedNumberOfColumns);

                if (first)
                {
                    result = dt;
                    expectedNumberOfColumns = result.Columns.Count;
                    first = false;
                }
                else                
                {
                    foreach (DataRow row in dt.Rows)
                        result.ImportRow(row);
                }                
            }                        

            return result;
        }

        public static DataTable AverageResults(DataTable dtc)
        {
            //Linq.Dynamic
            /*
            var query = dtc.AsEnumerable()
                        .GroupBy("new (it[\"ds\"].ToString() as ds, " +
                                      "it[\"model\"].ToString() as model, " +
                                      "Convert.ToInt32(it[\"ens\"]) as ens, " +
                                      "Convert.ToInt32(it[\"eps\"]) as eps)", "it")
                        .Select("new (it.Key.ds, " +
                                "it.Key.model, " +                                
                                "it.Key.ens, " +
                                "it.Key.eps, " +
                                "Min(Convert.ToDouble(it[\"acc\"])) as acc, " +
                                "Max(Convert.ToDouble(it[\"attr\"])) as attr, " +
                                "Min(Convert.ToDouble(it[\"numrul\"])) as numrul, " +
                                "Sum(Convert.ToDouble(it[\"dthm\"])) as dthm, " +
                                "Average(Convert.ToDouble(it[\"dtha\"])) as dtha)"
                                );

            return query.ToDataTable();
            */

            return (from row in dtc.AsEnumerable()
                    group row by new
                    {
                        ds = row.Field<string>("ds"),
                        model = row.Field<string>("model"),
                        eps = row.Field<double>("eps"),
                        //pruning = row.Field<string>("pruning")
                        //ens = row.Field<int>("ens")                        
                    } into grp
                    select new
                    {
                        ds = grp.Key.ds,
                        model = grp.Key.model,                        
                        eps = grp.Key.eps,
                        //pruning = grp.Key.pruning,
                        //ens = grp.Key.ens,

                        acc = grp.Average(x => x.Field<double>("acc")),
                        attr = grp.Average(x => x.Field<double>("attr")),
                        numrul = grp.Average(x => x.Field<double>("numrul")),
                        dthm = grp.Average(x => x.Field<double>("dthm")),
                        dtha = grp.Average(x => x.Field<double>("dtha"))
                        
                    }).ToDataTable();
        }

        public static DataTable AverageResults2(DataTable dtc)
        {            
            return (from row in dtc.AsEnumerable()
                    group row by new
                    {
                        ds = row.Field<string>("ds"),
                        model = row.Field<string>("model"),
                        eps = row.Field<double>("eps")                        
                    } into grp
                    select new
                    {
                        ds = grp.Key.ds,
                        model = grp.Key.model,
                        eps = grp.Key.eps,                        

                        acc = grp.Average(x => x.Field<double>("acc")),
                        accdev = grp.StandardDeviation(x => x.Field<double>("acc")),

                        attr = grp.Average(x => x.Field<double>("attr")),
                        attrdev = grp.StandardDeviation(x => x.Field<double>("attr")),

                        numrul = grp.Average(x => x.Field<double>("numrul")),
                        numruldev = grp.StandardDeviation(x => x.Field<double>("numrul")),

                        dthm = grp.Average(x => x.Field<double>("dthm")),
                        dthmdev = grp.StandardDeviation(x => x.Field<double>("dthm")),

                        dtha = grp.Average(x => x.Field<double>("dtha")),
                        dthadev = grp.StandardDeviation(x => x.Field<double>("dtha"))

                    }).ToDataTable();
        }

        public static DataTable AverageResults3(DataTable dtc)
        {
            //Linq.Dynamic
            /*
            var query = dtc.AsEnumerable()
                        .GroupBy("new (it[\"ds\"].ToString() as ds, " +
                                      "it[\"model\"].ToString() as model, " +
                                      "Convert.ToInt32(it[\"ens\"]) as ens, " +
                                      "Convert.ToInt32(it[\"eps\"]) as eps)", "it")
                        .Select("new (it.Key.ds, " +
                                "it.Key.model, " +                                
                                "it.Key.ens, " +
                                "it.Key.eps, " +
                                "Min(Convert.ToDouble(it[\"acc\"])) as acc, " +
                                "Max(Convert.ToDouble(it[\"attr\"])) as attr, " +
                                "Min(Convert.ToDouble(it[\"numrul\"])) as numrul, " +
                                "Sum(Convert.ToDouble(it[\"dthm\"])) as dthm, " +
                                "Average(Convert.ToDouble(it[\"dtha\"])) as dtha)"
                                );

            return query.ToDataTable();
            */

            return (from row in dtc.AsEnumerable()
                    group row by new
                    {
                        ds = row.Field<string>("ds"),
                        model = row.Field<string>("model"),
                        eps = row.Field<double>("eps"),
                        //pruning = row.Field<string>("pruning")
                        //ens = row.Field<int>("ens")                        
                    } into grp
                    select new
                    {
                        ds = grp.Key.ds,
                        model = grp.Key.model,
                        eps = grp.Key.eps,
                        //pruning = grp.Key.pruning,
                        //ens = grp.Key.ens,

                        acc = grp.Average(x => x.Field<double>("acc")),
                        recallmacro = grp.Average(x => x.Field<double>("recallmacro")),
                        precisionmacro = grp.Average(x => x.Field<double>("precisionmacro")),
                        attr = grp.Average(x => x.Field<double>("attr")),
                        numrul = grp.Average(x => x.Field<double>("numrul")),
                        dthm = grp.Average(x => x.Field<double>("dthm")),
                        dtha = grp.Average(x => x.Field<double>("dtha"))

                    }).ToDataTable();
        }

        public static DataTable AverageResults4(DataTable dtc)
        {
            return (from row in dtc.AsEnumerable()
                    group row by new
                    {
                        ds = row.Field<string>("ds"),
                        model = row.Field<string>("model"),
                        eps = row.Field<double>("eps")
                    } into grp
                    select new
                    {
                        ds = grp.Key.ds,
                        model = grp.Key.model,
                        eps = grp.Key.eps,

                        acc = grp.Average(x => x.Field<double>("acc")),
                        accdev = grp.StandardDeviation(x => x.Field<double>("acc")),

                        recallmacro = grp.Average(x => x.Field<double>("recallmacro")),
                        recallmacrodev = grp.StandardDeviation(x => x.Field<double>("recallmacro")),

                        precisionmacro = grp.Average(x => x.Field<double>("precisionmacro")),
                        precisionmacrodev = grp.StandardDeviation(x => x.Field<double>("precisionmacro")),

                        attr = grp.Average(x => x.Field<double>("attr")),
                        attrdev = grp.StandardDeviation(x => x.Field<double>("attr")),

                        numrul = grp.Average(x => x.Field<double>("numrul")),
                        numruldev = grp.StandardDeviation(x => x.Field<double>("numrul")),

                        dthm = grp.Average(x => x.Field<double>("dthm")),
                        dthmdev = grp.StandardDeviation(x => x.Field<double>("dthm")),

                        dtha = grp.Average(x => x.Field<double>("dtha")),
                        dthadev = grp.StandardDeviation(x => x.Field<double>("dtha"))

                    }).ToDataTable();
        }

        public static DataTable AggregateResults(DataTable dtc, string columnName)
        {
            var result =  (from row in dtc.AsEnumerable()
                    group row by new
                    {                        
                        ds = row.Field<string>("ds"),
                        model = row.Field<string>("model"),                        
                        eps = row.Field<double>("eps")
                        //pruning = row.Field<string>("pruning")
                        //ens = row.Field<int>("ens")                        
                    } into grp
                    select new
                    {
                        ds = grp.Key.ds,
                        model = grp.Key.model,                        
                        eps = grp.Key.eps,
                        //pruning = grp.Key.pruning,
                        //ens = grp.Key.ens,
                        
                        field_min = grp.Min(x => x.Field<double>(columnName)),
                        field_max = grp.Max(x => x.Field<double>(columnName)),
                        field_avg = grp.Average(x => x.Field<double>(columnName)),
                        field_dev = grp.StandardDeviation(x => x.Field<double>(columnName))
                        //field_med = grp.Median(x => x.Field<double>(columnName))
                    }).ToDataTable();

            result.Columns["field_min"].ColumnName = columnName + "_min";
            result.Columns["field_max"].ColumnName = columnName + "_max";
            result.Columns["field_avg"].ColumnName = columnName + "_avg";

            return result;
        }
        
        #endregion Methods
    }
}