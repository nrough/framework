using System;
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
        public IEnumerable<long> DecisionValues { get; set; }
        public IReductStoreCollection ReductStoreCollection { get; set; }        
        public bool UseExceptionRules { get; set; }
        public RuleQualityFunction IdentificationFunction { get; set; }
        public RuleQualityFunction VoteFunction { get; set; }
        
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

        public void PrintClassification(DataRecordInternal record)
        {
            var ruleVotesSum = new Dictionary<long, decimal>();
            foreach (IReductStore rs in this.ReductStoreCollection)
            {
                if (rs.IsActive)
                {
                    foreach (IReduct reduct in rs)
                    {
                        if (UseExceptionRules == false && reduct.IsException)
                            continue;

                        var decisionIdentification = new Dictionary<long, decimal>();
                        long identifiedDecision = -1;
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

                            identifiedDecision = decisionIdentification.FindMaxValue();
                        }
                        else
                        {
                            identifiedDecision = -1;
                        }

                        if (ruleVotesSum.ContainsKey(identifiedDecision))
                            ruleVotesSum[identifiedDecision] += (this.VoteFunction(identifiedDecision, reduct, eqClass) * rs.Weight);
                        else
                            ruleVotesSum.Add(identifiedDecision, this.VoteFunction(identifiedDecision, reduct, eqClass) * rs.Weight);

                        Console.Write("{0}: ", this.IdentificationFunction.Method.Name);
                        foreach (var kvp in decisionIdentification)
                            Console.Write("{0}->{1} ", kvp.Key, kvp.Value);
                        Console.Write(Environment.NewLine);

                        Console.WriteLine("Identified decision {0}", identifiedDecision);
                        Console.WriteLine("Vote weight {0}", this.VoteFunction(identifiedDecision, reduct, eqClass) * rs.Weight);

                        if (this.UseExceptionRules == true && reduct.IsException && eqClass.NumberOfObjects > 0)
                            break;
                    }
                }
            }

            Console.Write("{0}: ", this.VoteFunction.Method.Name);
            foreach (var kvp in ruleVotesSum)
                Console.Write("{0}->{1} ", kvp.Key, kvp.Value);
            Console.Write(Environment.NewLine);
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

            if (weights == null)
            {
                double w = 1.0 / testData.NumberOfRecords;
                Parallel.For(0, testData.NumberOfRecords, objectIndex =>
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
                Parallel.For(0, testData.NumberOfRecords, objectIndex =>
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
            var ruleVotesSum = new Dictionary<long, decimal>();
            foreach (IReductStore rs in this.ReductStoreCollection)
            {
                if (rs.IsActive)
                {
                    foreach (IReduct reduct in rs)
                    {
                        if (UseExceptionRules == false && reduct.IsException)
                            continue;

                        var decisionIdentification = new Dictionary<long, decimal>();
                        long identifiedDecision = -1;
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

                            identifiedDecision = decisionIdentification.FindMaxValue();
                        }
                        else
                        {
                            identifiedDecision = -1;
                        }

                        if (ruleVotesSum.ContainsKey(identifiedDecision))
                            ruleVotesSum[identifiedDecision] += (this.VoteFunction(identifiedDecision, reduct, eqClass) * rs.Weight);
                        else
                            ruleVotesSum.Add(identifiedDecision, this.VoteFunction(identifiedDecision, reduct, eqClass) * rs.Weight);

                        if (this.UseExceptionRules == true && reduct.IsException && eqClass != null && eqClass.NumberOfObjects > 0)
                            break;
                    }
                }
            }

            return ruleVotesSum;
        }

        public Dictionary<long, decimal> IdentifyDecisions(
            DataRecordInternal record, 
            IReduct reduct)
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
