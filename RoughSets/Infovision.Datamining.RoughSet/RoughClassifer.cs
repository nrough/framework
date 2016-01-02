﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{   
    [Serializable]
    public class RoughClassifier
    {
        private IReductStoreCollection reductStoreCollection { get; set; }
        private int numberOfModels;
        private bool allModelsAreEqual;
        
        public IEnumerable<long> DecisionValues { get; set; }
        public bool UseExceptionRules { get; set; }
        public RuleQualityFunction IdentificationFunction { get; set; }
        public RuleQualityFunction VoteFunction { get; set; }

        public IReductStoreCollection ReductStoreCollection 
        {
            get { return this.reductStoreCollection; }
            protected set 
            {
                this.reductStoreCollection = value;
                if (reductStoreCollection != null)
                {
                    numberOfModels = this.reductStoreCollection.Where(rs => rs.IsActive).Count();
                    allModelsAreEqual = this.reductStoreCollection.Where(rs => rs.IsActive).GroupBy(rs => rs.Weight).Count() > 1;
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
            IEnumerable<long> decisionValues)
        {
            this.ReductStoreCollection = reductStoreCollection;
            this.UseExceptionRules = true;
            this.IdentificationFunction = identificationFunction;
            this.VoteFunction = voteFunction;
            this.DecisionValues = decisionValues;
        }        

        public Dictionary<long, decimal> Classify(DataRecordInternal record, IReduct reduct)
        {
            var decisionIdentification = new Dictionary<long, decimal>();
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
        
        public ClassificationResult Classify(DataStore testData, decimal[] weights = null)
        {
            ClassificationResult result = new ClassificationResult(testData, this.DecisionValues);

            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = Environment.ProcessorCount;
            //options.MaxDegreeOfParallelism = 1;

            if (weights == null)
            {
                double w = 1.0 / testData.NumberOfRecords;
                
                //for(int objectIndex = 0; objectIndex<testData.NumberOfRecords; objectIndex++)
                Parallel.For(0, testData.NumberOfRecords, options, objectIndex =>
                {
                    DataRecordInternal record = testData.GetRecordByIndex(objectIndex);
                    var prediction = this.Classify(record);
                    var max = prediction.FindMaxValuePair();

                    result.AddResult(
                        record.ObjectId,
                        max.Key,
                        record[testData.DataStoreInfo.DecisionFieldId],
                        w);
                });
            }
            else
            {
                //for (int objectIndex = 0; objectIndex < testData.NumberOfRecords; objectIndex++)
                Parallel.For(0, testData.NumberOfRecords, options, objectIndex =>
                {
                    DataRecordInternal record = testData.GetRecordByIndex(objectIndex);
                    var prediction = this.Classify(record);
                    var max = prediction.FindMaxValuePair();

                    result.AddResult(
                        record.ObjectId,
                        max.Key,
                        record[testData.DataStoreInfo.DecisionFieldId],
                        (double)weights[objectIndex]);
                });
            }

            return result;
        }

        public Dictionary<long, decimal> Classify(DataRecordInternal record)
        {
            var globalStoreVoresSum = new Dictionary<long, decimal>();
            foreach (IReductStore rs in this.ReductStoreCollection.Where(r => r.IsActive))
            {
                var reductVotesSum = new Dictionary<long, decimal>();
                foreach (IReduct reduct in rs)
                {
                    if (UseExceptionRules == false && reduct.IsException)
                        continue;

                    var decisionIdentification = new Dictionary<long, decimal>();
                    KeyValuePair<long, decimal> identifiedDecision = new KeyValuePair<long, decimal>(-1, Decimal.Zero);
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

                        identifiedDecision = decisionIdentification.FindMaxValuePair();
                    }                    

                    //Voting and Identification is the same method, no need to calculate again the same 
                    if (this.VoteFunction.Equals(this.IdentificationFunction))
                    {
                        if (reductVotesSum.ContainsKey(identifiedDecision.Key))
                            reductVotesSum[identifiedDecision.Key] += identifiedDecision.Value;
                        else
                            reductVotesSum.Add(identifiedDecision.Key, identifiedDecision.Value);
                    }
                    else
                    {
                        if (reductVotesSum.ContainsKey(identifiedDecision.Key))
                            reductVotesSum[identifiedDecision.Key] += this.VoteFunction(identifiedDecision.Key, reduct, eqClass);
                        else
                            reductVotesSum.Add(identifiedDecision.Key, this.VoteFunction(identifiedDecision.Key, reduct, eqClass));
                    }

                    if (this.UseExceptionRules == true && reduct.IsException && eqClass != null && eqClass.NumberOfObjects > 0)
                        break;
                }

                if(this.numberOfModels == 1)
                    return reductVotesSum;

                if(this.allModelsAreEqual)
                {
                    foreach(var kvp in reductVotesSum)
                    {
                        if(globalStoreVoresSum.ContainsKey(kvp.Key))
                            globalStoreVoresSum[kvp.Key] += kvp.Value;
                        else
                            globalStoreVoresSum.Add(kvp.Key, kvp.Value);
                    }
                }
                else
                {
                    long maxDecision = reductVotesSum.FindMaxValuePair().Key;
                    
                    if(globalStoreVoresSum.ContainsKey(maxDecision))
                        globalStoreVoresSum[maxDecision] += rs.Weight;
                    else
                        globalStoreVoresSum.Add(maxDecision, rs.Weight);                    
                }
            }

            return globalStoreVoresSum;
        }

        public Dictionary<long, decimal> IdentifyDecisions(DataRecordInternal record, IReduct reduct)
        {
            var votes = new Dictionary<long, decimal>();
            EquivalenceClass eqClass = reduct.EquivalenceClasses.GetEquivalenceClass(record);
            if (eqClass != null)
            {
                foreach (long decisionValue in reduct.ObjectSetInfo.GetDecisionValues())
                {
                    if (votes.ContainsKey(decisionValue))
                        votes[decisionValue] += this.IdentificationFunction(decisionValue, reduct, eqClass);
                    else
                        votes.Add(decisionValue, this.IdentificationFunction(decisionValue, reduct, eqClass));
                }
            }
            else
            {
                if (votes.ContainsKey(-1))
                    votes[-1] += this.IdentificationFunction(-1, reduct, null);
                else
                    votes[-1] = this.IdentificationFunction(-1, reduct, null);
            }

            return votes;
        }

        public bool IsObjectRecognizable(DataStore data, int objectIdx, IReduct reduct)
        {
            DataRecordInternal record = data.GetRecordByIndex(objectIdx, false);
            var decisions = this.IdentifyDecisions(record, reduct);
            long result = decisions.Count > 0 ? decisions.FindMaxValue() : -1;
            if (record[data.DataStoreInfo.DecisionFieldId] == result)
                return true;
            return false;
        }

        public double[] GetDiscernibilityVector(DataStore data, decimal[] weights, IReduct reduct)
        {
            double[] result = new double[data.NumberOfRecords];
            foreach (int objectIdx in data.GetObjectIndexes())
                if (this.IsObjectRecognizable(data, objectIdx, reduct))
                    result[objectIdx] = (double)weights[objectIdx];
            return result;
        }
    }
}
