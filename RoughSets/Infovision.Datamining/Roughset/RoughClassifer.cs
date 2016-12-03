using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Math;
using Infovision.Core;
using Infovision.MachineLearning.Classification;

namespace Infovision.MachineLearning.Roughset
{
    [Serializable]
    public class RoughClassifier
    {
        #region Members

        protected readonly Stopwatch timer = new Stopwatch();

        private int numberOfModels;
        private bool allModelsAreEqual;
        private int decCount;
        private int decCountPlusOne;
        private long[] decisions;
        private Dictionary<long, int> dec2index;
        private string singleVoteName;
        private string singleVoteModule;
        private IReductStoreCollection reductStoreCollection;
        private readonly object mutex = new object();

        #endregion Members

        #region Properties

        public long? DefaultOutput { get; set; }

        public ICollection<long> DecisionValues { get; set; }
        public bool UseExceptionRules { get; set; }
        public bool ExceptionRulesAsGaps { get; set; }
        public bool IdentifyMultipleDecision { get; set; }
        public RuleQualityFunction IdentificationFunction { get; set; }
        public RuleQualityFunction VoteFunction { get; set; }
        public double MinimumVoteValue { get; set; }
        public virtual long ClassificationTime { get { return timer.ElapsedMilliseconds; } }

        public IReductStoreCollection ReductStoreCollection
        {
            get { return this.reductStoreCollection; }
            protected set
            {
                this.reductStoreCollection = value;
                if (reductStoreCollection != null)
                {
                    numberOfModels = this.reductStoreCollection.Where(rs => rs.IsActive).Count();
                    allModelsAreEqual = !(this.reductStoreCollection.Where(rs => rs.IsActive).GroupBy(rs => rs.Weight).Count() > 1);
                }
                else
                {
                    numberOfModels = 0;
                    allModelsAreEqual = true;
                }
            }
        }

        public int ExceptionRuleHitCounter { get; protected set; }
        public int StandardRuleHitCounter { get; protected set; }
        public int ExceptionRuleLengthSum { get; protected set; }
        public int StandardRuleLengthSum { get; protected set; }

        #endregion Properties

        #region Constructors

        public RoughClassifier(
            IReductStoreCollection reductStoreCollection,
            RuleQualityFunction identificationFunction,
            RuleQualityFunction voteFunction,
            ICollection<long> decisionValues)
        {
            this.ReductStoreCollection = reductStoreCollection;
            this.UseExceptionRules = true;
            this.IdentifyMultipleDecision = true;
            this.IdentificationFunction = identificationFunction;
            this.VoteFunction = voteFunction;
            this.DecisionValues = decisionValues;

            this.decCount = decisionValues.Count;
            this.decCountPlusOne = decCount + 1;
            this.decisions = new long[this.decCountPlusOne];
            this.decisions[0] = -1;

            dec2index = new Dictionary<long, int>(this.decCount);
            int k = 1;
            foreach (long decVal in this.DecisionValues)
            {
                this.decisions[k] = decVal;
                dec2index.Add(decVal, k);
                k++;
            }

            this.MinimumVoteValue = Double.MinValue;

            MethodInfo singleVoteMethod = ((RuleQualityFunction)RuleQuality.SingleVote).Method;
            this.singleVoteName = singleVoteMethod.Name;
            this.singleVoteModule = singleVoteMethod.DeclaringType.FullName;
        }

        #endregion Constructors

        public Dictionary<long, double> Classify(DataRecordInternal record, IReduct reduct)
        {
            var decisionIdentification = new Dictionary<long, double>(reduct.ObjectSetInfo.GetDecisionValues().Count);
            EquivalenceClass eqClass = reduct.EquivalenceClasses.Find(record);
            if (eqClass != null)
            {
                foreach (long decisionValue in reduct.ObjectSetInfo.GetDecisionValues())
                {
                    if (decisionIdentification.ContainsKey(decisionValue))
                        decisionIdentification[decisionValue] += this.IdentificationFunction(decisionValue, reduct, eqClass);
                    else
                        decisionIdentification.Add(decisionValue, this.IdentificationFunction(decisionValue, reduct, eqClass));
                }
            }
            else
            {
                decisionIdentification.Add(-1, this.IdentificationFunction(-1, reduct, eqClass));
            }

            return decisionIdentification;
        }

