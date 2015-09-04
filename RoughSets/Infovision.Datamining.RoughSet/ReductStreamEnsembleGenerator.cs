using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;
using Infovision.Datamining;
using Infovision.Datamining.Clustering.Hierarchical;
using Infovision.Math;
using Infovision.Datamining.Experimenter.Parms;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class ReductStreamEnsembleGenerator : ReductGenerator
    {
        private int[] permEpsilon;
        private IPermutationGenerator permutationGenerator;
        private double dataSetQuality = 1.0;
        private WeightGenerator weightGenerator;
        private Func<IReduct, double[], double[]> recognition;
        private Func<int[], int[], DistanceMatrix, double[][], double> linkage;
        private Func<double[], double[], double> distance;
        private HierarchicalClustering hCluster;
        private ReductStore localReductPool;

        public HierarchicalClustering Dendrogram
        {
            get { return this.hCluster; }
        }

        protected override IPermutationGenerator PermutationGenerator
        {
            get
            {
                if (permutationGenerator == null)
                {
                    lock (syncRoot)
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
                    this.weightGenerator = new WeightGeneratorConstant(this.DataStore);
                }

                return this.weightGenerator;
            }

            set
            {
                this.weightGenerator = value;
            }
        }

        public ReductStreamEnsembleGenerator()
            : base()
        {
        }

        public override void SetDefaultParameters()
        {
            base.SetDefaultParameters();

            this.recognition = ReductEnsembleReconWeightsHelper.GetDefaultReconWeights;
            this.linkage = ClusteringLinkage.Complete;
            this.distance = Similarity.Euclidean;
        }

        public override void InitFromArgs(Args args)
        {
            base.InitFromArgs(args);

            if (args.Exist("PermutationEpsilon"))
            {
                int[] epsilons = (int[])args.GetParameter("PermutationEpsilon");
                this.permEpsilon = new int[epsilons.Length];
                Array.Copy(epsilons, this.permEpsilon, epsilons.Length);
            }

            if (args.Exist("Distance"))
                this.distance = (Func<double[], double[], double>)args.GetParameter("Distance");

            if (args.Exist("Linkage"))
                this.linkage = (Func<int[], int[], DistanceMatrix, double[][], double>)args.GetParameter("Linkage");

            if (args.Exist("WeightGenerator"))
                this.WeightGenerator = (WeightGenerator)args.GetParameter("WeightGenerator");

            if (args.Exist("ReconWeights"))
                this.recognition = (Func<IReduct, double[], double[]>)args.GetParameter("ReconWeights");

        }

        private IReduct GetNextReduct()
        {
            int idx = this.GetNextReductId();
            IReduct reduct = this.CreateReductObject(new int[] { }, this.permEpsilon[idx], idx.ToString());
            Permutation permutation = this.Permutations[idx];

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

            return reduct;
        }

        private void ProcessReduct(IReduct reduct)
        {
            this.ReductPool.AddReduct(reduct);
        }

        private bool HasConverged()
        {
            if (this.ReductPool.Count() > 20)
                return true;
            return false;
        }

        public override void Generate()
        {
            localReductPool = new ReductStore();
            this.ReductPool = localReductPool;

            while (!this.HasConverged())
            {
                IReduct reduct = this.GetNextReduct();
                this.ProcessReduct(reduct);
            }

            localReductPool = localReductPool.RemoveDuplicates();
            this.ReductPool = localReductPool;

            double[][] errorVectors = this.GetWeightVectorsFromReducts(localReductPool);

            Dictionary<int, double[]> errors = new Dictionary<int, double[]>();
            for (int i = 0; i < errorVectors.Length; i++)
            {
                errors.Add(i, errorVectors[i]);
            }

            this.hCluster = new HierarchicalClustering(distance, linkage);
            this.hCluster.Instances = errors;
            this.hCluster.Compute();
        }


        public override IReductStoreCollection GetReductGroups(int numberOfEnsembles)
        {
            Dictionary<int, List<int>> clusterMembership = this.hCluster.GetClusterMembershipAsDict(numberOfEnsembles);
            ReductStoreCollection result = new ReductStoreCollection();

            foreach (KeyValuePair<int, List<int>> kvp in clusterMembership)
            {
                ReductStore tmpReductStore = new ReductStore();
                foreach (int r in kvp.Value)
                {
                    tmpReductStore.DoAddReduct(this.ReductPool.GetReduct(r));
                }

                result.AddStore(tmpReductStore);
            }

            return result;
        }

        /// <summary>
        /// Returns a weight vector array, where for each reduct an recognition weight is stored        
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        public double[][] GetWeightVectorsFromReducts(IReductStore store)
        {
            double[][] errors = new double[store.Count][];
            for (int i = 0; i < store.Count; i++)
                errors[i] = recognition(store.GetReduct(i), this.WeightGenerator.Weights);
            return errors;
        }



        protected virtual bool IsReduct(IReduct reduct, IReductStore reductStore, bool useCache)
        {
            if (reductStore.IsSuperSet(reduct))
                return true;

            double partitionQuality = this.GetPartitionQuality(reduct);
            double tinyDouble = 0.0001 / this.DataStore.NumberOfRecords;
            if (partitionQuality >= (((1.0 - (reduct.Epsilon / 100.0)) * this.DataSetQuality) - tinyDouble))
                return true;

            return false;
        }

        protected virtual double GetPartitionQuality(IReduct reduct)
        {
            //TODO Consider changing to information measue
            //return this.InformationMeasure.Calc(reduct);

            double tinyDouble = (0.0001 / (double)this.DataStore.NumberOfRecords);
            double result = 0;
            foreach (EquivalenceClass e in reduct.EquivalenceClassMap)
            {
                double maxValue = Double.MinValue;
                long maxDecision = -1;
                foreach (long decisionValue in e.DecisionValues)
                {
                    double sum = 0;
                    foreach (int objectIdx in e.GetObjectIndexes(decisionValue))
                    {
                        sum += reduct.Weights[objectIdx];
                    }
                    if (sum > (maxValue + tinyDouble))
                    {
                        maxValue = sum;
                        maxDecision = decisionValue;
                    }
                }

                result += maxValue;
            }

            return result;
        }

        protected virtual void CalcDataSetQuality()
        {
            IReduct reduct = this.CreateReductObject(this.DataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), 0, "");
            this.DataSetQuality = this.GetPartitionQuality(reduct);
        }

        protected override IReductStore CreateReductStore()
        {
            return new ReductStore();
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id)
        {
            ReductWeights r = new ReductWeights(this.DataStore, fieldIds, this.WeightGenerator.Weights, epsilon);
            r.Id = id;
            return r;
        }
    }

    public class ReductStreamEnsambleFactory : IReductFactory
    {
        public virtual string FactoryKey
        {
            get { return "ReductStreamEnsemble"; }
        }

        public virtual IReductGenerator GetReductGenerator(Args args)
        {
            ReductStreamEnsembleGenerator rGen = new ReductStreamEnsembleGenerator();
            rGen.InitFromArgs(args);
            return rGen;
        }

        public virtual IPermutationGenerator GetPermutationGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter("DataStore");
            return new PermutationGenerator(dataStore);
        }
    }
}
