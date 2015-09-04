using System;
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

            private Dictionary<long, double> decisionSupport;
            private Dictionary<long, double> decisionConfidence;
            private Dictionary<long, double> decisionCoverage;
            private Dictionary<long, double> decisionRatio;
            private Dictionary<long, double> decisionStrenght;

            private Dictionary<long, double> decisionWeightSupport;
            private Dictionary<long, double> decisionWeightConfidence;
            private Dictionary<long, double> decisionWeightCoverage;
            private Dictionary<long, double> decisionWeightRatio;
            private Dictionary<long, double> decisionWeightStrenght;

            private Dictionary<long, double> decisionConfidenceRelative;

            private readonly IRuleMeasure support = new RuleMeasureSupport();
            private readonly IRuleMeasure confidence = new RuleMeasureConfidence();
            private readonly IRuleMeasure coverage = new RuleMeasureCoverage();
            private readonly IRuleMeasure ratio = new RuleMeasureRatio();
            private readonly IRuleMeasure strenght = new RuleMeasureStrenght();

            private readonly IRuleMeasure weightSupport = new RuleMeasureWeightSupport();
            private readonly IRuleMeasure weightConfidence = new RuleMeasureWeightConfidence();
            private readonly IRuleMeasure weightCoverage = new RuleMeasureWeightCoverage();
            private readonly IRuleMeasure weightRatio = new RuleMeasureWeightRatio();
            private readonly IRuleMeasure weightStrenght = new RuleMeasureWeightStrenght();

            private readonly IRuleMeasure confidenceRelative = new RuleMeasureConfidenceRelative();

            private Dictionary<string, long> identifiedDecision;

            #endregion

            #region Constructors

            public DecisionRuleDescriptor(int numberOfDecisionValues)
            {
                decisionSupport = new Dictionary<long, double>(numberOfDecisionValues);
                decisionConfidence = new Dictionary<long, double>(numberOfDecisionValues);
                decisionCoverage = new Dictionary<long, double>(numberOfDecisionValues);
                decisionRatio = new Dictionary<long, double>(numberOfDecisionValues);
                decisionStrenght = new Dictionary<long, double>(numberOfDecisionValues);

                decisionWeightSupport = new Dictionary<long, double>(numberOfDecisionValues);
                decisionWeightConfidence = new Dictionary<long, double>(numberOfDecisionValues);
                decisionWeightCoverage = new Dictionary<long, double>(numberOfDecisionValues);
                decisionWeightRatio = new Dictionary<long, double>(numberOfDecisionValues);
                decisionWeightStrenght = new Dictionary<long, double>(numberOfDecisionValues);

                decisionConfidenceRelative = new Dictionary<long, double>(numberOfDecisionValues);

                identifiedDecision = new Dictionary<string, long>(6);
            }            

            #endregion

            #region Properties
            
            public IRuleMeasure Support { get { return support; } }
            public IRuleMeasure Confidence { get { return confidence; } }
            public IRuleMeasure Coverage { get { return coverage; } }
            public IRuleMeasure WeightSupport { get { return weightSupport; } }
            public IRuleMeasure WeightConfidence { get { return weightConfidence; } }
            public IRuleMeasure WeightCoverage { get { return weightCoverage; } }
            public IRuleMeasure ConfidenceRelative { get { return confidenceRelative; } }

            #endregion

            #region Methods

            public void AddDescription(long decision, IReduct reduct, EquivalenceClass eqClass)
            {                
                decisionSupport.Add(decision, support.Calc(decision, reduct, eqClass));
                decisionConfidence.Add(decision, confidence.Calc(decision, reduct, eqClass));
                decisionCoverage.Add(decision, coverage.Calc(decision, reduct, eqClass));
                
                decisionRatio.Add(decision, ratio.Calc(decision, reduct, eqClass));
                decisionStrenght.Add(decision, strenght.Calc(decision, reduct, eqClass));

                decisionWeightSupport.Add(decision, weightSupport.Calc(decision, reduct, eqClass));
                decisionWeightConfidence.Add(decision, weightConfidence.Calc(decision, reduct, eqClass));
                decisionWeightCoverage.Add(decision, weightCoverage.Calc(decision, reduct, eqClass));
                
                decisionWeightRatio.Add(decision, weightRatio.Calc(decision, reduct, eqClass));
                decisionWeightStrenght.Add(decision, weightStrenght.Calc(decision, reduct, eqClass));

                decisionConfidenceRelative.Add(decision, confidenceRelative.Calc(decision, reduct, eqClass));
            }

            public void IdentifyDecision()
            {
                identifiedDecision.Add(support.Description(), FindMaxValue(decisionSupport));
                identifiedDecision.Add(confidence.Description(), FindMaxValue(decisionConfidence));
                identifiedDecision.Add(coverage.Description(), FindMaxValue(decisionCoverage));

                identifiedDecision.Add(weightSupport.Description(), FindMaxValue(decisionWeightSupport));
                identifiedDecision.Add(weightConfidence.Description(), FindMaxValue(decisionWeightConfidence));
                identifiedDecision.Add(weightCoverage.Description(), FindMaxValue(decisionWeightCoverage));
            }

            private long FindMaxValue(Dictionary<long, double> dictionary)
            {
                long decision = -1;
                double maxValue = Double.MinValue;

                foreach (KeyValuePair<Int64, double> kvp in dictionary)
                {                    
                    if (kvp.Value.CompareTo(maxValue + 0.00000001) > 0)
                    {
                        maxValue = kvp.Value;
                        decision = kvp.Key;
                    }
                }
                return decision;
            }

            public long GetIdentifiedDecision(string measureKey)
            {
                return identifiedDecision[measureKey];
            }            

            public double GetSupport(long decisionValue)
            {
                double result = 0;
                if(decisionSupport.TryGetValue(decisionValue, out result))
                    return result;
                return 0;
            }

            public double GetCoverage(long decisionValue)
            {
                double result = 0;
                if (decisionCoverage.TryGetValue(decisionValue, out result))
                    return result;
                return 0;
            }

            public double GetConfidence(long decisionValue)
            {
                double result = 0;
                if (decisionConfidence.TryGetValue(decisionValue, out result))
                    return result;               
                return 0;
            }

            public double GetRatio(long decisionValue)
            {
                double result = 0;
                if (decisionRatio.TryGetValue(decisionValue, out result))
                    return result;
                return 0;
            }

            public double GetStrenght(long decisionValue)
            {
                double result = 0;
                if (decisionStrenght.TryGetValue(decisionValue, out result))
                    return result;
                return 0;
            }

            public double GetWeightSupport(long decisionValue)
            {
                double result = 0;
                if (decisionWeightSupport.TryGetValue(decisionValue, out result))
                    return result;
                return 0;
            }

            public double GetWeightCoverage(long decisionValue)
            {
                double result = 0;
                if (decisionWeightCoverage.TryGetValue(decisionValue, out result))
                    return result;
                return 0;
            }

            public double GetWeightConfidence(long decisionValue)
            {
                double result = 0;
                if (decisionWeightConfidence.TryGetValue(decisionValue, out result))
                    return result;
                return 0;
            }

            public double GetWeightRatio(long decisionValue)
            {
                double result = 0;
                if (decisionWeightRatio.TryGetValue(decisionValue, out result))
                    return result;
                return 0;
            }

            public double GetWeightStrenght(long decisionValue)
            {
                double result = 0;
                if (decisionWeightStrenght.TryGetValue(decisionValue, out result))
                    return result;
                return 0;
            }

            public double GetConfidenceRelative(long decisionValue)
            {
                double result = 0;
                if (decisionConfidenceRelative.TryGetValue(decisionValue, out result))
                    return result;
                return 0;
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

            public double Weight { get; set; }

            #endregion

            #region Constructors

            public ReductRuleDescriptor()
            {
                reductDescriptorMap = new Dictionary<IReduct, DecisionRuleDescriptor>();
                this.Weight = 1.0;
            }

            public ReductRuleDescriptor(double weight)
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
            /// <returns>An IEnumerator instance.</returns>
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

        private Dictionary<long, ReductRuleDescriptor> objectReductDescriptorMap;
        private IReductStore reductStore;
        private IReductStoreCollection reductStoreCollection;

        #endregion        

        #region Properties

        public IReductStore ReductStore
        {
            get { return this.reductStore; }
            set { this.reductStore = value; }                        
        }

        public IReductStoreCollection ReductStoreCollection
        {
            get { return this.reductStoreCollection; }
            set { this.reductStoreCollection = value; }
        }
        #endregion

        #region Constructors

        public RoughClassifier()
        {
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
        public void Train(DataStore trainingData, string reductFactoryKey, double epsilon, PermutationCollection permutations)
        {
            Args args = new Args();
            args.AddParameter("DataStore", trainingData);
            args.AddParameter("ApproximationRatio", epsilon);
            args.AddParameter("NumberOfThreads", 32);
            args.AddParameter("PermutationCollection", permutations);
            args.AddParameter("FactoryKey", reductFactoryKey);            

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(args);

            reductGenerator.Epsilon = epsilon;            
            reductGenerator.Generate();
            this.ReductStore = reductGenerator.ReductPool;
            
            //TODO number of groups should be passed in a different way
            this.ReductStoreCollection = reductGenerator.GetReductStoreCollection(Int32.MaxValue);
        }

        public void Train(DataStore trainingData, string reductFactoryKey, double epsilon, int numberOfPermutations)
        {
            Args args = new Args();
            args.AddParameter("DataStore", trainingData);
            args.AddParameter("ApproximationRatio", epsilon);
            //args.AddParameter("USECACHE", null);

            IPermutationGenerator permGen = ReductFactory.GetReductFactory(reductFactoryKey).GetPermutationGenerator(args);
            PermutationCollection permutations = permGen.Generate(numberOfPermutations);
            
            Train(trainingData, reductFactoryKey, epsilon, permutations);
        }

        public void Train(DataStore trainingtData, IReductStore reductStore)
        {
            this.reductStore = reductStore;

            IReductStoreCollection localReductStoreCollection = new ReductStoreCollection();
            localReductStoreCollection.AddStore(reductStore);

            this.reductStoreCollection = localReductStoreCollection;
        }        

        public IReductStoreCollection Classify(DataStore dataStore, string reductMeasureKey, int numberOfReducts, IReductStoreCollection reductStoreCollection)
        {
            IReductStore localReductStore;
            IReductStoreCollection localReductStoreCollection = new ReductStoreCollection();
            
            if (String.IsNullOrEmpty(reductMeasureKey) == false)
            {
                foreach (IReductStore rs in reductStoreCollection)
                {
                    Comparer<IReduct> reductComparer = ReductFactory.GetReductComparer(reductMeasureKey);
                    localReductStore = rs.FilterReducts(numberOfReducts, reductComparer);
                    localReductStoreCollection.AddStore(localReductStore);
                }
            }
            else
            {
                localReductStoreCollection = reductStoreCollection;
            }
                        
            this.objectReductDescriptorMap = new Dictionary<long, ReductRuleDescriptor>(dataStore.NumberOfRecords);
            foreach (int objectIndex in dataStore.GetObjectIndexes())
            {
                DataRecordInternal record = dataStore.GetRecordByIndex(objectIndex);
                this.objectReductDescriptorMap.Add(record.ObjectId, this.CalcReductDescriptiors(record, localReductStoreCollection));
            }
            return localReductStoreCollection;
        }

        public IReductStoreCollection Classify(DataStore dataStore, string reductMeasureKey, int numberOfReducts)
        {
            return this.Classify(dataStore, reductMeasureKey, numberOfReducts, this.ReductStoreCollection);
        }

        public IReductStoreCollection Classify(DataStore dataStore)
        {
            return this.Classify(dataStore, null, 0);
        }

        private ReductRuleDescriptor CalcReductDescriptiors(DataRecordInternal record, IReductStoreCollection reductStoreCollection)
        {
            ReductRuleDescriptor reductRuleDescriptor = new ReductRuleDescriptor();

            foreach (IReductStore rs in reductStoreCollection)
            {                                
                foreach (IReduct reduct in rs)
                {                                        
                    int[] attributes = reduct.Attributes.ToArray();
                    long[] values = new long[attributes.Length];
                    for (int i = 0; i < attributes.Length; i++)
                        values[i] = record[attributes[i]];

                    AttributeValueVector dataVector = new AttributeValueVector(attributes, values, false);
                    EquivalenceClass eqClass = reduct.EquivalenceClassMap.GetEquivalenceClass(dataVector);

                    //DecisionRuleDescriptor decisionRuleDescriptor = new DecisionRuleDescriptor(reduct.ObjectSetInfo.NumberOfDecisionValues, rs.Weight);
                    DecisionRuleDescriptor decisionRuleDescriptor = new DecisionRuleDescriptor(reduct.ObjectSetInfo.NumberOfDecisionValues);
                    foreach (long decisionValue in reduct.ObjectSetInfo.GetDecisionValues())
                        decisionRuleDescriptor.AddDescription(decisionValue, reduct, eqClass);

                    decisionRuleDescriptor.IdentifyDecision();
                    reductRuleDescriptor.AddDecisionRuleDescriptor(reduct, decisionRuleDescriptor);

                    //TODO This is OK if a single model contains only one single reduct
                    //TODO It is also OK if all reducts in the model share the same object w
                    //TODO This is not OK, if we have many reduct store collections and/or reducts have different object w
                    reductRuleDescriptor.Weight = reduct.Weights[record.ObjectIdx];
                }
            }
            return reductRuleDescriptor;
        }        

        public double[] GetDiscernibilityVector(DataStore data, double[] weights, IReduct reduct, IdentificationType decisionIdentificationType)
        {
            double[] result = new double[data.NumberOfRecords];
            foreach(long objectId in data.GetObjectIds())
            {
                int objectIdx = data.ObjectId2ObjectIndex(objectId);
                ReductRuleDescriptor reductDescriptor = this.objectReductDescriptorMap[objectId];
                DecisionRuleDescriptor ruleDescriptor = reductDescriptor.GetDecisionRuleDescriptor(reduct);
                
                long decision = -1;
                switch (decisionIdentificationType)
                {
                    case IdentificationType.Support:
                        decision = ruleDescriptor.GetIdentifiedDecision(ruleDescriptor.Support.Description());
                        break;

                    case IdentificationType.Confidence:
                        decision = ruleDescriptor.GetIdentifiedDecision(ruleDescriptor.Confidence.Description());
                        break;

                    case IdentificationType.Coverage:
                        decision = ruleDescriptor.GetIdentifiedDecision(ruleDescriptor.Coverage.Description());
                        break;

                    case IdentificationType.WeightSupport:
                        decision = ruleDescriptor.GetIdentifiedDecision(ruleDescriptor.WeightSupport.Description());
                        break;

                    case IdentificationType.WeightConfidence:
                        decision = ruleDescriptor.GetIdentifiedDecision(ruleDescriptor.WeightConfidence.Description());
                        break;

                    case IdentificationType.WeightCoverage:
                        decision = ruleDescriptor.GetIdentifiedDecision(ruleDescriptor.WeightCoverage.Description());
                        break;

                    default:
                        throw new ArgumentException("Unknown value", "identificationType");
                }
                
                if (decision == data.GetDecisionValue(objectIdx))
                {
                    result[objectIdx] = weights[objectIdx];
                }
            }

            return result;
            
        }

        public ClassificationResult Vote(DataStore dataStore, IdentificationType identificationType, VoteType voteType, double[] weights)
        {
            ClassificationResult classificationResult = new ClassificationResult(dataStore);

            foreach (int objectIndex in dataStore.GetObjectIndexes())
            {
                DataRecordInternal record = dataStore.GetRecordByIndex(objectIndex);
                
                long result = this.VoteObject(record, identificationType, voteType);                
                classificationResult.AddResult(dataStore.ObjectIndex2ObjectId(objectIndex), //objectId
                                               result,//predicted class
                                               dataStore.GetDecisionValue(objectIndex), //actual class
                                               weights != null ? weights[objectIndex] : 1.0 / dataStore.NumberOfRecords); //object weight from model
            }

            return classificationResult;
        }
                
        private long VoteObject(DataRecordInternal record, IdentificationType identificationType, VoteType voteType)
        {
            Dictionary<long, double> ensebleVotes = new Dictionary<long, double>();
            
            foreach (ReductStore rs in this.ReductStoreCollection)
            {
                long result = -1;
                double maxWeight = 0;
                Dictionary<long, double> decisionVotes = new Dictionary<long, double>();

                //TODO ReductDescriptor should be created per ReductStoreCollection

                ReductRuleDescriptor reductDescriptor = this.objectReductDescriptorMap[record.ObjectId];
                foreach (DecisionRuleDescriptor decisionRuleDescriptor in reductDescriptor)
                {
                    long decision = -1;

                    switch (identificationType)
                    {
                        case IdentificationType.Support:
                            decision = decisionRuleDescriptor.GetIdentifiedDecision(decisionRuleDescriptor.Support.Description());
                            break;

                        case IdentificationType.Confidence:
                            decision = decisionRuleDescriptor.GetIdentifiedDecision(decisionRuleDescriptor.Confidence.Description());
                            break;

                        case IdentificationType.Coverage:
                            decision = decisionRuleDescriptor.GetIdentifiedDecision(decisionRuleDescriptor.Coverage.Description());
                            break;

                        case IdentificationType.WeightSupport:
                            decision = decisionRuleDescriptor.GetIdentifiedDecision(decisionRuleDescriptor.WeightSupport.Description());
                            break;

                        case IdentificationType.WeightConfidence:
                            decision = decisionRuleDescriptor.GetIdentifiedDecision(decisionRuleDescriptor.WeightConfidence.Description());
                            break;

                        case IdentificationType.WeightCoverage:
                            decision = decisionRuleDescriptor.GetIdentifiedDecision(decisionRuleDescriptor.WeightCoverage.Description());
                            break;

                        default:
                            throw new ArgumentException("Unknown value", "identificationType");
                    }

                    double voteWeight = 0.0;
                    switch (voteType)
                    {
                        case VoteType.Support:
                            voteWeight = decisionRuleDescriptor.GetSupport(decision);
                            break;

                        case VoteType.Confidence:
                            voteWeight = decisionRuleDescriptor.GetConfidence(decision);
                            break;

                        case VoteType.Coverage:
                            voteWeight = decisionRuleDescriptor.GetCoverage(decision);
                            break;

                        case VoteType.Ratio:
                            voteWeight = decisionRuleDescriptor.GetRatio(decision);
                            break;

                        case VoteType.Strength:
                            voteWeight = decisionRuleDescriptor.GetStrenght(decision);
                            break;

                        case VoteType.MajorDecision:
                            voteWeight = 1.0;
                            break;

                        case VoteType.WeightSupport:
                            voteWeight = decisionRuleDescriptor.GetWeightSupport(decision);
                            break;

                        case VoteType.WeightConfidence:
                            voteWeight = decisionRuleDescriptor.GetWeightConfidence(decision);
                            break;

                        case VoteType.WeightCoverage:
                            voteWeight = decisionRuleDescriptor.GetWeightCoverage(decision);
                            break;

                        case VoteType.WeightRatio:
                            voteWeight = decisionRuleDescriptor.GetWeightRatio(decision);
                            break;

                        case VoteType.WeightStrength:
                            voteWeight = decisionRuleDescriptor.GetWeightStrenght(decision);
                            break;

                        case VoteType.ConfidenceRelative:
                            voteWeight = decisionRuleDescriptor.GetConfidenceRelative(decision);
                            break;

                        default:
                            throw new ArgumentException("Unknown value", "voteType");
                    }

                    double voteWeightSum = 0.0;
                    if (decisionVotes.TryGetValue(decision, out voteWeightSum))
                    {
                        voteWeightSum += voteWeight;
                    }
                    else
                    {
                        voteWeightSum = voteWeight;
                    }
                    decisionVotes[decision] = voteWeightSum;

                    if (voteWeightSum > maxWeight)
                    {
                        maxWeight = voteWeightSum;
                        result = decision;
                    }
                }

                double ensembleWeightSum = 0.0;
                if (ensebleVotes.TryGetValue(result, out ensembleWeightSum))
                {
                    ensembleWeightSum += rs.Weight;
                    ensebleVotes[result] = ensembleWeightSum;
                }
                else
                {
                    ensebleVotes.Add(result, rs.Weight);
                }
                
            }

            long ensembleResult = -1;
            double maxValue = Double.MinValue;
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

        public bool IsObjectRecognizable(DataStore dataStore, long objectId, IReduct reduct, IdentificationType identificationType)
        {

            ReductRuleDescriptor reductDescriptor = this.objectReductDescriptorMap[objectId];
            DecisionRuleDescriptor ruleDescriptor = reductDescriptor.GetDecisionRuleDescriptor(reduct);
                        
            long identifiedDecision = -1;

            switch (identificationType)
            {
                case IdentificationType.Support:
                    identifiedDecision = ruleDescriptor.GetIdentifiedDecision(ruleDescriptor.Support.Description());
                    break;

                case IdentificationType.Confidence:
                    identifiedDecision = ruleDescriptor.GetIdentifiedDecision(ruleDescriptor.Confidence.Description());
                    break;

                case IdentificationType.Coverage:
                    identifiedDecision = ruleDescriptor.GetIdentifiedDecision(ruleDescriptor.Coverage.Description());
                    break;

                case IdentificationType.WeightSupport:
                    identifiedDecision = ruleDescriptor.GetIdentifiedDecision(ruleDescriptor.WeightSupport.Description());
                    break;

                case IdentificationType.WeightConfidence:
                    identifiedDecision = ruleDescriptor.GetIdentifiedDecision(ruleDescriptor.WeightConfidence.Description());
                    break;

                case IdentificationType.WeightCoverage:
                    identifiedDecision = ruleDescriptor.GetIdentifiedDecision(ruleDescriptor.WeightCoverage.Description());
                    break;

                default:
                    throw new ArgumentException("Unknown value", "identificationType");
            }
            
            int objectIdx = dataStore.ObjectId2ObjectIndex(objectId);
            if (dataStore.GetDecisionValue(objectIdx) == identifiedDecision)
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
                    sb.Append(dataStoreInfo.GetFieldInfo(attr[i]).NameAlias).Append(' ');
                sb.AppendLine();

                foreach (EquivalenceClass eq in reduct.EquivalenceClassMap)
                {
                    sb.AppendLine(String.Format("{0} => {1}={2}", 
                                    eq.Instance.ToString2(dataStoreInfo), 
                                    dataStoreInfo.GetDecisionFieldInfo().NameAlias,
                                    dataStoreInfo.GetDecisionFieldInfo().Internal2External(eq.MajorDecision)));
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        #endregion
    }
}