        public ClassificationResult Classify(DataStore testData, double[] weights = null)
        {
            timer.Reset();
            timer.Start();

            ClassificationResult result = new ClassificationResult(testData, this.DecisionValues);

            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = InfovisionConfiguration.MaxDegreeOfParallelism
            };

            if (weights == null)
            {
                double w = 1.0 / testData.NumberOfRecords;
                //for(int objectIndex=0; objectIndex<testData.NumberOfRecords; objectIndex++)
                Parallel.For(0, testData.NumberOfRecords, options, objectIndex =>
                {
                    DataRecordInternal record = testData.GetRecordByIndex(objectIndex, false);
                    var prediction = this.Classify(record);
                    result.AddResult(objectIndex, prediction.FindMaxValueKey(), record[testData.DataStoreInfo.DecisionFieldId], w);
                }
                );
            }
            else
            {
                //for (int objectIndex = 0; objectIndex < testData.NumberOfRecords; objectIndex++)
                Parallel.For(0, testData.NumberOfRecords, options, objectIndex =>
                {
                    DataRecordInternal record = testData.GetRecordByIndex(objectIndex, false);
                    var prediction = this.Classify(record);
                    result.AddResult(
                        objectIndex,
                        prediction.FindMaxValueKey(),
                        record[testData.DataStoreInfo.DecisionFieldId],
                        (double)weights[objectIndex]);
                }
                );
            }

            timer.Stop();

            result.AvgNumberOfAttributes = reductStoreCollection.GetAvgMeasure(new ReductMeasureLength(), false);

            result.ClassificationTime = this.ClassificationTime;

            result.ExceptionRuleHitCounter = this.ExceptionRuleHitCounter;
            result.StandardRuleHitCounter = this.StandardRuleHitCounter;

            result.ExceptionRuleLengthSum = this.ExceptionRuleLengthSum;
            result.StandardRuleLengthSum = this.StandardRuleLengthSum;

            return result;
        }

