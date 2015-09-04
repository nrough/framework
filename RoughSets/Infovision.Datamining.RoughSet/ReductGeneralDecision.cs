using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
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

        public override IReduct CreateReduct(int[] permutation, double epsilon, double[] weights)
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

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id)
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

    class ReductGeneralizedMajorityDecisionGenerator : ReductGenerator
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

        public override IReduct CreateReduct(int[] permutation, double epsilon, double[] weights)
        {
            throw new NotImplementedException("CreteReduct() method was not implemented.");
        }

        public ReductGeneralizedMajorityDecision CalculateReduct(int[] attributes)
        {
            if (attributes.Length < 1)
                throw new ArgumentOutOfRangeException("attributes", "Attribute array length must be greater than 1");
            
            EquivalenceClassCollection eqClasses = EquivalenceClassCollection.Create(this.DataStore, attributes, this.Epsilon);
            EquivalenceClassCollection eqClassesReduced = null;
            for (int i = 0; i < attributes.Length; i++)
            {
                eqClassesReduced = EquivalenceClassCollection.Reduce(eqClasses, attributes[i], this.Epsilon, this.DataStore);
            }

            ReductGeneralizedMajorityDecision reduct = (ReductGeneralizedMajorityDecision)this.CreateReductObject(eqClassesReduced.Attributes, this.Epsilon, this.GetNextReductId().ToString());
            reduct.Reduce(attributes, 0);

            return reduct;
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id)
        {
            ReductGeneralizedMajorityDecision r = new ReductGeneralizedMajorityDecision(this.DataStore, fieldIds, this.WeightGenerator.Weights, epsilon);
            r.Id = id;
            return r;
        }
    }

    public class GeneralizedMajorityDecisionFactory : IReductFactory
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
