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
    public class ReductEnsembleGenerator : ReductGenerator
    {                        
        private decimal[] permEpsilon;
        private IPermutationGenerator permutationGenerator;
        private decimal dataSetQuality = Decimal.One;        
        private WeightGenerator weightGenerator;
        private Func<IReduct, decimal[], double[]> recognition;
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
            get; set;
        }

        protected decimal DataSetQuality
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
        
        public ReductEnsembleGenerator()
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

            if (args.Exist(ReductGeneratorParamHelper.PermutationEpsilon))
            {
                decimal[] epsilons = (decimal[])args.GetParameter(ReductGeneratorParamHelper.PermutationEpsilon);
                this.permEpsilon = new decimal[epsilons.Length];
                Array.Copy(epsilons, this.permEpsilon, epsilons.Length);
            }
            
            if (args.Exist(ReductGeneratorParamHelper.Distance))
                this.distance = (Func<double[], double[], double>)args.GetParameter(ReductGeneratorParamHelper.Distance);

            if (args.Exist(ReductGeneratorParamHelper.Linkage))
                this.linkage = (Func<int[], int[], DistanceMatrix, double[][], double>)args.GetParameter(ReductGeneratorParamHelper.Linkage);            

            if (args.Exist(ReductGeneratorParamHelper.WeightGenerator))
                this.WeightGenerator = (WeightGenerator)args.GetParameter(ReductGeneratorParamHelper.WeightGenerator);

            if (args.Exist(ReductGeneratorParamHelper.ReconWeights))
                this.recognition = (Func<IReduct, decimal[], double[]>)args.GetParameter(ReductGeneratorParamHelper.ReconWeights);
        }        
       
        public override void Generate()
        {
            ReductStore localReductPool = new ReductStore();
            
            int k = -1;            
            foreach (Permutation permutation in this.Permutations)
            {                
                decimal localApproxLevel = this.permEpsilon[++k];

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

            Dictionary<int, double[]> errors = new Dictionary<int, double[]>();
            for (int i = 0; i < errorVectors.Length; i++)
            {
                errors.Add(i, errorVectors[i]);
            }
                            
            this.hCluster = new HierarchicalClustering(distance, linkage);
            this.hCluster.Instances = errors;
            this.hCluster.Compute();
        }

        public override IReduct CreateReduct(int[] permutation, decimal epsilon, decimal[] weights)
        {
            throw new NotImplementedException("CreteReduct() method was not implemented.");
        }

        public override IReductStoreCollection GetReductStoreCollection(int numberOfEnsembles)
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
            {
                IReduct reduct = store.GetReduct(i);
                errors[i] = recognition(reduct, reduct.Weights);
            }
            return errors;
        }

        protected virtual bool IsReduct(IReduct reduct, IReductStore reductStore, bool useCache)
        {            
            if (reductStore.IsSuperSet(reduct))            
                return true;            

            decimal partitionQuality = this.GetPartitionQuality(reduct);
            if (Decimal.Round(partitionQuality, 17) >= Decimal.Round((Decimal.One - reduct.Epsilon) * this.DataSetQuality, 17))
                return true;

            return false;
        }

        protected virtual decimal GetPartitionQuality(IReduct reduct)
        {
            return new InformationMeasureWeights().Calc(reduct);
        }

        protected virtual void CalcDataSetQuality()
        {
            IReduct reduct = this.CreateReductObject(this.DataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray(), 0, "");
            this.DataSetQuality = this.GetPartitionQuality(reduct);
        }

        protected override IReduct CreateReductObject(int[] fieldIds, decimal epsilon, string id)
        {
            ReductWeights r = new ReductWeights(this.DataStore, fieldIds, this.WeightGenerator.Weights, epsilon);
            r.Id = id;
            return r;
        }
    }

    public class ReductEnsambleFactory : IReductFactory
    {
        public virtual string FactoryKey
        {
            get { return ReductFactoryKeyHelper.ReductEnsemble; }
        }

        public virtual IReductGenerator GetReductGenerator(Args args)
        {            
            ReductEnsembleGenerator rGen = new ReductEnsembleGenerator();
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
