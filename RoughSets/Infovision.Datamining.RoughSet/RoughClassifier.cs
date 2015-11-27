﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class RoughClassifier
    {
        #region RuleDecisionDescriptor

        [Serializable]
        private class DecisionRuleDescriptor
        {
            #region Members
            
            private Dictionary<long, decimal> decisionQuality;
            private Dictionary<long, decimal> decisionVotes;

            #endregion

            #region Constructors
            
            public DecisionRuleDescriptor()
            {                
                this.decisionQuality = new Dictionary<long, decimal>();
                this.decisionVotes = new Dictionary<long, decimal>();
            }

            #endregion            

            #region Methods

            public void AddDescription(long decision, IReduct reduct, EquivalenceClass eqClass, RuleQualityFunction identifyFunction, RuleQualityFunction voteFunction)
            {                
                this.decisionQuality.Add(decision, identifyFunction(decision, reduct, eqClass));
                this.decisionVotes.Add(decision, voteFunction(decision, reduct, eqClass));
            }

            public long IdentifyDecision()
            {
                return decisionQuality.Count > 0 ? decisionQuality.FindMaxValue() : -1;
            }

            public decimal GetDecisionQuality(long decision)
            {
                decimal result = Decimal.Zero;
                if (decisionQuality.TryGetValue(decision, out result))
                    return result;
                return Decimal.Zero;
            }

            public decimal GetRuleVote(long decision)
            {
                decimal result = Decimal.Zero;
                if (decisionVotes.TryGetValue(decision, out result))
                    return result;
                return Decimal.Zero; 
            }

            #endregion
        }

        #endregion

        #region ReductRuleDescriptor

        [Serializable]
        private class ReductRuleDescriptor : IEnumerable<DecisionRuleDescriptor>
        {
            #region Members

            private Dictionary<IReduct, DecisionRuleDescriptor> reductDescriptorMap;

            #endregion

            #region Properties

            public decimal Weight { get; set; }

            #endregion

            #region Constructors

            public ReductRuleDescriptor()
            {
                reductDescriptorMap = new Dictionary<IReduct, DecisionRuleDescriptor>();
                this.Weight = Decimal.One;
            }

            public ReductRuleDescriptor(decimal weight)
                : this()
            {
                this.Weight = weight;
            }

            #endregion

            #region Methods

            public void AddDecisionRuleDescriptor(IReduct reduct, DecisionRuleDescriptor decisionRuleDescriptor)
            {
                reductDescriptorMap[reduct] = decisionRuleDescriptor;
            }

            public DecisionRuleDescriptor GetDecisionRuleDescriptor(IReduct reduct)
            {
                DecisionRuleDescriptor result = null;
                reductDescriptorMap.TryGetValue(reduct, out result);
                return result;
            }

            #region IEnumerable Members
            /// <summary>
            /// Returns an IEnumerator to enumerate through the reduct rule descriptor.
            /// </summary>
            /// <returns>An IEnumerator newInstance.</returns>
            public IEnumerator<DecisionRuleDescriptor> GetEnumerator()
            {
                return reductDescriptorMap.Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return reductDescriptorMap.Values.GetEnumerator();
            }

            #endregion

            #endregion
        }

        #endregion



        #region Members

        private Dictionary<long, List<ReductRuleDescriptor>> objectReductDescriptorMap;

        #endregion

        #region Properties

        public IReductStore ReductStore { get; set; }
        public IReductStoreCollection ReductStoreCollection { get; set; }
        public bool UseExceptionRules { get; set; }

        #endregion

        #region Constructors

        public RoughClassifier()
        {
            this.UseExceptionRules = true;
        }

        #endregion

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="trainingData"></param>
        /// <param name="reductFactoryKey"></param>
        /// <param name="epsilon">Value from range 0 to 99</param>
        /// <param name="permutations"></param>
        public void Train(DataStore trainingData, string reductFactoryKey, decimal epsilon, PermutationCollection permutations)
        {
            Args args = new Args();
            args.AddParameter(ReductGeneratorParamHelper.DataStore, trainingData);
            args.AddParameter(ReductGeneratorParamHelper.Epsilon, epsilon);
            args.AddParameter(ReductGeneratorParamHelper.NumberOfThreads, 32);
            args.AddParameter(ReductGeneratorParamHelper.PermutationCollection, permutations);
            args.AddParameter(ReductGeneratorParamHelper.FactoryKey, reductFactoryKey);

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(args);

            reductGenerator.Epsilon = epsilon;
            reductGenerator.Generate();
            this.ReductStore = reductGenerator.ReductPool;

            //TODO number of groups should be passed in a different way
            this.ReductStoreCollection = reductGenerator.GetReductStoreCollection(Int32.MaxValue);
        }

        public void Train(DataStore trainingData, string reductFactoryKey, decimal epsilon, int numberOfPermutations)
        {
            Args args = new Args();
            args.AddParameter(ReductGeneratorParamHelper.DataStore, trainingData);
            args.AddParameter(ReductGeneratorParamHelper.Epsilon, epsilon);
            //args.AddParameter("USECACHE", null);

            IPermutationGenerator permGen = ReductFactory.GetReductFactory(reductFactoryKey).GetPermutationGenerator(args);
            PermutationCollection permutations = permGen.Generate(numberOfPermutations);

            Train(trainingData, reductFactoryKey, epsilon, permutations);
        }

        public void Train(DataStore trainingtData, IReductStore reductStore)
        {
            this.ReductStore = reductStore;

            IReductStoreCollection localReductStoreCollection = new ReductStoreCollection();
            localReductStoreCollection.AddStore(reductStore);

            this.ReductStoreCollection = localReductStoreCollection;
        }

        public void Classify(DataStore dataStore, IReduct reduct, RuleQualityFunction identifyFunction, RuleQualityFunction voteFunction)
        {
            ReductStoreCollection localReductStoreCollection = new ReductStoreCollection();
            ReductStore localReductStore = new ReductStore();
            localReductStore.AddReduct(reduct);
            localReductStoreCollection.AddStore(localReductStore);

            this.ReductStore = localReductStore;
            this.ReductStoreCollection = localReductStoreCollection;

            this.Classify(dataStore, identifyFunction, voteFunction);
        }

        public void Classify(
            DataStore dataStore,
            IReductStoreCollection reductStoreCollection,
            RuleQualityFunction identifyFunction,
            RuleQualityFunction voteFunction)
        {
            this.objectReductDescriptorMap = new Dictionary<long, List<ReductRuleDescriptor>>(dataStore.NumberOfRecords);
            foreach (int objectIndex in dataStore.GetObjectIndexes())
            {
                DataRecordInternal record = dataStore.GetRecordByIndex(objectIndex);
                this.objectReductDescriptorMap.Add(record.ObjectId, this.CalcReductDescriptiors(record, reductStoreCollection, identifyFunction, voteFunction));
            }
        }

        public IReductStoreCollection Classify(
            DataStore dataStore,
            string reductMeasureKey,
            int numberOfReducts,
            IReductStoreCollection reductStoreCollection,
            RuleQualityFunction identifyFunction,
            RuleQualityFunction voteFunction)
        {
            IReductStore localReductStore;
            //TODO Code smell!
            IReductStoreCollection localReductStoreCollection = new ReductStoreCollection();

            if (reductStoreCollection != null)
            {
                if (!String.IsNullOrEmpty(reductMeasureKey))
                {
                    foreach (IReductStore rs in reductStoreCollection)
                    {
                        if (rs.IsActive)
                        {
                            Comparer<IReduct> reductComparer = ReductFactory.GetReductComparer(reductMeasureKey);
                            localReductStore = rs.FilterReducts(numberOfReducts, reductComparer);
                            localReductStoreCollection.AddStore(localReductStore);
                        }
                    }
                }
                else
                {
                    localReductStoreCollection = reductStoreCollection;
                }
            }

            this.objectReductDescriptorMap = new Dictionary<long, List<ReductRuleDescriptor>>(dataStore.NumberOfRecords);
            foreach (int objectIndex in dataStore.GetObjectIndexes())
            {
                DataRecordInternal record = dataStore.GetRecordByIndex(objectIndex);
                this.objectReductDescriptorMap.Add(record.ObjectId, this.CalcReductDescriptiors(record, localReductStoreCollection, identifyFunction, voteFunction));
            }
            return localReductStoreCollection;
        }

        public IReductStoreCollection Classify(
            DataStore dataStore,
            string reductMeasureKey,
            int numberOfReducts,
            RuleQualityFunction identifyFunction,
            RuleQualityFunction voteFunction)
        {
            return this.Classify(dataStore, reductMeasureKey, numberOfReducts, this.ReductStoreCollection, identifyFunction, voteFunction);
        }

        public IReductStoreCollection Classify(DataStore dataStore,
            RuleQualityFunction identifyFunction,
            RuleQualityFunction voteFunction)
        {
            return this.Classify(dataStore, null, 0, identifyFunction, voteFunction);
        }

        private List<ReductRuleDescriptor> CalcReductDescriptiors(
            DataRecordInternal record, 
            IReductStoreCollection reductStoreCollection, 
            RuleQualityFunction identifyFunction, 
            RuleQualityFunction voteFunction)
        {
            List<ReductRuleDescriptor> result = new List<ReductRuleDescriptor>(reductStoreCollection.Count);

            foreach (IReductStore rs in reductStoreCollection)
            {
                if (rs.IsActive)
                {
                    ReductRuleDescriptor reductRuleDescriptor = new ReductRuleDescriptor();
                    reductRuleDescriptor.Weight = rs.Weight;
                    foreach (IReduct reduct in rs)
                    {
                        if (UseExceptionRules == false && reduct.IsException)
                            continue;

                        long[] values = new long[reduct.Attributes.Count];
                        int i = 0;
                        foreach (int attribute in reduct.Attributes)
                        {
                            values[i] = record[attribute];
                            i++;
                        }

                        DecisionRuleDescriptor decisionRuleDescriptor = new DecisionRuleDescriptor();
                        EquivalenceClass eqClass = reduct.EquivalenceClasses.GetEquivalenceClass(values);
                        if (eqClass != null)
                        {
                            foreach (long decisionValue in reduct.ObjectSetInfo.GetDecisionValues())
                                decisionRuleDescriptor.AddDescription(decisionValue, reduct, eqClass, identifyFunction, voteFunction);
                        }

                        decisionRuleDescriptor.IdentifyDecision();
                        reductRuleDescriptor.AddDecisionRuleDescriptor(reduct, decisionRuleDescriptor);

                        if (this.UseExceptionRules == true && reduct.IsException && eqClass != null && eqClass.NumberOfObjects > 0)
                            break;
                    }

                    result.Add(reductRuleDescriptor);
                }
            }
            return result;
        }

        public double[] GetDiscernibilityVector(DataStore data, decimal[] weights, IReduct reduct)
        {
            double[] result = new double[data.NumberOfRecords];
            foreach (long objectId in data.GetObjectIds())
            {
                int objectIdx = data.ObjectId2ObjectIndex(objectId);
                if (this.IsObjectRecognizable(data, objectId, reduct))
                {
                    result[objectIdx] = (double)weights[objectIdx];
                }
            }

            return result;

        }

        public ClassificationResult Vote(DataStore dataStore, RuleQualityFunction identificationFunction, RuleQualityFunction voteFunction, decimal[] weights)
        {
            ClassificationResult classificationResult = new ClassificationResult(dataStore);

            foreach (int objectIndex in dataStore.GetObjectIndexes())
            {
                DataRecordInternal record = dataStore.GetRecordByIndex(objectIndex);

                long result = this.VoteObject(record, identificationFunction, voteFunction);
                classificationResult.AddResult(dataStore.ObjectIndex2ObjectId(objectIndex), //objectId
                                               result,//predicted class
                                               dataStore.GetDecisionValue(objectIndex), //actual class
                                               weights != null
                                                ? (double)weights[objectIndex]
                                                : 1.0 / dataStore.NumberOfRecords); //object weight from model
            }

            return classificationResult;
        }

        private long VoteObject(DataRecordInternal record, RuleQualityFunction identificationFunction, RuleQualityFunction voteFunction)
        {
            Dictionary<long, decimal> ensebleVotes = new Dictionary<long, decimal>();
            List<ReductRuleDescriptor> list = this.objectReductDescriptorMap[record.ObjectId];

            foreach (ReductRuleDescriptor reductDescriptor in list)
            {
                long result = -1;
                decimal maxWeight = Decimal.Zero;
                Dictionary<long, decimal> decisionVotes = new Dictionary<long, decimal>();
                foreach (DecisionRuleDescriptor decisionRuleDescriptor in reductDescriptor)
                {
                    long decision = decisionRuleDescriptor.IdentifyDecision();
                    decimal voteWeight = decisionRuleDescriptor.GetRuleVote(decision);                   

                    decimal voteWeightSum = Decimal.Zero;
                    if (decisionVotes.TryGetValue(decision, out voteWeightSum))
                    {
                        voteWeightSum += voteWeight;
                        decisionVotes[decision] = voteWeightSum;
                    }
                    else
                    {
                        voteWeightSum = voteWeight;
                        decisionVotes.Add(decision, voteWeightSum);
                    }

                    if (voteWeightSum > maxWeight)
                    {
                        maxWeight = voteWeightSum;
                        result = decision;
                    }
                }

                decimal ensembleWeightSum = Decimal.Zero;
                if (ensebleVotes.TryGetValue(result, out ensembleWeightSum))
                {
                    ensembleWeightSum += reductDescriptor.Weight;
                    ensebleVotes[result] = ensembleWeightSum;
                }
                else
                {
                    ensebleVotes.Add(result, reductDescriptor.Weight);
                }
            }

            long ensembleResult = -1;
            decimal maxValue = Decimal.MinValue;
            foreach (var kvp in ensebleVotes)
            {
                if (maxValue < kvp.Value)
                {
                    maxValue = kvp.Value;
                    ensembleResult = kvp.Key;
                }
            }


            return ensembleResult;
        }

        public bool IsObjectRecognizable(DataStore dataStore, long objectId, IReduct reduct)
        {
            List<ReductRuleDescriptor> list = this.objectReductDescriptorMap[objectId];
            Dictionary<long, decimal> votes = new Dictionary<long, decimal>();

            foreach (ReductRuleDescriptor reductDescriptor in list)
            {
                long identifiedDecision = -1;

                DecisionRuleDescriptor ruleDescriptor = reductDescriptor.GetDecisionRuleDescriptor(reduct);
                identifiedDecision = ruleDescriptor.IdentifyDecision();

                decimal voteSum = Decimal.Zero;
                if (votes.TryGetValue(identifiedDecision, out voteSum))
                {
                    voteSum += reductDescriptor.Weight;
                    votes[identifiedDecision] = voteSum;
                }
                else
                {
                    votes.Add(identifiedDecision, reductDescriptor.Weight);
                }

            }

            decimal maxValue = Decimal.MinValue;
            long result = -1;
            foreach (var kvp in votes)
            {
                if (maxValue < kvp.Value)
                {
                    maxValue = kvp.Value;
                    result = kvp.Key;
                }
            }

            int objectIdx = dataStore.ObjectId2ObjectIndex(objectId);
            if (dataStore.GetDecisionValue(objectIdx) == result)
                return true;

            return false;
        }

        public string PrintDecisionRules(DataStoreInfo dataStoreInfo)
        {
            StringBuilder sb = new StringBuilder();
            foreach (IReduct reduct in this.ReductStore)
            {
                int[] attr = reduct.Attributes.ToArray();
                for (int i = 0; i < attr.Length; i++)
                    sb.Append(dataStoreInfo.GetFieldInfo(attr[i]).Alias).Append(' ');
                sb.AppendLine();

                foreach (EquivalenceClass eq in reduct.EquivalenceClasses)
                {
                    sb.AppendLine(String.Format("{0} => {1}={2}",
                                    this.PrintDecisionRuleCondition(reduct.EquivalenceClasses.Attributes, eq.Instance, dataStoreInfo),
                                    dataStoreInfo.GetDecisionFieldInfo().Alias,
                                    dataStoreInfo.GetDecisionFieldInfo().Internal2External(eq.MajorDecision)));
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private string PrintDecisionRuleCondition(int[] attributes, long[] values, DataStoreInfo dataStoreInfo)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < attributes.Length; i++)
            {
                sb.Append(String.Format("{0}={1}",
                        dataStoreInfo.GetFieldInfo(attributes[i]).Alias,
                        dataStoreInfo.GetFieldInfo(attributes[i]).Internal2External(values[i])));
                if (i != attributes.Length - 1)
                    sb.Append(" & ");
            }
            return sb.ToString();
        }

        #endregion
    }
}
