﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Math;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    //TODO Move to Infovision.Datamining namespace
    //TODO IReductStoreCollection must implement some other interface from Infovision.Datamining namespace
    internal class NaiveBayes : IClassifier
    {
        #region Members

        protected readonly Stopwatch timer = new Stopwatch();

        private int numberOfModels;
        private bool allModelsAreEqual;

        private int decCount;
        private int decCountPlusOne;
        private long[] decisions;
        private Dictionary<long, int> dec2index;

        public RuleQualityFunction IdentificationFunction { get; set; }

        private IReductStoreCollection reductStoreCollection;
        private Dictionary<int, EquivalenceClassCollection> attributeEqClasses;

        private object mutex = new object();

        #endregion Members

        #region Properties

        public ICollection<long> DecisionValues { get; set; }
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

        public bool UseExceptionRules { get; set; }
        public bool ExceptionRulesAsGaps { get; set; }
        public bool IdentifyMultipleDecision { get; set; }

        public decimal MinimumVoteValue { get; set; }

        #endregion Properties

        #region Constructors

        public NaiveBayes(
            IReductStoreCollection reductStoreCollection,
            ICollection<int> attributes,
            ICollection<long> decisionValues,
            RuleQualityFunction identificationFunction)
        {
            this.ReductStoreCollection = reductStoreCollection;
            this.UseExceptionRules = true;
            this.IdentifyMultipleDecision = true;
            this.IdentificationFunction = identificationFunction;
            //this.VoteFunction = voteFunction;
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

            this.MinimumVoteValue = Decimal.MinValue;

            //MethodInfo singleVoteMethod = ((RuleQualityFunction)RuleQuality.SingleVote).Method;
            //this.singleVoteName = singleVoteMethod.Name;
            //this.singleVoteModule = singleVoteMethod.DeclaringType.FullName;

            attributeEqClasses = new Dictionary<int, EquivalenceClassCollection>();

            foreach (var redStore in reductStoreCollection)
            {
                foreach (var red in redStore.Where(x => x.IsException == false))
                {
                    foreach (int attribute in red.Attributes)
                    {
                        if (!attributeEqClasses.ContainsKey(attribute))
                        {
                            attributeEqClasses.Add(attribute, EquivalenceClassCollection
                                .Create(new int[] { attribute }, red.DataStore, red.Epsilon, red.Weights));
                        }
                    }
                }
            }
        }

        #endregion Constructors

        public ClassificationResult Classify(DataStore testData, decimal[] weights = null, bool calcFullEquivalenceClasses = true)
        {
            timer.Reset();
            timer.Start();

            ClassificationResult result = new ClassificationResult(testData, this.DecisionValues);

            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = System.Math.Max(1, Environment.ProcessorCount)
            };
#if DEBUG
            options.MaxDegreeOfParallelism = 1;
#endif

            if (weights == null)
            {
                double w = 1.0 / testData.NumberOfRecords;
                //for(int objectIndex=0; objectIndex<testData.NumberOfRecords; objectIndex++)
                Parallel.For(0, testData.NumberOfRecords, options, objectIndex =>
                {
                    DataRecordInternal record = testData.GetRecordByIndex(objectIndex, false);
                    var prediction = this.Classify(record, calcFullEquivalenceClasses);
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
                    var prediction = this.Classify(record, calcFullEquivalenceClasses);
                    result.AddResult(
                        objectIndex,
                        prediction.FindMaxValueKey(),
                        record[testData.DataStoreInfo.DecisionFieldId],
                        (double)weights[objectIndex]);
                }
                );
            }

            timer.Stop();

            result.ClassificationTime = this.ClassificationTime;

            result.ExceptionRuleHitCounter = this.ExceptionRuleHitCounter;
            result.StandardRuleHitCounter = this.StandardRuleHitCounter;

            result.ExceptionRuleLengthSum = this.ExceptionRuleLengthSum;
            result.StandardRuleLengthSum = this.StandardRuleLengthSum;

            return result;
        }

        public Dictionary<long, decimal> Classify(DataRecordInternal record, bool calcFullEquivalenceClasses = true)
        {
            decimal[] globalVotes = new decimal[this.decCountPlusOne];
            decimal[] reductsVotes = new decimal[this.decCountPlusOne];
            decimal[] identificationWeights = new decimal[this.decCountPlusOne];
            int identifiedDecision;
            decimal identifiedDecisionWeight;

            foreach (IReductStore rs in this.ReductStoreCollection.Where(r => r.IsActive))
            {
                for (int k = 0; k < this.decCountPlusOne; k++)
                    reductsVotes[k] = Decimal.Zero;

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
                        identificationWeights[k] = Decimal.Zero;
                    identifiedDecision = 0; // -1 (unclassified)
                    identifiedDecisionWeight = Decimal.Zero;

                    EquivalenceClass eqClass = reduct.EquivalenceClasses.GetEquivalenceClass(record);

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
                            if (reduct.IsException)
                            {
                                identificationWeights[idx] = this.IdentificationFunction(decVal, reduct, eqClass);
                            }
                            else
                            {
                                //TODO Run Naive Bayes Rule
                                //TODO For Bayes rule we need prior probabilities for all decision classes in denominator
                                identificationWeights[idx] = this.IdentificationFunction(decVal, reduct, eqClass);
                            }

                            if (identifiedDecisionWeight < identificationWeights[idx])
                            {
                                identifiedDecisionWeight = identificationWeights[idx];
                                identifiedDecision = idx;
                            }
                        }

                        if (this.IdentifyMultipleDecision)
                        {
                            foreach (var decVal in eqClass.DecisionSet)
                            {
                                int idx = dec2index[decVal];
                                if (DecimalEpsilonComparer.Instance.Equals(identificationWeights[idx], identifiedDecisionWeight))
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
                    else //equivalence class not found
                    {
                        if (this.UseExceptionRules && reduct.IsException && this.ExceptionRulesAsGaps)
                            continue;
                    }

                    if (this.UseExceptionRules && reduct.IsException && eqClass != null && eqClass.WeightSum > 0)
                        break;
                }

                if (this.numberOfModels == 1)
                {
                    var result = new Dictionary<long, decimal>(this.decCountPlusOne);
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
                    decimal maxDecWeight = Decimal.Zero;
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

            var resultGlobal = new Dictionary<long, decimal>(this.decCountPlusOne);
            for (int k = 0; k < this.decCountPlusOne; k++)
                resultGlobal[decisions[k]] = globalVotes[k];

            return resultGlobal;
        }
    }
}