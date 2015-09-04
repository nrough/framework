using System;
using System.Collections;
using System.Collections.Generic;
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
            #region Globals

            private Dictionary<Int64, double> decisionSupport;
            private Dictionary<Int64, double> decisionConfidence;
            private Dictionary<Int64, double> decisionCoverage;
            private Dictionary<Int64, double> decisionRatio;
            private Dictionary<Int64, double> decisionStrenght;

            private Dictionary<Int64, double> decisionWeightSupport;
            private Dictionary<Int64, double> decisionWeightConfidence;
            private Dictionary<Int64, double> decisionWeightCoverage;
            private Dictionary<Int64, double> decisionWeightRatio;
            private Dictionary<Int64, double> decisionWeightStrenght;

            private Dictionary<Int64, double> decisionConfidenceRelative;

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

            private Dictionary<string, Int64> identifiedDecision;

            #endregion

            #region Constructors

            public DecisionRuleDescriptor(int numberOfDecisionValues)
            {
                decisionSupport = new Dictionary<Int64, double>(numberOfDecisionValues);
                decisionConfidence = new Dictionary<Int64, double>(numberOfDecisionValues);
                decisionCoverage = new Dictionary<Int64, double>(numberOfDecisionValues);
                decisionRatio = new Dictionary<Int64, double>(numberOfDecisionValues);
                decisionStrenght = new Dictionary<Int64, double>(numberOfDecisionValues);

                decisionWeightSupport = new Dictionary<Int64, double>(numberOfDecisionValues);
                decisionWeightConfidence = new Dictionary<Int64, double>(numberOfDecisionValues);
                decisionWeightCoverage = new Dictionary<Int64, double>(numberOfDecisionValues);
                decisionWeightRatio = new Dictionary<Int64, double>(numberOfDecisionValues);
                decisionWeightStrenght = new Dictionary<Int64, double>(numberOfDecisionValues);

                decisionConfidenceRelative = new Dictionary<Int64, double>(numberOfDecisionValues);

                identifiedDecision = new Dictionary<string, Int64>(4);
            }

            #endregion

            #region Properties

            public IRuleMeasure Support
            {
                get { return support; }
            }

            public IRuleMeasure Confidence
            {
                get { return confidence; }
            }

            public IRuleMeasure Coverage
            {
                get { return coverage; }
            }

            public IRuleMeasure WeightSupport
            {
                get { return weightSupport; }
            }

            public IRuleMeasure WeightConfidence
            {
                get { return weightConfidence; }
            }

            public IRuleMeasure WeightCoverage
            {
                get { return weightCoverage; }
            }

            public IRuleMeasure ConfidenceRelative
            {
                get { return confidenceRelative; }
            }

            #endregion

            #region Methods

            public void AddDescription(long decision, IReduct reduct, EquivalenceClass reductStatistic)
            {
                
                decisionSupport.Add(decision, support.Calc(decision, reduct, reductStatistic));
                decisionConfidence.Add(decision, confidence.Calc(decision, reduct, reductStatistic));
                decisionCoverage.Add(decision, coverage.Calc(decision, reduct, reductStatistic));
                decisionRatio.Add(decision, ratio.Calc(decision, reduct, reductStatistic));
                decisionStrenght.Add(decision, strenght.Calc(decision, reduct, reductStatistic));

                decisionWeightSupport.Add(decision, weightSupport.Calc(decision, reduct, reductStatistic));
                decisionWeightConfidence.Add(decision, weightConfidence.Calc(decision, reduct, reductStatistic));
                decisionWeightCoverage.Add(decision, weightCoverage.Calc(decision, reduct, reductStatistic));
                decisionWeightRatio.Add(decision, weightRatio.Calc(decision, reduct, reductStatistic));
                decisionWeightStrenght.Add(decision, weightStrenght.Calc(decision, reduct, reductStatistic));

                decisionConfidenceRelative.Add(decision, confidenceRelative.Calc(decision, reduct, reductStatistic));
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

            private static long FindMaxValue(Dictionary<Int64, double> dictionary)
            {
                long decision = -1;
                double maxValue = -1;

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
                {
                    return result;
                }
                return 0;
            }

            public double GetCoverage(long decisionValue)
            {
                double result = 0;
                if (decisionCoverage.TryGetValue(decisionValue, out result))
                {
                    return result;
                }
                return 0;
            }

            public double GetConfidence(long decisionValue)
            {
                double result = 0;
                if (decisionConfidence.TryGetValue(decisionValue, out result))
                {
                    return result;
                }
                return 0;
            }

            public double GetRatio(long decisionValue)
            {
                double result = 0;
                if (decisionRatio.TryGetValue(decisionValue, out result))
                {
                    return result;
                }
                return 0;
            }

            public double GetStrenght(long decisionValue)
            {
                double result = 0;
                if (decisionStrenght.TryGetValue(decisionValue, out result))
                {
                    return result;
                }
                return 0;
            }

            public double GetWeightSupport(long decisionValue)
            {
                double result = 0;
                if (decisionWeightSupport.TryGetValue(decisionValue, out result))
                {
                    return result;
                }
                return 0;
            }

            public double GetWeightCoverage(long decisionValue)
            {
                double result = 0;
                if (decisionWeightCoverage.TryGetValue(decisionValue, out result))
                {
                    return result;
                }
                return 0;
            }

            public double GetWeightConfidence(long decisionValue)
            {
                double result = 0;
                if (decisionWeightConfidence.TryGetValue(decisionValue, out result))
                {
                    return result;
                }
                return 0;
            }

            public double GetWeightRatio(long decisionValue)
            {
                double result = 0;
                if (decisionWeightRatio.TryGetValue(decisionValue, out result))
                {
                    return result;
                }
                return 0;
            }

            public double GetWeightStrenght(long decisionValue)
            {
                double result = 0;
                if (decisionWeightStrenght.TryGetValue(decisionValue, out result))
                {
                    return result;
                }
                return 0;
            }

            public double GetConfidenceRelative(long decisionValue)
            {
                double result = 0;
                if (decisionConfidenceRelative.TryGetValue(decisionValue, out result))
                {
                    return result;
                }
                return 0;
            }

            #endregion
        }
        
        #endregion

        #region ReductRuleDescriptor

        [Serializable]
        private class ReductRuleDescriptor : IEnumerable<DecisionRuleDescriptor>
        {
            #region Globals

            private Dictionary<IReduct, DecisionRuleDescriptor> reductDescriptorMap;

            #endregion

            #region Constructors

            public ReductRuleDescriptor()
            {
                reductDescriptorMap = new Dictionary<IReduct, DecisionRuleDescriptor>();
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

        #region Globals

        private Dictionary<Int64, ReductRuleDescriptor> objectReductDescriptorMap;
        private IReductStore reductStore;

        #endregion

        #region Constructors

        public RoughClassifier()
        {
        }

        #endregion

        #region Properties

        public IReductStore ReductStore
        {
            get { return this.reductStore; }
            set { this.reductStore = value; }
                        
        } 
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="trainingData"></param>
        /// <param name="reductFactoryKey"></param>
        /// <param name="approximationRatio">Value from range 0 to 99</param>
        /// <param name="permutations"></param>
        public void Train(DataStore trainingData, string reductFactoryKey, int approximationRatio, PermutationCollection permutations)
        {
            Args args = new Args();
            args.AddParameter("DataStore", trainingData);
            args.AddParameter("ApproximationRatio", approximationRatio);
            args.AddParameter("NumberOfThreads", 32);
            args.AddParameter("PermutationCollection", permutations);
            args.AddParameter("FactoryKey", reductFactoryKey);
            //args.AddParameter("USECACHE", true);

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(args);

            reductGenerator.ApproximationDegree = approximationRatio;
            //this.reductStore = reductGenerator.Generate(args).First();
            reductGenerator.Generate();
            this.reductStore = reductGenerator.ReductPool;
        }

        public void Train(DataStore trainingData, string reductFactoryKey, int approximationRatio, int numberOfPermutations)
        {
            Args args = new Args();
            args.AddParameter("DataStore", trainingData);
            args.AddParameter("ApproximationRatio", approximationRatio);
            //args.AddParameter("USECACHE", null);

            IPermutationGenerator permGen = ReductFactory.GetReductFactory(reductFactoryKey).GetPermutationGenerator(args);
            PermutationCollection permutations = permGen.Generate(numberOfPermutations);
            
            Train(trainingData, reductFactoryKey, approximationRatio, permutations);
        }

        public void Train(DataStore trainingtData, IReductStore reductStore)
        {
            this.reductStore = reductStore;
        }

        public IReductStore Classify(DataStore dataStore, string reductMeasureKey, int numberOfReducts, IReductStore reductStore)
        {

            Comparer<IReduct> reductComparer = ReductFactory.GetReductComparer(reductMeasureKey);

            IReductStore localReductStore = reductStore.FilterReducts(numberOfReducts, reductComparer);

            this.objectReductDescriptorMap = new Dictionary<Int64, ReductRuleDescriptor>(dataStore.NumberOfRecords);
            
            foreach(int objectIndex in dataStore.GetObjectIndexes())
            {
                DataRecordInternal record = dataStore.GetRecordByIndex(objectIndex);
                this.objectReductDescriptorMap.Add(record.ObjectId, this.CalcReductDescriptiors(record, localReductStore));
            }

            return localReductStore;
        }

        public IReductStore Classify(DataStore dataStore, string reductMeasureKey, int numberOfReducts)
        {
            return this.Classify(dataStore, reductMeasureKey, numberOfReducts, this.reductStore);
        }

        public IReductStore Classify(DataStore dataStore)
        {
            return this.Classify(dataStore, null, 0);
        }

        private ReductRuleDescriptor CalcReductDescriptiors(DataRecordInternal record, IReductStore reductStore)
        {
            ReductRuleDescriptor reductRuleDescrptor = new ReductRuleDescriptor();

            foreach (IReduct reduct in reductStore)
            {
                int[] attributeArray = reduct.Attributes.ToArray();
                Int64[] dataVectorArray = new Int64[attributeArray.Length];
                for (int i = 0; i < attributeArray.Length; i++)
                {
                    dataVectorArray[i] = record[attributeArray[i]];
                }

                DataVector dataVector = new DataVector(dataVectorArray, false);
                EquivalenceClass reductStatistic = reduct.EquivalenceClassMap.GetEquivalenceClass(dataVector);

                DecisionRuleDescriptor decisionRuleDescriptor = new DecisionRuleDescriptor(reduct.ObjectSetInfo.NumberOfDecisionValues);
                foreach (long decisionValue in reduct.ObjectSetInfo.GetDecisionValues())
                {
                    decisionRuleDescriptor.AddDescription(decisionValue, reduct, reductStatistic);   
                }

                decisionRuleDescriptor.IdentifyDecision();
                reductRuleDescrptor.AddDecisionRuleDescriptor(reduct, decisionRuleDescriptor);
            }

            return reductRuleDescrptor;            
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

        public ClassificationResult Vote(DataStore dataStore, IdentificationType identificationType, VoteType voteType)
        {
            ClassificationResult classificationResult = new ClassificationResult(dataStore);

            foreach (int objectIndex in dataStore.GetObjectIndexes())
            {
                DataRecordInternal record = dataStore.GetRecordByIndex(objectIndex);
                classificationResult.AddResult(dataStore.ObjectIndex2ObjectId(objectIndex),
                                               this.VoteObject(record, identificationType, voteType),
                                               dataStore.GetDecisionValue(objectIndex));
            }

            return classificationResult;
        }

        private long VoteObject(DataRecordInternal record, IdentificationType identificationType, VoteType voteType)
        {
            long result = -1;
            double maxWeight = 0;
            Dictionary<Int64, double> decisionVotes = new Dictionary<Int64, double>();

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
                
                double voteWeight = 0;
                switch (voteType)
                {
                    case VoteType.Support :
                        voteWeight = decisionRuleDescriptor.GetSupport(decision);
                        break;

                    case VoteType.Confidence :
                        voteWeight = decisionRuleDescriptor.GetConfidence(decision);
                        break;

                    case VoteType.Coverage :
                        voteWeight = decisionRuleDescriptor.GetCoverage(decision);
                        break;

                    case VoteType.Ratio :
                        voteWeight = decisionRuleDescriptor.GetRatio(decision);
                        break;

                    case VoteType.Strength :
                        voteWeight = decisionRuleDescriptor.GetStrenght(decision);
                        break;

                    case VoteType.MajorDecision :
                        voteWeight = 1;
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
                
                double voteWeightSum = 0;
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
            
            return result;
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

        #endregion
    }
}
