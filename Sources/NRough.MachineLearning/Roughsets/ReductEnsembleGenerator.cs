using System;
using System.Collections.Generic;
using System.Linq;
using NRough.Data;
using NRough.MachineLearning.Clustering.Hierarchical;
using NRough.Math;
using NRough.Core;
using NRough.MachineLearning.Permutations;
using NRough.MachineLearning.Weighting;

namespace NRough.MachineLearning.Roughsets
{
    [Serializable]
    public class ReductEnsembleGenerator : ReductGenerator
    {
        private double[] permEpsilon;
        private IPermutationGenerator permutationGenerator;
        private double dataSetQuality = 1.0;
        private WeightGenerator weightGenerator;
        private Func<IReduct, double[], RuleQualityMethod, double[]> recognition;
        private Func<int[], int[], DistanceMatrix, double[][], double> linkage;
        private Func<double[], double[], double> distance;
        private HierarchicalClusteringBase hCluster;

        public HierarchicalClusteringBase Dendrogram
        {
            get { return this.hCluster; }
        }

        protected override IPermutationGenerator PermutationGenerator
        {
            get
            {
                if (permutationGenerator == null)
                {
                    lock (mutex)
                    {
                        if (permutationGenerator == null)
                        {
                            permutationGenerator = new PermutatioGeneratorFieldGroup(this.FieldGroups);
                        }
                    }
                }

                return this.permutationGenerator;
            }
        }

        protected bool IsQualityCalculated
        {
            get;
            set;
        }

        protected double DataSetQuality
        {
            get
            {
                if (!this.IsQualityCalculated)
                {
                    this.CalcDataSetQuality();
                    this.IsQualityCalculated = true;
                }

                return this.dataSetQuality;
            }

            set
            {
                this.dataSetQuality = value;
                this.IsQualityCalculated = true;
            }
        }

        public WeightGenerator WeightGenerator
        {
            get
            {
                if (this.weightGenerator == null)
                {
                    this.weightGenerator = new WeightGeneratorConstant(this.DecisionTable);
                }

                return this.weightGenerator;
            }

            set
            {
                this.weightGenerator = value;
            }
        }

        public ReductEnsembleGenerator()
            : base()
        {
        }

        public override void InitDefaultParameters()
        {
            base.InitDefaultParameters();

            this.recognition = ReductToVectorConversionMethods.GetDefaultReconWeights;
            this.linkage = ClusteringLinkage.Complete;
            this.distance = Distance.Euclidean;
        }

        public override void InitFromArgs(Args args)
        {
            base.InitFromArgs(args);

            if (args.Exist(ReductFactoryOptions.PermutationEpsilon))
            {
                double[] epsilons = (double[])args.GetParameter(ReductFactoryOptions.PermutationEpsilon);
                this.permEpsilon = new double[epsilons.Length];
                Array.Copy(epsilons, this.permEpsilon, epsilons.Length);
            }

            if (args.Exist(ReductFactoryOptions.Distance))
                this.distance = (Func<double[], double[], double>)args.GetParameter(ReductFactoryOptions.Distance);

            if (args.Exist(ReductFactoryOptions.Linkage))
                this.linkage = (Func<int[], int[], DistanceMatrix, double[][], double>)args.GetParameter(ReductFactoryOptions.Linkage);

            if (args.Exist(ReductFactoryOptions.WeightGenerator))
                this.WeightGenerator = (WeightGenerator)args.GetParameter(ReductFactoryOptions.WeightGenerator);

            if (args.Exist(ReductFactoryOptions.ReconWeights))
                this.recognition = (Func<IReduct, double[], RuleQualityMethod, double[]>)args.GetParameter(ReductFactoryOptions.ReconWeights);
        }

