using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    public class ReductGeneralizedMajorityDecision : ReductWeights
    {
        private HashSet<int> removedAttributes;
        private bool isEqMapCreated;

        #region Constructors

        public ReductGeneralizedMajorityDecision(DataStore dataStore)
            : base(dataStore, 0.0M)
        {
            this.Init();
        }

        public ReductGeneralizedMajorityDecision(DataStore dataStore, decimal epsilon)
            : base(dataStore, epsilon)
        {
            this.Init();
        }

        public ReductGeneralizedMajorityDecision(DataStore dataStore, int[] fieldIds, decimal epsilon)
            : base(dataStore, fieldIds, epsilon)
        {
            this.Init();
        }

        public ReductGeneralizedMajorityDecision(DataStore dataStore, int[] fieldIds, decimal[] weights, decimal epsilon)
            : base(dataStore, fieldIds, weights, epsilon)
        {
            this.Init();
        }

        public ReductGeneralizedMajorityDecision(ReductGeneralizedMajorityDecision reduct)
            : base(reduct as ReductWeights)
        {
            //TODO Casting Error:
            /*
            Test 'Infovision.Datamining.Roughset.UnitTests.ReductGeneralDecisionGeneratorTest.GenerateTest(System.Collections.Generic.Dictionary`2[System.String,System.Object])' failed:
                System.InvalidCastException : Nie można rzutować obiektu typu 'Infovision.Datamining.Roughset.EquivalenceClassCollection' na typ 'Infovision.Datamining.Roughset.EquivalenceClassSortedMap'.
                w Infovision.Datamining.Roughset.ReductGeneralizedMajorityDecision..ctor(ReductGeneralizedMajorityDecision reduct) w f:\Projects\Infovision\Infovision.Datamining.RoughSet\ReductGeneralizedMajorityDecision.cs:wiersz 38
                w Infovision.Datamining.Roughset.ReductGeneralizedMajorityDecision.Clone() w f:\Projects\Infovision\Infovision.Datamining.RoughSet\ReductGeneralizedMajorityDecision.cs:wiersz 105
                w Infovision.Datamining.Roughset.ReductStore..ctor(ReductStore reductStore) w f:\Projects\Infovision\Infovision.Datamining.RoughSet\ReductStore.cs:wiersz 121
                w Infovision.Datamining.Roughset.ReductStore.RemoveDuplicates() w f:\Projects\Infovision\Infovision.Datamining.RoughSet\ReductStore.cs:wiersz 157
                w Infovision.Datamining.Roughset.ReductGeneralDecisionGenerator.Generate() w f:\Projects\Infovision\Infovision.Datamining.RoughSet\ReductGeneralDecisionGenerator.cs:wiersz 56
                w Infovision.Datamining.Roughset.UnitTests.ReductGeneralDecisionGeneratorTest.GenerateTest(Dictionary`2 args) w f:\Projects\Infovision\Infovision.Datamining.Roughset.UnitTests\ReductGeneralDecisionGeneratorTest.cs:wiersz 87
            */

            //this.EquivalenceClassCollection = (EquivalenceClassSortedMap) reduct.EquivalenceClassCollection.Clone();

            //TODO Temporary fix : This will cause EQ map to be recalculated on next call
            this.EquivalenceClasses = null;
            this.removedAttributes = new HashSet<int>(reduct.removedAttributes);
        }

        #endregion

        #region Methods

        private void Init()
        {
            this.removedAttributes = new HashSet<int>();
            this.isEqMapCreated = false;
        }

        protected override void InitEquivalenceMap()
        {
            if (isEqMapCreated)
                throw new InvalidOperationException("EquicalenceClassMap can only be initialized once.");
            this.EquivalenceClasses = new EquivalenceClassSortedMap(this.DataStore);
        }

        public override void BuildEquivalenceMap()
        {
            //base.BuildEquivalenceMap();
            if (isEqMapCreated == false)
            {
                this.InitEquivalenceMap();
                this.EquivalenceClasses.Calc(this.Attributes, this.DataStore, this.Weights);
                this.isEqMapCreated = true;
            }
        }

        /// <summary>
        /// Method tries to remove attributes from current reduct. Attributes are removed in order passed in attributeOrder array.
        /// </summary>
        /// <param name="attributeOrder">Attributes to be tried to be removed in given order.</param>
        public virtual void Reduce(int[] attributeOrder, int minimumLength)
        {
            if (minimumLength == this.DataStore.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard))
                return;

            foreach (EquivalenceClass eq in this.EquivalenceClasses)
                eq.RemoveObjectsWithMinorDecisions();

            bool isReduced = false;
            int len = attributeOrder.Length;
            int reduced = 0;
            for (int i = len - 1; (i >= 0) && (len - minimumLength - reduced > 0); i--)
            {
                if (this.TryRemoveAttribute(attributeOrder[i]))
                {
                    reduced++;
                    isReduced = true;
                }
            }

            if (isReduced)
            {
                this.EquivalenceClasses.Calc(this.Attributes, this.DataStore, this.Weights);

                /*
                EquivalenceClassSortedMap newEqMap = new EquivalenceClassSortedMap(this.DataStore);
                foreach (EquivalenceClass eq in this.EquivalenceClassCollection)
                {                    
                    AttributeValueVector instance = eq.Instance;
                    foreach (int removedAttribute in this.removedAttributes)
                        instance = instance.RemoveAttribute(removedAttribute);

                    EquivalenceClass existingEqClass = newEqMap.GetEquivalenceClass(instance);

                    if (existingEqClass == null)
                    {
                        existingEqClass = new EquivalenceClass(instance, this.DataStore);
                        newEqMap.Partitions.Add(instance, existingEqClass);
                    }

                    existingEqClass.Merge(eq);					
                }

                this.EquivalenceClassCollection = newEqMap;
                */

                this.removedAttributes = new HashSet<int>();
            }
        }


        /// <summary>
        /// Method tries to remove attributes from current reduct
        /// </summary>
        /// <remarks>
        /// this method always reduce attributes in increasing order, consider using Reduce(int[] attributes)
        /// </remarks>
        public virtual void Reduce()
        {
            this.Reduce(this.Attributes.ToArray(), 0);
        }

        public override bool TryRemoveAttribute(int attributeId)
        {
            //base.TryRemoveAttribute(attributeId);
            if (this.CheckRemoveAttribute(attributeId))
            {
                this.Attributes.RemoveElement(attributeId);
                this.removedAttributes.Add(attributeId);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if an attribute can be removed from super-reduct.
        /// </summary>
        /// <param name="attributeId">Attribute Id to remove</param>
        /// <returns></returns>
        protected override bool CheckRemoveAttribute(int attributeId)
        {
            //checks if attribute exists in current reduct
            bool ret = base.CheckRemoveAttribute(attributeId);

            if (ret == false)
                return false;

            //duplicate current attribute set and remove selected attribute
            FieldSet newFieldSet = (FieldSet)(this.Attributes - attributeId);

            //new temporary equivalence class map with decision set intersection  (data vector --> generalized decision)
            Dictionary<AttributeValueVector, PascalSet> generalDecisionMap = new Dictionary<AttributeValueVector, PascalSet>();
            DataFieldInfo decisionFieldInfo = this.DataStore.DataStoreInfo.GetDecisionFieldInfo();

            foreach (EquivalenceClass eq in this.EquivalenceClasses)
            {
                //instance of a record belonging to equivalence class
                AttributeValueVector instance = eq.Instance.RemoveAttribute(attributeId);
                foreach (int removedAttribute in this.removedAttributes)
                    instance = instance.RemoveAttribute(removedAttribute);

                //add EQ class to map and calculate intersection of decisions
                PascalSet existingGeneralDecisions = null;

                if (generalDecisionMap.TryGetValue(instance, out existingGeneralDecisions))
                    existingGeneralDecisions = existingGeneralDecisions.Intersection(eq.DecisionSet);
                else
                    existingGeneralDecisions = eq.DecisionSet;
                generalDecisionMap[instance] = existingGeneralDecisions;

                //empty intersection => we cannot remove the attribute
                if (existingGeneralDecisions.GetCardinality() == 0)
                    return false;
            }

            return true;
        }

        #region ICloneable Members
        /// <summary>
        /// Clones the Reduct, performing a deep copy.
        /// </summary>
        /// <returns>A new instance of a FieldSet, using a deep copy.</returns>
        public override object Clone()
        {
            return new ReductGeneralizedMajorityDecision(this);
        }
        #endregion

        #region System.Object Methods

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            ReductGeneralizedMajorityDecision reduct = obj as ReductGeneralizedMajorityDecision;
            if (reduct == null)
                return false;

            return base.Equals(reduct);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion

        #endregion
    }

    public class ReductGeneralDecisionGenerator : ReductGenerator
    {
        private WeightGenerator weightGenerator;

        public WeightGenerator WeightGenerator
        {
            get
            {
                if (this.weightGenerator == null)
                {
                    this.weightGenerator = new WeightGeneratorConstant(this.DataStore);
                }

                return this.weightGenerator;
            }

            set
            {
                this.weightGenerator = value;
            }
        }

        public override void InitFromArgs(Args args)
        {
            base.InitFromArgs(args);

            if (args.Exist(ReductGeneratorParamHelper.WeightGenerator))
                this.weightGenerator = (WeightGenerator)args.GetParameter(ReductGeneratorParamHelper.WeightGenerator);
        }

        public override void Generate()
        {
            ReductStore localReductPool = new ReductStore();            
            foreach (Permutation permutation in this.Permutations)
            {
                int cutoff = RandomSingleton.Random.Next(0, permutation.Length - 1);
                
                int[] attributes = new int[cutoff + 1];
                for(int i = 0; i <= cutoff; i++)
                    attributes[i] = permutation[i];
                
                localReductPool.DoAddReduct(this.CalculateReduct(attributes));
            }

            //TODO Repair ReductGeneralizedMajorityDecision Clone in order to get this working
            //localReductPool = localReductPool.RemoveDuplicates();

            this.ReductPool = localReductPool;            
        }

        public override IReduct CreateReduct(int[] permutation, decimal epsilon, decimal[] weights)
        {
            throw new NotImplementedException("CreteReduct() method was not implemented.");
        }

        public ReductGeneralizedMajorityDecision CalculateReduct(int[] attributes)
        {
            ReductGeneralizedMajorityDecision reduct = (ReductGeneralizedMajorityDecision)this.CreateReductObject(attributes, this.Epsilon, this.GetNextReductId().ToString());
            reduct.Reduce(attributes, 0);

            /*
            foreach (EquivalenceClass eq in reduct.EquivalenceClasses)
                eq.RemoveObjectsWithMinorDecisions();

            for (int i = attributes.Length - 1; i >= 0; i--)
                reduct.TryRemoveAttribute(attributes[i]);
            */

            return reduct;
        }

        protected override IReduct CreateReductObject(int[] fieldIds, decimal epsilon, string id)
        {
            ReductGeneralizedMajorityDecision r = new ReductGeneralizedMajorityDecision(this.DataStore, fieldIds, this.WeightGenerator.Weights, epsilon);
            r.Id = id;
            return r;
        }
    }

    public class ReductGeneralDecisionFactory : IReductFactory
    {
        public virtual string FactoryKey
        {
            get { return ReductFactoryKeyHelper.ReductGeneralizedDecision; }
        }

        public virtual IReductGenerator GetReductGenerator(Args args)
        {
            ReductGeneralDecisionGenerator rGen = new ReductGeneralDecisionGenerator();
            rGen.InitFromArgs(args);
            return rGen;
        }

        public virtual IPermutationGenerator GetPermutationGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter(ReductGeneratorParamHelper.DataStore);
            return new PermutationGenerator(dataStore);
        }
    }

    public class ReductGeneralizedMajorityDecisionGenerator : ReductGenerator
    {
        private WeightGenerator weightGenerator;

        public WeightGenerator WeightGenerator
        {
            get
            {
                if (this.weightGenerator == null)
                {
                    this.weightGenerator = new WeightGeneratorConstant(this.DataStore);
                }

                return this.weightGenerator;
            }

            set
            {
                this.weightGenerator = value;
            }
        }

        public override void InitFromArgs(Args args)
        {
            base.InitFromArgs(args);

            if (args.Exist(ReductGeneratorParamHelper.WeightGenerator))
                this.weightGenerator = (WeightGenerator)args.GetParameter(ReductGeneratorParamHelper.WeightGenerator);
        }               

        public override void Generate()
        {
            ReductStore localReductPool = new ReductStore();
            foreach (Permutation permutation in this.Permutations)
            {
                localReductPool.DoAddReduct(this.CalculateReduct(permutation.ToArray()));
            }

            this.ReductPool = localReductPool;
        }

        public override IReduct CreateReduct(int[] permutation, decimal epsilon, decimal[] weights)
        {
            throw new NotImplementedException("CreteReduct() method was not implemented.");
        }

        public ReductGeneralizedMajorityDecision CalculateReduct(int[] attributes)
        {
            if (attributes.Length < 1)
                throw new ArgumentOutOfRangeException("attributes", "Attribute array length must be greater than 1");
            EquivalenceClassCollection eqClasses = EquivalenceClassCollection.Create(this.DataStore, attributes, this.Epsilon, this.WeightGenerator.Weights);

            //Console.WriteLine("Initial decision destibution");
            //Console.WriteLine("{0}", eqClasses.ToString2());

            //Console.WriteLine("Trying to remove minority decisions");
            foreach (EquivalenceClass eq in eqClasses)
                if (eq.DecisionSet.Count > 1)
                    eq.KeepMajorDecisions(this.Epsilon);
            //Console.WriteLine("{0}", eqClasses.ToString2());
 
            for (int i = 0; i < attributes.Length; i++)
            {
                //Console.WriteLine("===== Trying to remove attribute {0} ======", attributes[i]);
                eqClasses = this.Reduce(eqClasses, attributes[i], this.Epsilon, this.DataStore);
                //Console.WriteLine("{0}", eqClasses.ToString2());
            }

            return (ReductGeneralizedMajorityDecision)this.CreateReductObject(eqClasses.Attributes, this.Epsilon, this.GetNextReductId().ToString());            
        }

        private EquivalenceClassCollection Reduce(EquivalenceClassCollection eqClasses, int attributeIdToReduce, decimal epsilon, DataStore dataStore)
        {

            int attributeIdxToRemove = -1;
            int[] newAttr = new int[eqClasses.Attributes.Length - 1];
            int j = 0;
            for (int i = 0; i < eqClasses.Attributes.Length; i++)
            {
                if (eqClasses.Attributes[i] != attributeIdToReduce)
                    newAttr[j++] = eqClasses.Attributes[j];
                else
                    attributeIdxToRemove = i;
            }

            if (attributeIdxToRemove == -1)
                return eqClasses;

            EquivalenceClassCollection eqClassCollection = new EquivalenceClassCollection(newAttr);

            foreach (EquivalenceClass eq in eqClasses)
            {
                //AttributeValueVector values = eq.Instance.RemoveAttribute(attributeIdToReduce);
                AttributeValueVector values = eq.Instance.RemoveAttributeAtPosition(attributeIdxToRemove);

                EquivalenceClass tmpClass = new EquivalenceClass(values, dataStore, true);
                EquivalenceClass eqNew = null;
                if (eqClassCollection.Partitions.TryGetValue(values, out eqNew))
                {
                    tmpClass.DecisionSet = eqNew.DecisionSet.Intersection(eq.DecisionSet);
                    foreach (long decision in tmpClass.DecisionSet)
                    {
                        tmpClass.AddDecision(decision, eqNew.GetDecisionWeigth(decision) + eq.GetDecisionWeigth(decision));
                    }
                }
                else
                {
                    foreach (long decision in eq.DecisionSet)
                    {
                        tmpClass.AddDecision(decision, eq.GetDecisionWeigth(decision));
                    }
                }
                    
                eqClassCollection.Partitions[values] = tmpClass;
            }

            foreach (EquivalenceClass eq in eqClassCollection)
            {
                eq.KeepMajorDecisions(epsilon);
                if (eq.DecisionSet.Count == 0)
                {
                    return eqClasses;
                }
            }

            return eqClassCollection;
        }

        private EquivalenceClassCollection Reduce2(EquivalenceClassCollection eqClasses, int attributeIdToReduce, decimal epsilon, DataStore dataStore)
        {
            int[] newAttr = new int[eqClasses.Attributes.Length - 1];
            int j = 0;
            for (int i = 0; i < eqClasses.Attributes.Length; i++)
            {
                if (eqClasses.Attributes[i] != attributeIdToReduce)
                    newAttr[j++] = eqClasses.Attributes[j];
            }

            EquivalenceClassCollection eqClassCollection = new EquivalenceClassCollection(newAttr);            

            foreach (EquivalenceClass eq in eqClasses)
            {
                AttributeValueVector values = eq.Instance.RemoveAttribute(attributeIdToReduce);

                EquivalenceClass tmpClass = new EquivalenceClass(values, dataStore, true);
                foreach (long decision in eq.DecisionSet)
                    tmpClass.AddDecision(decision, eq.GetDecisionWeigth(decision));

                EquivalenceClass eqNew = null;
                if (eqClassCollection.Partitions.TryGetValue(values, out eqNew))
                    foreach (long decision in eqNew.DecisionSet)
                        tmpClass.AddDecision(decision, eqNew.GetDecisionWeigth(decision));

                eqClassCollection.Partitions[values] = tmpClass;
            }

            foreach (EquivalenceClass eq in eqClassCollection)
            {
                eq.KeepMajorDecisions2(epsilon);
                if (eq.DecisionSet.Count == 0)
                    return eqClasses;
            }

            return eqClassCollection;
        }  

        protected override IReduct CreateReductObject(int[] fieldIds, decimal epsilon, string id)
        {
            ReductGeneralizedMajorityDecision r = new ReductGeneralizedMajorityDecision(this.DataStore, 
                                                                                        fieldIds, 
                                                                                        this.WeightGenerator.Weights, 
                                                                                        epsilon);
            r.Id = id;
            return r;
        }
    }

    public class ReductGeneralizedMajorityDecisionFactory : IReductFactory
    {
        public virtual string FactoryKey
        {
            get { return ReductFactoryKeyHelper.GeneralizedMajorityDecision; }
        }

        public virtual IReductGenerator GetReductGenerator(Args args)
        {
            ReductGeneralizedMajorityDecisionGenerator rGen = new ReductGeneralizedMajorityDecisionGenerator();
            rGen.InitFromArgs(args);
            return rGen;
        }

        public virtual IPermutationGenerator GetPermutationGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter(ReductGeneratorParamHelper.DataStore);
            return new PermutationGenerator(dataStore);
        }
    }
}
