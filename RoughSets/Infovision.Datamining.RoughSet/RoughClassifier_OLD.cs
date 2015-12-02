using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class RoughClassifier_OLD
    {
        #region RuleDecisionDescriptor

        [Serializable]
        private class DecisionRuleDescriptor
        {
            #region Members            
            
            private Dictionary<long, decimal> decisionSupport;
            private Dictionary<long, decimal> decisionConfidence;
            private Dictionary<long, decimal> decisionCoverage;
            private Dictionary<long, decimal> decisionRatio;
            private Dictionary<long, decimal> decisionStrenght;

            private Dictionary<long, decimal> decisionWeightSupport;
            private Dictionary<long, decimal> decisionWeightConfidence;
            private Dictionary<long, decimal> decisionWeightCoverage;
            private Dictionary<long, decimal> decisionWeightRatio;
            private Dictionary<long, decimal> decisionWeightStrenght;

            private Dictionary<long, decimal> decisionConfidenceRelative;

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
                decisionSupport = new Dictionary<long, decimal>(numberOfDecisionValues);
                decisionConfidence = new Dictionary<long, decimal>(numberOfDecisionValues);
                decisionCoverage = new Dictionary<long, decimal>(numberOfDecisionValues);
                decisionRatio = new Dictionary<long, decimal>(numberOfDecisionValues);
                decisionStrenght = new Dictionary<long, decimal>(numberOfDecisionValues);

                decisionWeightSupport = new Dictionary<long, decimal>(numberOfDecisionValues);
                decisionWeightConfidence = new Dictionary<long, decimal>(numberOfDecisionValues);
                decisionWeightCoverage = new Dictionary<long, decimal>(numberOfDecisionValues);
                decisionWeightRatio = new Dictionary<long, decimal>(numberOfDecisionValues);
                decisionWeightStrenght = new Dictionary<long, decimal>(numberOfDecisionValues);

                decisionConfidenceRelative = new Dictionary<long, decimal>(numberOfDecisionValues);

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

            public string DescriptionToString(IdentificationType identificationType, VoteType voteType)
            {
                StringBuilder sb = new StringBuilder();

                switch (voteType)
                {
                }
                
                switch(identificationType)
                {

                }


                return sb.ToString();
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
            
            private long FindMaxValue(Dictionary<long, decimal> dictionary)
            {
                long decision = -1;
                decimal maxValue = Decimal.MinValue;

                foreach (KeyValuePair<Int64, decimal> kvp in dictionary)
                {                    
                    if (kvp.Value > maxValue)
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

            public decimal GetSupport(long decisionValue)
            {
                decimal result = 0;
                if(decisionSupport.TryGetValue(decisionValue, out result))
                    return result;
                return 0;
            }

            public decimal GetCoverage(long decisionValue)
            {
                decimal result = 0;
                if (decisionCoverage.TryGetValue(decisionValue, out result))
                    return result;
                return 0;
            }

            public decimal GetConfidence(long decisionValue)
            {
                decimal result = 0;
                if (decisionConfidence.TryGetValue(decisionValue, out result))
                    return result;               
                return 0;
            }

            public decimal GetRatio(long decisionValue)
            {
                decimal result = 0;
                if (decisionRatio.TryGetValue(decisionValue, out result))
                    return result;
                return 0;
            }

            public decimal GetStrenght(long decisionValue)
            {
                decimal result = 0;
                if (decisionStrenght.TryGetValue(decisionValue, out result))
                    return result;
                return 0;
            }

            public decimal GetWeightSupport(long decisionValue)
            {
                decimal result = 0;
                if (decisionWeightSupport.TryGetValue(decisionValue, out result))
                    return result;
                return 0;
            }

            public decimal GetWeightCoverage(long decisionValue)
            {
                decimal result = 0;
                if (decisionWeightCoverage.TryGetValue(decisionValue, out result))
                    return result;
                return 0;
            }

            public decimal GetWeightConfidence(long decisionValue)
            {
                decimal result = 0;
                if (decisionWeightConfidence.TryGetValue(decisionValue, out result))
                    return result;
                return 0;
            }

            public decimal GetWeightRatio(long decisionValue)
            {
                decimal result = 0;
                if (decisionWeightRatio.TryGetValue(decisionValue, out result))
                    return result;
                return 0;
            }

            public decimal GetWeightStrenght(long decisionValue)
            {
                decimal result = 0;
                if (decisionWeightStrenght.TryGetValue(decisionValue, out result))
                    return result;
                return 0;
            }

            public decimal GetConfidenceRelative(long decisionValue)
            {
                decimal result = 0;
                if (decisionConfidenceRelative.TryGetValue(decisionValue, out result))
                    return result;
                return 0;
            }

            #endregion


            #region

            public Dictionary<long, decimal> GetSupportDict()
            {
                return decisionSupport;
            }

            public Dictionary<long, decimal> GetCoverageDict()
            {
                return decisionCoverage;
            }

            public Dictionary<long, decimal> GetConfidenceDict()
            {
                return decisionConfidence;
            }

            public Dictionary<long, decimal> GetRatioDict()
            {
                return decisionRatio;
            }

            public Dictionary<long, decimal> GetStrenghtDict()
            {
                return decisionStrenght;
            }

            public Dictionary<long, decimal> GetWeightSupportDict()
            {
                return decisionWeightSupport;
            }

            public Dictionary<long, decimal> GetWeightCoverageDict()
            {
                return decisionWeightCoverage;
            }

            public Dictionary<long, decimal> GetWeightConfidenceDict()
            {
                return decisionWeightConfidence;
            }

            public Dictionary<long, decimal> GetWeightRatioDict()
            {
                return decisionWeightRatio;
            }

            public Dictionary<long, decimal> GetWeightStrenghtDict()
            {
                return decisionWeightStrenght;
            }

            public Dictionary<long, decimal> GetConfidenceRelativeDict()
            {
                return decisionConfidenceRelative;
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

        public RoughClassifier_OLD()
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

        public void Classify(DataStore dataStore, IReduct reduct)
        {
            ReductStoreCollection localReductStoreCollection = new ReductStoreCollection();
            ReductStore localReductStore = new ReductStore();
            localReductStore.AddReduct(reduct);
            localReductStoreCollection.AddStore(localReductStore);

            this.ReductStore = localReductStore;
            this.ReductStoreCollection = localReductStoreCollection;
            
            this.Classify(dataStore);
        }

        public void Classify(
            DataStore dataStore, 
            IReductStoreCollection reductStoreCollection)
        {
            this.objectReductDescriptorMap = new Dictionary<long, List<ReductRuleDescriptor>>(dataStore.NumberOfRecords);
            foreach (int objectIndex in dataStore.GetObjectIndexes())
            {
                DataRecordInternal record = dataStore.GetRecordByIndex(objectIndex);
                this.objectReductDescriptorMap.Add(record.ObjectId, this.CalcReductDescriptiors(record, reductStoreCollection));
            }
        }

        public IReductStoreCollection Classify(
            DataStore dataStore, 
            string reductMeasureKey, 
            int numberOfReducts, 
            IReductStoreCollection reductStoreCollection)
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
                this.objectReductDescriptorMap.Add(record.ObjectId, this.CalcReductDescriptiors(record, localReductStoreCollection));
            }
            return localReductStoreCollection;
        }

        public IReductStoreCollection Classify(
            DataStore dataStore, 
            string reductMeasureKey, 
            int numberOfReducts)
        {
            return this.Classify(dataStore, reductMeasureKey, numberOfReducts, this.ReductStoreCollection);
        }

        public IReductStoreCollection Classify(DataStore dataStore)
        {
            return this.Classify(dataStore, null, 0);
        }

        public void PrintClassification(DataRecordInternal record, IdentificationType identificationType, VoteType voteType)
        {
            List<ReductRuleDescriptor> localReductDecriptor = new List<ReductRuleDescriptor>();
            foreach (IReductStore rs in this.ReductStoreCollection)
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
                            values[i++] = record[attribute];

                        DecisionRuleDescriptor decisionRuleDescriptor = new DecisionRuleDescriptor(reduct.ObjectSetInfo.NumberOfDecisionValues);
                        EquivalenceClass eqClass = reduct.EquivalenceClasses.GetEquivalenceClass(values);
                        if (eqClass != null)
                        {
                            foreach (long decisionValue in reduct.ObjectSetInfo.GetDecisionValues())
                                decisionRuleDescriptor.AddDescription(decisionValue, reduct, eqClass);
                        }

                        decisionRuleDescriptor.IdentifyDecision();                                                

                        switch (identificationType)
                        {
                            case IdentificationType.Support:
                                Console.Write("Support: ");
                                foreach (var kvp in decisionRuleDescriptor.GetSupportDict())
                                    Console.Write("{0}->{1} ", kvp.Key, kvp.Value);
                                Console.Write(Environment.NewLine);
                                break;

                            case IdentificationType.Confidence:
                                Console.Write("Confidence: ");
                                foreach (var kvp in decisionRuleDescriptor.GetConfidenceDict())
                                    Console.Write("{0}->{1} ", kvp.Key, kvp.Value);
                                Console.Write(Environment.NewLine);                                
                                break;

                            case IdentificationType.Coverage:
                                Console.Write("Coverage: ");
                                foreach (var kvp in decisionRuleDescriptor.GetCoverageDict())
                                    Console.Write("{0}->{1} ", kvp.Key, kvp.Value);
                                Console.Write(Environment.NewLine);
                                break;

                            case IdentificationType.WeightSupport:
                                Console.Write("WeightSupport: ");
                                foreach (var kvp in decisionRuleDescriptor.GetWeightSupportDict())
                                    Console.Write("{0}->{1} ", kvp.Key, kvp.Value);
                                Console.Write(Environment.NewLine);
                                break;

                            case IdentificationType.WeightConfidence:
                                Console.Write("WeightConfidence: ");
                                foreach (var kvp in decisionRuleDescriptor.GetWeightConfidenceDict())
                                    Console.Write("{0}->{1} ", kvp.Key, kvp.Value);
                                Console.Write(Environment.NewLine);
                                break;

                            case IdentificationType.WeightCoverage:
                                Console.Write("WeightCoverage: ");
                                foreach (var kvp in decisionRuleDescriptor.GetWeightCoverageDict())
                                    Console.Write("{0}->{1} ", kvp.Key, kvp.Value);
                                Console.Write(Environment.NewLine);
                                break;

                            default:
                                throw new ArgumentException("Unknown value", "identificationType");
                        }

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

                        Console.WriteLine("Identified decision {0}", decision);


                        reductRuleDescriptor.AddDecisionRuleDescriptor(reduct, decisionRuleDescriptor);

                        if (this.UseExceptionRules == true && reduct.IsException && eqClass != null && eqClass.NumberOfObjects > 0)
                            break;
                    }

                    localReductDecriptor.Add(reductRuleDescriptor);                    
                }
            }

            Dictionary<long, decimal> localEnsemblesVotes = new Dictionary<long, decimal>();
            foreach (ReductRuleDescriptor reductDescriptor in localReductDecriptor)
            {                
                Dictionary<long, decimal> decisionVotes = new Dictionary<long, decimal>();
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

                    decimal voteWeight = Decimal.Zero;
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
                            voteWeight = Decimal.One;
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

                    Console.WriteLine("Vote weight = {0}", voteWeight);
                }


            }
        }
        
        public ClassificationPrediction Classify(DataRecordInternal record, IdentificationType identificationType, VoteType voteType)
        {

            List<ReductRuleDescriptor> localReductDecriptor = new List<ReductRuleDescriptor>();
            foreach (IReductStore rs in this.ReductStoreCollection)
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
                            values[i++] = record[attribute];

                        DecisionRuleDescriptor decisionRuleDescriptor = new DecisionRuleDescriptor(reduct.ObjectSetInfo.NumberOfDecisionValues);
                        EquivalenceClass eqClass = reduct.EquivalenceClasses.GetEquivalenceClass(values);
                        if (eqClass != null)
                        {
                            foreach (long decisionValue in reduct.ObjectSetInfo.GetDecisionValues())
                                decisionRuleDescriptor.AddDescription(decisionValue, reduct, eqClass);
                        }

                        decisionRuleDescriptor.IdentifyDecision();
                        reductRuleDescriptor.AddDecisionRuleDescriptor(reduct, decisionRuleDescriptor);

                        if (this.UseExceptionRules == true && reduct.IsException && eqClass != null && eqClass.NumberOfObjects > 0)
                            break;
                    }

                    localReductDecriptor.Add(reductRuleDescriptor);
                }
            }

            Dictionary<long, decimal> localEnsemblesVotes = new Dictionary<long, decimal>();
            Dictionary<long, decimal> decisionVotes = new Dictionary<long, decimal>();                
            foreach (ReductRuleDescriptor reductDescriptor in localReductDecriptor)
            {
                long result = -1;                
                decisionVotes = new Dictionary<long, decimal>();                
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

                    decimal voteWeight = Decimal.Zero;
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
                            voteWeight = Decimal.One;
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

                    if (decisionVotes.ContainsKey(decision))
                        decisionVotes[decision] += voteWeight;
                    else
                        decisionVotes.Add(decision, voteWeight);
                }

                result = decisionVotes.Count > 0 ? decisionVotes.FindMaxValue() : -1;

                if (result != -1)
                {
                    if (localEnsemblesVotes.ContainsKey(result))
                        localEnsemblesVotes[result] += reductDescriptor.Weight;
                    else
                        localEnsemblesVotes.Add(result, reductDescriptor.Weight);
                }
            }

            long ensembleResult = localEnsemblesVotes.Count > 0 ? localEnsemblesVotes.FindMaxValue() : - 1;
            
            return new ClassificationPrediction()
            {
                DecisionProbability = localEnsemblesVotes,
                IsRecognized = ensembleResult == -1 ? false : true
            };            
        }
        
        private List<ReductRuleDescriptor> CalcReductDescriptiors(DataRecordInternal record, IReductStoreCollection reductStoreCollection)
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
                            values[i++] = record[attribute];                                                    
                        
                        DecisionRuleDescriptor decisionRuleDescriptor = new DecisionRuleDescriptor(reduct.ObjectSetInfo.NumberOfDecisionValues);
                        EquivalenceClass eqClass = reduct.EquivalenceClasses.GetEquivalenceClass(values);
                        if (eqClass != null)
                        {                            
                            foreach (long decisionValue in reduct.ObjectSetInfo.GetDecisionValues())
                                decisionRuleDescriptor.AddDescription(decisionValue, reduct, eqClass);
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

        public double[] GetDiscernibilityVector(DataStore data, decimal[] weights, IReduct reduct, IdentificationType decisionIdentificationType)
        {
            double[] result = new double[data.NumberOfRecords];
            foreach(long objectId in data.GetObjectIds())
            {                
                int objectIdx = data.ObjectId2ObjectIndex(objectId);
                if(this.IsObjectRecognizable(data, objectId, reduct, decisionIdentificationType))
                {
                    result[objectIdx] = (double)weights[objectIdx];
                }                                                
            }

            return result;
            
        }

        public ClassificationResult Vote(DataStore dataStore, IdentificationType identificationType, VoteType voteType, decimal[] weights)
        {
            ClassificationResult classificationResult = new ClassificationResult(dataStore);

            foreach (int objectIndex in dataStore.GetObjectIndexes())
            {
                DataRecordInternal record = dataStore.GetRecordByIndex(objectIndex);
                
                long result = this.VoteObject(record, identificationType, voteType);                
                classificationResult.AddResult(dataStore.ObjectIndex2ObjectId(objectIndex), //objectId
                                               result,//predicted class
                                               dataStore.GetDecisionValue(objectIndex), //actual class
                                               weights != null 
                                                ? (double)weights[objectIndex] 
                                                : 1.0 / dataStore.NumberOfRecords); //object weight from model
            }

            return classificationResult;
        }
                
        private long VoteObject(DataRecordInternal record, IdentificationType identificationType, VoteType voteType)
        {
            Dictionary<long, decimal> ensebleVotes = new Dictionary<long, decimal>();
            List<ReductRuleDescriptor> list = this.objectReductDescriptorMap[record.ObjectId];
            foreach (ReductRuleDescriptor reductDescriptor in list)
            {
                long result = -1;                
                Dictionary<long, decimal> decisionVotes = new Dictionary<long, decimal>();                
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

                    decimal voteWeight = Decimal.Zero;
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
                            voteWeight = Decimal.One;
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
                    
                    if (decisionVotes.ContainsKey(decision))
                        decisionVotes[decision] += voteWeight;
                    else
                        decisionVotes.Add(decision, voteWeight);                                                         
                }

                result = decisionVotes.Count > 0 ? decisionVotes.FindMaxValue() : -1;

                if (result != -1)
                {
                    if (ensebleVotes.ContainsKey(result))                    
                        ensebleVotes[result] += reductDescriptor.Weight;
                    else
                        ensebleVotes.Add(result, reductDescriptor.Weight);
                }
            }

            long ensembleResult = ensebleVotes.Count > 0 ? ensebleVotes.FindMaxValue() : -1;            
            return ensembleResult;
        }

        public bool IsObjectRecognizable(DataStore dataStore, long objectId, IReduct reduct, IdentificationType identificationType)
        {            
            List<ReductRuleDescriptor> list = this.objectReductDescriptorMap[objectId];
            Dictionary<long, decimal> votes = new Dictionary<long, decimal>();

            foreach (ReductRuleDescriptor reductDescriptor in list)
            {
                long identifiedDecision = -1;
                
                DecisionRuleDescriptor ruleDescriptor = reductDescriptor.GetDecisionRuleDescriptor(reduct);                
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
                if (maxValue > kvp.Value)
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
