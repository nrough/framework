using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;
using System.Diagnostics;
using Infovision.Math;

namespace Infovision.Datamining.Roughset
{   
    [Serializable]
    public class RoughClassifier
    {
        #region Members

        private IReductStoreCollection reductStoreCollection { get; set; }
        private int numberOfModels;
        private bool allModelsAreEqual;
        private int decCount;
        private int decCountPlusOne;
        private long[] decisions;

        protected readonly Stopwatch timer = new Stopwatch();

        #endregion 

        #region Properties

        public ICollection<long> DecisionValues { get; set; }
        public bool UseExceptionRules { get; set; }
        public bool ExceptionRulesAsGaps { get; set; }
        public bool IdentifyMultipleDecision { get; set; }
        public RuleQualityFunction IdentificationFunction { get; set; }
        public RuleQualityFunction VoteFunction { get; set; }
        public decimal MinimumVoteValue { get; set; }
        public virtual long ClassificationTime { get { return timer.ElapsedMilliseconds; } }

        #endregion

        #region Constructors

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
            Array.Copy(this.DecisionValues.ToArray(), 0, this.decisions, 1, this.decCount);

            this.MinimumVoteValue = Decimal.MinValue;
        }

        #endregion

        public Dictionary<long, decimal> Classify(DataRecordInternal record, IReduct reduct)
        {
            var decisionIdentification = new Dictionary<long, decimal>(reduct.ObjectSetInfo.GetDecisionValues().Count);
            EquivalenceClass eqClass = reduct.EquivalenceClasses.GetEquivalenceClass(record);
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
                    result.AddResult(objectIndex, prediction.FindMaxValueKey(), record[testData.DataStoreInfo.DecisionFieldId], (double)weights[objectIndex]);
                }
                );
            }

            timer.Stop();

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
                        continue;

                    //TODO Check for standard Bireduct do we need to recalculate?
                    if (reduct.IsEquivalenceClassCollectionCalculated == false)
                    {
                        EquivalenceClassCollection equivalenceClasses = (reduct is Bireduct)
                                ? EquivalenceClassCollection.Create(
                                    reduct,
                                    reduct.DataStore,
                                    reduct.Weights,
                                    reduct.ObjectSet)
                                : EquivalenceClassCollection.Create(
                                    reduct.Attributes.ToArray(),
                                    reduct.DataStore,
                                    reduct.Epsilon,
                                    reduct.Weights);

                        reduct.SetEquivalenceClassCollection(equivalenceClasses);
                    }

                    for (int k = 0; k < this.decCountPlusOne; k++)
                        identificationWeights[k] = Decimal.Zero;

                    identifiedDecision = 0; // -1 (unclassified)
                    identifiedDecisionWeight = Decimal.Zero;

                    EquivalenceClass eqClass = reduct.EquivalenceClasses.GetEquivalenceClass(record);
                    
                    if (eqClass != null)
                    {
                        if (this.UseExceptionRules && reduct.IsException && this.ExceptionRulesAsGaps)
                            break;

                        for (int k = 1; k < this.decCountPlusOne; k++)
                        {
                            identificationWeights[k] = this.IdentificationFunction(decisions[k], reduct, eqClass);
                            if (identifiedDecisionWeight < identificationWeights[k])
                            {
                                identifiedDecisionWeight = identificationWeights[k];
                                identifiedDecision = k;
                            }
                        }
                    }
                    else
                    {
                        if (this.UseExceptionRules && reduct.IsException && this.ExceptionRulesAsGaps)
                            continue;
                    }                    

                    if (this.VoteFunction.Equals(this.IdentificationFunction))
                    {                        
                        if (this.IdentifyMultipleDecision)
                        {
                            for (int k = 1; k < this.decCountPlusOne; k++)
                            {
                                if (DecimalEpsilonComparer.Instance.Equals(identificationWeights[k], identifiedDecisionWeight))
                                {
                                    if ((this.MinimumVoteValue <= 0) || (identificationWeights[k] >= this.MinimumVoteValue))
                                    {
                                        reductsVotes[k] += identificationWeights[k];
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
                                for (int k = 1; k < this.decCountPlusOne; k++)
                                {
                                    if (DecimalEpsilonComparer.Instance.Equals(identificationWeights[k], identifiedDecisionWeight))
                                    {
                                        decimal vote = this.VoteFunction(decisions[k], reduct, eqClass);
                                        if ((this.MinimumVoteValue <= 0) || (vote >= this.MinimumVoteValue))
                                        {
                                            reductsVotes[k] += vote;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                decimal vote = this.VoteFunction(decisions[identifiedDecision], reduct, eqClass);
                                if ((this.MinimumVoteValue <= 0) || (vote >= this.MinimumVoteValue))
                                {
                                    reductsVotes[identifiedDecision] += vote;
                                }
                            }
                        }
                    }

                    if (this.UseExceptionRules && reduct.IsException && eqClass != null && eqClass.WeightSum > 0)
                        break;
                }

                if(this.numberOfModels == 1)
                {
                    var result = new Dictionary<long, decimal>(this.decCountPlusOne);
                    for(int k = 0; k < this.decCountPlusOne; k++)
                        result[decisions[k]] = reductsVotes[k];
                    return result;
                }

                if(this.allModelsAreEqual)
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
            for(int k = 0; k < this.decCountPlusOne; k++)
                resultGlobal[decisions[k]] = globalVotes[k];

            return resultGlobal;
        }

        public Dictionary<long, decimal> IdentifyDecision(DataRecordInternal record, IReduct reduct)
        {
            decimal[] identificationWeights = new decimal[this.decCountPlusOne];                                   
            EquivalenceClass eqClass = reduct.EquivalenceClasses.GetEquivalenceClass(record);

            //if reduct is an Exception and we found an existing rule and we treat exceptions as gaps, 
            //then we cannot identify the decision, return empty dictionary
            if (reduct.IsException && eqClass != null && this.ExceptionRulesAsGaps)
            {
                Dictionary<long, decimal> result = new Dictionary<long,decimal>(1);
                result.Add(-1, 0);
                return result;
            }

            if (eqClass != null)
            {
                Dictionary<long, decimal> result = new Dictionary<long, decimal>(eqClass.DecisionSet.Count);
                foreach (var decision in eqClass.DecisionSet)
                    result.Add(decision, this.IdentificationFunction(decision, reduct, eqClass));
                return result;                
            }

            Dictionary<long, decimal> unknown = new Dictionary<long, decimal>(1);
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
        
        public static double[] GetDiscernibilityVector(DataStore data, decimal[] weights, IReduct reduct, RuleQualityFunction decisionIndentificationMethod)
        {
            double[] result = new double[data.NumberOfRecords];
            for (int objectIdx = 0; objectIdx < data.NumberOfRecords; objectIdx++)
                if (RoughClassifier.IsObjectRecognizable(data, objectIdx, reduct, decisionIndentificationMethod))
                    result[objectIdx] = (double)weights[objectIdx];
            return result;
        }

        public static Tuple<long, decimal> IdentifyDecision(DataRecordInternal record, IReduct reduct, RuleQualityFunction decisionIndentificationMethod)
        {
            ICollection<long> decisions = reduct.EquivalenceClasses.DecisionValues();
            int decCountPlusOne = decisions.Count + 1;
            //decimal[] identificationWeights = new decimal[decCountPlusOne];
            long identifiedDecision;
            decimal identifiedDecisionWeight;

            //for (int k = 0; k < decCountPlusOne; k++)
            //    identificationWeights[k] = Decimal.Zero;

            identifiedDecision = -1; 
            identifiedDecisionWeight = Decimal.Zero;

            EquivalenceClass eqClass = reduct.EquivalenceClasses.GetEquivalenceClass(record);
            if (eqClass != null)
            {
                foreach(long decision in decisions)                
                {
                    decimal tmpDecisionWeight = decisionIndentificationMethod(decision, reduct, eqClass);
                    if (identifiedDecisionWeight < tmpDecisionWeight)
                    {
                        identifiedDecisionWeight = tmpDecisionWeight;
                        identifiedDecision = decision;
                    }
                }
            }

            return new Tuple<long, decimal>(identifiedDecision, identifiedDecisionWeight);
        }

        #endregion 
    }
}
