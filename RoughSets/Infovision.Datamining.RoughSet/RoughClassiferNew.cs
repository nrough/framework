using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{   
    [Serializable]
    public class ClassificationPrediction
    {
        public Dictionary<long, decimal> DecisionProbability { get; set; }
        public bool IsRecognized { get; set; }

        public ClassificationPrediction() { this.IsRecognized = false; }

        public long GetPrediction()
        {
            if(!this.IsRecognized)
                return -1;

            return this.DecisionProbability.FindMaxValue();
        }

        public decimal GetProbability(long decision)
        {
            if(decision == -1 && this.IsRecognized == true)
                return Decimal.One;

            return this.DecisionProbability[decision];
        }
    }
    
    
    [Serializable]
    public class RoughClassifierNew
    {
        public IEnumerable<long> DecisionValues { get; set; }
        public IReductStoreCollection ReductStoreCollection { get; set; }        
        public bool UseExceptionRules { get; set; }
        public RuleQualityFunction IdentificationFunction { get; set; }
        public RuleQualityFunction VoteFunction { get; set; }
        
        public RoughClassifierNew(            
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

        public ClassificationPrediction Classify(DataRecordInternal record)
        {            
            var ruleVotesSum = new Dictionary<long, decimal>();
            bool isDecisionIdentified = false;

            foreach (IReductStore rs in this.ReductStoreCollection)
            {
                if (rs.IsActive)
                {                                        
                    foreach (IReduct reduct in rs)
                    {
                        if (UseExceptionRules == false && reduct.IsException)
                            continue;

                        var decisionIdentification = new Dictionary<long,decimal>();

                        long[] values = new long[reduct.Attributes.Count];
                        int i = 0;
                        foreach (int attribute in reduct.Attributes)
                            values[i++] = record[attribute];

                        long identifiedDecision = -1;
                        EquivalenceClass eqClass = reduct.EquivalenceClasses.GetEquivalenceClass(values);                        
                        if (eqClass != null)
                        {
                            foreach (long decisionValue in reduct.ObjectSetInfo.GetDecisionValues())
                            {
                                if (decisionIdentification.ContainsKey(decisionValue))
                                    decisionIdentification[decisionValue] += this.IdentificationFunction(decisionValue, reduct, eqClass);
                                else
                                    decisionIdentification.Add(decisionValue, this.IdentificationFunction(decisionValue, reduct, eqClass));
                            }

                            identifiedDecision = decisionIdentification.FindMaxValue();
                            if (ruleVotesSum.ContainsKey(identifiedDecision))
                                ruleVotesSum[identifiedDecision] += (this.VoteFunction(identifiedDecision, reduct, eqClass) * rs.Weight);
                            else
                                ruleVotesSum.Add(identifiedDecision, this.VoteFunction(identifiedDecision, reduct, eqClass) * rs.Weight);

                            isDecisionIdentified = true;

                            if (this.UseExceptionRules == true && reduct.IsException && eqClass.NumberOfObjects > 0)
                                break;
                        }                                                                                                
                    }                    
                }
            }

            return new ClassificationPrediction()
            {
                DecisionProbability = ruleVotesSum,
                IsRecognized = isDecisionIdentified
            };
        }
        
        public ClassificationResult Classify(DataStore testData, decimal[] weights = null)
        {
            ClassificationResult result = new ClassificationResult(testData);
            foreach(int objectIndex in testData.GetObjectIndexes())
            {
                DataRecordInternal record = testData.GetRecordByIndex(objectIndex);
                ClassificationPrediction prediction = this.Classify(record);
                
                result.AddResult(
                    testData.ObjectIndex2ObjectId(objectIndex), 
                    prediction.GetPrediction(),
                    testData.GetDecisionValue(objectIndex), 
                    weights != null ? (double)weights[objectIndex] : 1.0 / testData.NumberOfRecords); 
            }

            return result;
        }
    }
}
