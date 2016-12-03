using System;
using Infovision.Data;
using Infovision.Core;

namespace Infovision.Datamining.Roughset
{
    public class ReductRandomSubsetGenerator : ReductGenerator
    {
        private WeightGenerator weightGenerator;

        public int MinReductLength { get; set; }
        public int MaxReductLength { get; set; }

        protected int NumberOfAttributes { get; set; }

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

            this.MinReductLength = 0;
            this.MaxReductLength = this.DataStore.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard);

            if (args.Exist(ReductGeneratorParamHelper.MinReductLength))
                this.MinReductLength = (int)args.GetParameter(ReductGeneratorParamHelper.MinReductLength);

            if (args.Exist(ReductGeneratorParamHelper.MaxReductLength))
                this.MaxReductLength = (int)args.GetParameter(ReductGeneratorParamHelper.MaxReductLength);

            if (this.MaxReductLength > this.DataStore.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard))
                this.MaxReductLength = this.DataStore.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard);

            if (this.MaxReductLength < this.MinReductLength)
                this.MaxReductLength = this.MinReductLength;
        }

        protected override void Generate()
        {
            ReductStore localReductPool = new ReductStore();
            foreach (Permutation permutation in this.Permutations)
            {
                int cut = this.MinReductLength == this.MaxReductLength
                        ? this.MaxReductLength
                        : (int)((1.0 - this.Epsilon) * this.DataStore.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard));

                int[] attributes = new int[cut];
                for (int i = 0; i < cut; i++)
                    attributes[i] = permutation[i];

                localReductPool.DoAddReduct(this.CalculateReduct(attributes));
            }

            this.ReductPool = localReductPool;
        }

        public override IReduct CreateReduct(int[] permutation, double epsilon, double[] weights, IReductStore reductStore = null, IReductStoreCollection reductStoreCollection = null)
        {
            throw new NotImplementedException("CreteReduct() method was not implemented.");
        }

        public ReductWeights CalculateReduct(int[] attributes)
        {
            ReductWeights reduct
                = (ReductWeights)this.CreateReductObject(
                    attributes, this.Epsilon, this.GetNextReductId().ToString());
            return reduct;
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id)
        {
            ReductWeights r = new ReductWeights(this.DataStore, fieldIds, epsilon, this.WeightGenerator.Weights);
            r.Id = id;
            return r;
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id, EquivalenceClassCollection equivalenceClasses)
        {
            ReductWeights r = new ReductWeights(this.DataStore, fieldIds, epsilon, this.WeightGenerator.Weights, equivalenceClasses);
            r.Id = id;
            return r;
        }
    }

    public class ReductRandomSubsetFactory : IReductFactory
    {
        public virtual string FactoryKey
        {
            get { return ReductFactoryKeyHelper.RandomSubset; }
        }

        public virtual IReductGenerator GetReductGenerator(Args args)
        {
            ReductRandomSubsetGenerator rGen = new ReductRandomSubsetGenerator();
            rGen.InitFromArgs(args);
            return rGen;
        }

        public virtual IPermutationGenerator GetPermutationGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter(ReductGeneratorParamHelper.TrainData);
            return new PermutationGenerator(dataStore);
        }
    }
}