using System;
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
        
        public ICollection<long> DecisionValues { get; set; }
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
            ICollection<long> decisionValues)
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

            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = System.Math.Max(1, Environment.ProcessorCount / 2)
            };
            options.MaxDegreeOfParallelism = 1;

            if (weights == null)
            {
                double w = 1.0 / testData.NumberOfRecords;
                
                //for(int objectIndex = 0; objectIndex < testData.NumberOfRecords; objectIndex++)
                Parallel.For(0, testData.NumberOfRecords, options, objectIndex =>
                {
                    DataRecordInternal record = testData.GetRecordByIndex(objectIndex, false);
                    var prediction = this.Classify(record);
                    result.AddResult(objectIndex, prediction.FindMaxValueKey(), record[testData.DataStoreInfo.DecisionFieldId], w);
                });
            }
            else
            {
                //for (int objectIndex = 0; objectIndex < testData.NumberOfRecords; objectIndex++)
                Parallel.For(0, testData.NumberOfRecords, options, objectIndex =>
                {
                    DataRecordInternal record = testData.GetRecordByIndex(objectIndex, false);
                    var prediction = this.Classify(record);
                    result.AddResult(objectIndex, prediction.FindMaxValueKey(), record[testData.DataStoreInfo.DecisionFieldId], (double)weights[objectIndex]);
                });
            }

            return result;
        }

        public Dictionary<long, decimal> Classify(DataRecordInternal record)
        {
            int decCount = this.DecisionValues.Count;
            int decCountPlusOne = this.DecisionValues.Count + 1;
            long[] decisions = new long[decCountPlusOne];
            decisions[0] = -1;
            Array.Copy(this.DecisionValues.ToArray(), 0, decisions, 1, decCount);
            
            decimal[] globalVotes = new decimal[decCountPlusOne];
            decimal[] reductsVotes = new decimal[decCountPlusOne];
            decimal[] identificationWeights = new decimal[decCountPlusOne];
            int identifiedDecision;
            decimal identifiedDecisionWeight;

            foreach (IReductStore rs in this.ReductStoreCollection.Where(r => r.IsActive))
            {
                for (int k = 0; k < decCountPlusOne; k++)
                    reductsVotes[k] = Decimal.Zero;

                foreach (IReduct reduct in rs)
                {
                    if (UseExceptionRules == false && reduct.IsException)
                        continue;

                    for (int k = 0; k < decCountPlusOne; k++)
                        identificationWeights[k] = Decimal.Zero;
                    
                    identifiedDecision = 0; // -1 (unclassified)
                    identifiedDecisionWeight = Decimal.Zero;
                    EquivalenceClass eqClass = reduct.EquivalenceClasses.GetEquivalenceClass(record);
                    if (eqClass != null)
                    {
                        for (int k = 1; k < decCountPlusOne; k++)
                        {
                            identificationWeights[k] = this.IdentificationFunction(decisions[k], reduct, eqClass);
                            if (identifiedDecisionWeight < identificationWeights[k])
                            {
                                identifiedDecisionWeight = identificationWeights[k];
                                identifiedDecision = k;
                            }
                        }
                    }                    

                    reductsVotes[identifiedDecision] += this.VoteFunction.Equals(this.IdentificationFunction) 
                        ? identifiedDecisionWeight
                        : identifiedDecision != 0 ? this.VoteFunction(decisions[identifiedDecision], reduct, eqClass)
                                                  : Decimal.Zero;

                    if (this.UseExceptionRules == true && reduct.IsException && eqClass != null && eqClass.NumberOfObjects > 0)
                        break;
                }

                if(this.numberOfModels == 1)
                {
                    var result = new Dictionary<long, decimal>(decCountPlusOne);
                    for(int k = 0; k < decCountPlusOne; k++)
                        result[decisions[k]] = reductsVotes[k];
                    return result;
                }

                if(this.allModelsAreEqual)
                {
                    for (int k = 0; k < decCountPlusOne; k++)
                        globalVotes[k] += reductsVotes[k];
                }
                else
                {
                    int maxDec = 0; // -1 (unclassified)
                    decimal maxDecWeight = Decimal.Zero;
                    for (int k = 0; k < decCountPlusOne; k++)
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

            var resultGlobal = new Dictionary<long, decimal>(decCountPlusOne);
            for(int k = 0; k < decCountPlusOne; k++)
                resultGlobal[decisions[k]] = globalVotes[k];

            return resultGlobal;
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
            long result = decisions.Count > 0 ? decisions.FindMaxValueKey() : -1;
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
