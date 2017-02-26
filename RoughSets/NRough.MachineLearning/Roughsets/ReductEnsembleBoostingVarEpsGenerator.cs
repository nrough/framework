using System;
using NRough.Data;
using NRough.Core;
using NRough.MachineLearning.Permutations;

namespace NRough.MachineLearning.Roughsets
{
    public class ReductEnsembleBoostingVarEpsGenerator : ReductEnsembleBoostingGenerator
    {
        private double m0;
        private InformationMeasureWeights informationMeasure;

        private InformationMeasureWeights InformationMeasure
        {
            get
            {
                if (this.informationMeasure == null)
                    this.informationMeasure = new InformationMeasureWeights();
                return this.informationMeasure;
            }
        }

        public double M0
        {
            get { return this.m0; }
            protected set { this.m0 = value; }
        }

        public ReductEnsembleBoostingVarEpsGenerator()
            : base()
        {
        }

        public ReductEnsembleBoostingVarEpsGenerator(DataStore data)
            : base(data)
        {
        }

        public override IReduct GetNextReduct(double[] weights)
        {
            Permutation permutation = new PermutationGenerator(this.DecisionTable).Generate(1)[0];
            return this.CreateReduct(permutation.ToArray(), this.Epsilon, weights);
        }

        public override IReduct CreateReduct(int[] permutation, double epsilon, double[] weights, IReductStore reductStore = null, IReductStoreCollection reductStoreCollection = null)
        {
            double[] weightsCopy = new double[weights.Length];
            Array.Copy(weights, weightsCopy, weights.Length);

            ReductWeights reduct = new ReductWeights(this.DecisionTable, new int[] { }, this.Epsilon, weightsCopy);
            reduct.Id = this.GetNextReductId().ToString();
            this.Reach(reduct, permutation, null);
            this.Reduce(reduct, permutation, null);
            return reduct;
        }

        protected virtual void Reach(IReduct reduct, int[] permutation, IReductStore reductStore)
        {
            for (int i = 0; i < permutation.Length; i++)
            {
                reduct.AddAttribute(permutation[i]);
                if (this.IsReduct(reduct, reductStore))
                    return;
            }
        }

        protected virtual void Reduce(IReduct reduct, int[] permutation, IReductStore reductStore)
        {
            int len = permutation.Length - 1;
            for (int i = len; i >= 0; i--)
            {
                int attributeId = permutation[i];
                if (reduct.TryRemoveAttribute(attributeId))
                    if (!this.IsReduct(reduct, reductStore))
                        reduct.AddAttribute(attributeId);
            }
        }

        /// <summary>
        /// Checks if M(Bw) – M(0) >= (1-epsilon) * (M(Aw)-M(0))
        /// which is equivalent to M(Bw) >= (1 - epsilon) * M(Aw) + epsilon * M(0)
        /// </summary>
        /// <param name="reduct"></param>
        /// <returns></returns>
        public virtual bool CheckIsReduct(IReduct reduct)
        {            
            return ToleranceDoubleComparer.Instance.Compare(
                this.GetPartitionQuality(reduct),
                ((1.0 - this.Epsilon) * this.GetDataSetQuality(reduct)) + (this.Epsilon * this.m0)) != -1;

            /*
            double mB = this.GetPartitionQuality(reduct);
            double mA = this.GetDataSetQuality(reduct);
            if ((mB - this.m0) >= (1.0 - this.Epsilon) * (mA - m0))
                return true;
            return false;
            */
        }

        protected virtual bool IsReduct(IReduct reduct, IReductStore reductStore)
        {
            bool isReduct = false;
            if (reductStore != null && reductStore.IsSuperSet(reduct))
                isReduct = true;
            else
                isReduct = this.CheckIsReduct(reduct);

            return isReduct;
        }

        protected virtual double GetPartitionQuality(IReduct reduct)
        {
            return this.InformationMeasure.Calc(reduct);
        }

        protected virtual double GetDataSetQuality(IReduct reduct)
        {
            ReductWeights allAttributesReduct = new ReductWeights(this.DecisionTable, this.DecisionTable.DataStoreInfo.SelectAttributeIds(a => a.IsStandard), reduct.Epsilon, reduct.Weights);

            return this.GetPartitionQuality(allAttributesReduct);
        }

        public override void InitDefaultParameters()
        {
            base.InitDefaultParameters();

            this.Epsilon = 0.5 * this.Threshold;
        }

        public override void InitFromArgs(Args args)
        {
            base.InitFromArgs(args);

            IReduct emptyReduct = this.CreateReduct(new int[] { }, this.Epsilon, WeightGenerator.Weights);
            this.M0 = this.GetPartitionQuality(emptyReduct);

            if (!args.Exist(ReductFactoryOptions.Epsilon))
            {
                int K = this.DecisionTable.DataStoreInfo.NumberOfDecisionValues;
                this.Epsilon = (1.0 / K) * this.Threshold;
            }
        }
    }

    public class ReductEnsembleBoostingVarEpsFactory : IReductFactory
    {
        public virtual string FactoryKey
        {
            get { return ReductTypes.ReductEnsembleBoostingVarEps; }
        }

        public virtual IReductGenerator GetReductGenerator(Args args)
        {
            ReductEnsembleBoostingVarEpsGenerator rGen = new ReductEnsembleBoostingVarEpsGenerator();
            rGen.InitFromArgs(args);
            return rGen;
        }

        public virtual IPermutationGenerator GetPermutationGenerator(Args args)
        {
            throw new NotImplementedException("GetPermutationGenerator(Args args) method is not implemented");
        }
    }
}