        protected override void Generate()
        {
            ReductStore localReductPool = new ReductStore(this.Permutations.Count);

            int k = -1;
            foreach (Permutation permutation in this.Permutations)
            {
                double localApproxLevel = this.permEpsilon[++k];

                IReduct reduct = this.CreateReductObject(new int[] { }, localApproxLevel, this.GetNextReductId().ToString());

                //Reach
                for (int i = 0; i < permutation.Length; i++)
                {
                    reduct.AddAttribute(permutation[i]);

                    if (this.IsReduct(reduct, localReductPool, false))
                    {
                        break;
                    }
                }

                //Reduce
                int len = permutation.Length - 1;
                for (int i = len; i >= 0; i--)
                {
                    int attributeId = permutation[i];
                    if (reduct.TryRemoveAttribute(attributeId))
                    {
                        if (!this.IsReduct(reduct, localReductPool, false))
                        {
                            reduct.AddAttribute(attributeId);
                        }
                    }
                }

                localReductPool.DoAddReduct(reduct);
            }

            localReductPool = localReductPool.RemoveDuplicates();
            this.ReductPool = localReductPool;

            double[][] errorVectors = this.GetWeightVectorsFromReducts(localReductPool);

            Dictionary<int, double[]> errors = new Dictionary<int, double[]>(errorVectors.Length);
            for (int i = 0; i < errorVectors.Length; i++)
            {
                errors.Add(i, errorVectors[i]);
            }

            this.hCluster = new HierarchicalClustering(distance, linkage);
            this.hCluster.Instances = errors;
            this.hCluster.Compute();
        }

        public override IReduct CreateReduct(int[] permutation, double epsilon, double[] weights, IReductStore reductStore = null, IReductStoreCollection reductStoreCollection = null)
        {
            throw new NotImplementedException("CreteReduct() method was not implemented.");
        }

        public override IReductStoreCollection GetReductStoreCollection(int numberOfEnsembles)
        {
            Dictionary<int, List<int>> clusterMembership = this.hCluster.GetClusterMembershipAsDict(numberOfEnsembles);
            ReductStoreCollection result = new ReductStoreCollection(clusterMembership.Count);

            foreach (KeyValuePair<int, List<int>> kvp in clusterMembership)
            {
                ReductStore tmpReductStore = new ReductStore(kvp.Value.Count);
                foreach (int r in kvp.Value)
                {
                    tmpReductStore.DoAddReduct(this.ReductPool.GetReduct(r));
                }

                result.AddStore(tmpReductStore);
            }

            return result;
        }

        /// <summary>
        /// Returns a objectWeight vector array, where for each reduct an recognition objectWeight is stored
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        public double[][] GetWeightVectorsFromReducts(IReductStore store)
        {
            double[][] errors = new double[store.Count][];
            for (int i = 0; i < store.Count; i++)
            {
                IReduct reduct = store.GetReduct(i);
                errors[i] = recognition(reduct, reduct.Weights, RuleQualityMethods.ConfidenceW);
            }
            return errors;
        }

        protected virtual bool IsReduct(IReduct reduct, IReductStore reductStore, bool useCache)
        {
            if (reductStore.IsSuperSet(reduct))
                return true;

            double partitionQuality = this.GetPartitionQuality(reduct);
            if (partitionQuality >= (1.0 - reduct.Epsilon) * this.DataSetQuality)
                return true;

            return false;
        }

        protected virtual double GetPartitionQuality(IReduct reduct)
        {
            return new InformationMeasureWeights().Calc(reduct);
        }

        protected virtual void CalcDataSetQuality()
        {
            IReduct reduct = this.CreateReductObject(this.DecisionTable.GetStandardFields(), 0, "");
            this.DataSetQuality = this.GetPartitionQuality(reduct);
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id)
        {
            ReductWeights r = new ReductWeights(this.DecisionTable, fieldIds, epsilon, this.WeightGenerator.Weights);
            r.Id = id;
            return r;
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id, EquivalenceClassCollection equivalenceClasses)
        {
            return this.CreateReductObject(fieldIds, epsilon, id);
        }
    }

    public class ReductEnsembleFactory : IReductFactory
    {
        public virtual string FactoryKey
        {
            get { return ReductTypes.ReductEnsemble; }
        }

        public virtual IReductGenerator GetReductGenerator(Args args)
        {
            ReductEnsembleGenerator rGen = new ReductEnsembleGenerator();
            rGen.InitFromArgs(args);
            return rGen;
        }

        public virtual IPermutationGenerator GetPermutationGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter(ReductFactoryOptions.DecisionTable);
            return new PermutationGenerator(dataStore);
        }
    }
}