        public Dictionary<long, double> Classify(DataRecordInternal record)
        {
            double[] globalVotes = new double[this.decCountPlusOne];
            double[] reductsVotes = new double[this.decCountPlusOne];
            double[] identificationWeights = new double[this.decCountPlusOne];
            int identifiedDecision;
            double identifiedDecisionWeight;

            foreach (IReductStore rs in this.ReductStoreCollection.Where(r => r.IsActive))
            {
                for (int k = 0; k < this.decCountPlusOne; k++)
                    reductsVotes[k] = 0.0;

                foreach (IReduct reduct in rs)
                {
                    if (this.UseExceptionRules == false && reduct.IsException)
                    {
                        lock (mutex)
                        {
                            this.ExceptionRuleHitCounter++;
                            this.ExceptionRuleLengthSum += reduct.Attributes.Count;
                        }

                        continue;
                    }

                    for (int k = 0; k < this.decCountPlusOne; k++)
                        identificationWeights[k] = 0.0;

                    identifiedDecision = 0; // -1 (unclassified)
                    identifiedDecisionWeight = 0.0;

                    EquivalenceClass eqClass = reduct.EquivalenceClasses.Find(record);

                    if (eqClass != null)
                    {
                        if (this.UseExceptionRules && reduct.IsException && this.ExceptionRulesAsGaps)
                        {
                            lock (mutex)
                            {
                                this.ExceptionRuleHitCounter++;
                                this.ExceptionRuleLengthSum += reduct.Attributes.Count;
                            }

                            break;
                        }

                        lock (mutex)
                        {
                            if (reduct.IsException)
                            {
                                this.ExceptionRuleHitCounter++;
                                this.ExceptionRuleLengthSum += reduct.Attributes.Count;
                            }
                            else
                            {
                                this.StandardRuleHitCounter++;
                                this.StandardRuleLengthSum += reduct.Attributes.Count;
                            }
                        }

                        foreach (var decVal in eqClass.DecisionSet)
                        {
                            int idx = dec2index[decVal];
                            identificationWeights[idx] = this.IdentificationFunction(decVal, reduct, eqClass);
                            if (identifiedDecisionWeight < identificationWeights[idx])
                            {
                                identifiedDecisionWeight = identificationWeights[idx];
                                identifiedDecision = idx;
                            }
                        }

                        if (this.VoteFunction.Equals(this.IdentificationFunction))
                        {
                            if (this.IdentifyMultipleDecision)
                            {
                                foreach (var decVal in eqClass.DecisionSet)
                                {
                                    int idx = dec2index[decVal];
                                    if (DoubleEpsilonComparer.Instance.Equals(identificationWeights[idx], identifiedDecisionWeight))
                                    {
                                        if ((this.MinimumVoteValue <= 0) || (identificationWeights[idx] >= this.MinimumVoteValue))
                                        {
                                            reductsVotes[idx] += identificationWeights[idx];
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if ((this.MinimumVoteValue <= 0) || (identifiedDecisionWeight >= this.MinimumVoteValue))
                                {
                                    reductsVotes[identifiedDecision] += identificationWeights[identifiedDecision];
                                }
                            }
                        }
                        else
                        {
                            if (identifiedDecision != 0)
                            {
                                if (this.IdentifyMultipleDecision)
                                {
                                    foreach (var decVal in eqClass.DecisionSet)
                                    {
                                        int idx = dec2index[decVal];
                                        if (DoubleEpsilonComparer.Instance.Equals(identificationWeights[idx], identifiedDecisionWeight))
                                        {
                                            double vote = this.VoteFunction(decVal, reduct, eqClass);
                                            if ((this.MinimumVoteValue <= 0) || (vote >= this.MinimumVoteValue))
                                            {
                                                reductsVotes[idx] += vote;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    double vote = this.VoteFunction(decisions[identifiedDecision], reduct, eqClass);
                                    if ((this.MinimumVoteValue <= 0) || (vote >= this.MinimumVoteValue))
                                    {
                                        reductsVotes[identifiedDecision] += vote;
                                    }
                                }
                            }
                            else
                            {
                                if (this.VoteFunction.Method.Name == singleVoteName
                                    && this.VoteFunction.Method.DeclaringType.FullName == singleVoteModule)
                                {
                                    reductsVotes[0] += 1;
                                }
                            }
                        }
                    }
                    else //equivalence class not found
                    {
                        if (this.UseExceptionRules && reduct.IsException && this.ExceptionRulesAsGaps)
                            continue;

                        if (!reduct.IsException)
                        {
                            if (this.VoteFunction.Method.Name == singleVoteName
                                && this.VoteFunction.Method.DeclaringType.FullName == singleVoteModule)
                            {
                                reductsVotes[0] += 1;
                            }
                        }
                    }

                    if (this.UseExceptionRules && reduct.IsException && eqClass != null && eqClass.WeightSum > 0)
                        break;
                }

                if (this.numberOfModels == 1)
                {
                    var result = new Dictionary<long, double>(this.decCountPlusOne);
                    for (int k = 0; k < this.decCountPlusOne; k++)
                        result[decisions[k]] = reductsVotes[k];
                    return result;
                }

                if (this.allModelsAreEqual)
                {
                    for (int k = 0; k < this.decCountPlusOne; k++)
                        globalVotes[k] += reductsVotes[k];
                }
                else
                {
                    int maxDec = 0; // -1 (unclassified)
                    double maxDecWeight = 0.0;
                    for (int k = 0; k < this.decCountPlusOne; k++)
                    {
                        if (maxDecWeight < reductsVotes[k])
                        {
                            maxDecWeight = reductsVotes[k];
                            maxDec = k;
                        }
                    }
                    globalVotes[maxDec] += rs.Weight;
                }
            }

            var resultGlobal = new Dictionary<long, double>(this.decCountPlusOne);
            for (int k = 0; k < this.decCountPlusOne; k++)
                resultGlobal[decisions[k]] = globalVotes[k];

            return resultGlobal;
        }

        public Dictionary<long, double> IdentifyDecision(EquivalenceClass eqClass, IReduct reduct)
        {
            //if reduct is an Exception and we found an existing rule and we treat exceptions as gaps,
            //then we cannot identify the decision, return empty dictionary
            if (reduct.IsException && eqClass != null && this.ExceptionRulesAsGaps)
            {
                Dictionary<long, double> result = new Dictionary<long, double>(1);
                result.Add(-1, 0);
                return result;
            }

            if (eqClass != null)
            {
                Dictionary<long, double> result = new Dictionary<long, double>(eqClass.DecisionSet.Count);
                foreach (var decision in eqClass.DecisionSet)
                    result.Add(decision, this.IdentificationFunction(decision, reduct, eqClass));
                return result;
            }

            Dictionary<long, double> unknown = new Dictionary<long, double>(1);
            unknown.Add(-1, 0);
            return unknown;
        }

        public Dictionary<long, double> IdentifyDecision(DataRecordInternal record, IReduct reduct)
        {
            EquivalenceClass eqClass = reduct.EquivalenceClasses.Find(record);

            //if reduct is an Exception and we found an existing rule and we treat exceptions as gaps,
            //then we cannot identify the decision, return empty dictionary
            if (reduct.IsException && eqClass != null && this.ExceptionRulesAsGaps)
            {
                Dictionary<long, double> result = new Dictionary<long, double>(1);
                result.Add(-1, 0);
                return result;
            }

            if (eqClass != null)
            {
                Dictionary<long, double> result = new Dictionary<long, double>(eqClass.DecisionSet.Count);
                foreach (var decision in eqClass.DecisionSet)
                    result.Add(decision, this.IdentificationFunction(decision, reduct, eqClass));
                return result;
            }

            Dictionary<long, double> unknown = new Dictionary<long, double>(1);
            unknown.Add(-1, 0);
            return unknown;
        }

        #region Recognition Vectors (needs improvement)

        public static bool IsObjectRecognizable(DataStore data, int objectIdx, IReduct reduct, RuleQualityFunction decisionIndentificationMethod)
        {
            DataRecordInternal record = data.GetRecordByIndex(objectIdx, false);
            var decision = RoughClassifier.IdentifyDecision(record, reduct, decisionIndentificationMethod);
            if (record[data.DataStoreInfo.DecisionFieldId] == decision.Item1)
                return true;
            return false;
        }

        public static double[] GetDiscernibilityVector(DataStore data, double[] weights, IReduct reduct, RuleQualityFunction decisionIndentificationMethod)
        {
            double[] result = new double[data.NumberOfRecords];
            for (int objectIdx = 0; objectIdx < data.NumberOfRecords; objectIdx++)
                if (RoughClassifier.IsObjectRecognizable(data, objectIdx, reduct, decisionIndentificationMethod))
                    result[objectIdx] = (double)weights[objectIdx];
            return result;
        }

        public static Tuple<long, double> IdentifyDecision(DataRecordInternal record, IReduct reduct, RuleQualityFunction decisionIndentificationMethod)
        {
            ICollection<long> decisions = reduct.DataStore.DataStoreInfo.GetDecisionValues();
            int decCountPlusOne = decisions.Count + 1;
            //double[] identificationWeights = new double[decCountPlusOne];
            long identifiedDecision;
            double identifiedDecisionWeight;

            //for (int k = 0; k < decCountPlusOne; k++)
            //    identificationWeights[k] = 0.0;

            identifiedDecision = -1;
            identifiedDecisionWeight = 0.0;

            EquivalenceClass eqClass = reduct.EquivalenceClasses.Find(record);
            if (eqClass != null)
            {
                foreach (long decision in decisions)
                {
                    double tmpDecisionWeight = decisionIndentificationMethod(decision, reduct, eqClass);
                    if (identifiedDecisionWeight < tmpDecisionWeight)
                    {
                        identifiedDecisionWeight = tmpDecisionWeight;
                        identifiedDecision = decision;
                    }
                }
            }

            return new Tuple<long, double>(identifiedDecision, identifiedDecisionWeight);
        }

        #endregion Recognition Vectors (needs improvement)
    }
}