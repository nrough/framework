using System;
using System.Collections.Generic;
using System.Drawing;//TODO Remove this lib after tests
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
    public class ReductEnsembleStreamGenerator : ReductGenerator
    {
        #region Members
        
        private double[] permEpsilon;
        private IPermutationGenerator permutationGenerator;
        private double dataSetQuality = 1.0;
        private WeightGenerator weightGenerator;        
        private HierarchicalClusteringIncrementalExt hCluster;

        #endregion

        #region Properties        

        public int ReductSize { get; set; }
        public int MinimumNumberOfInstances { get; set; }
        public Func<double[], double[], double> Distance { get; set; }
        public Func<int[], int[], DistanceMatrix, double[][], double> Linkage { get; set; }
        public Func<IReduct, double[], double[]> ReconWeights {get; set; }

        public HierarchicalClusteringIncrementalExt Dendrogram
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



        #endregion

        public ReductEnsembleStreamGenerator()
            : base()
        {
        }

        public override void SetDefaultParameters()
        {
            base.SetDefaultParameters();

            this.ReconWeights = ReductEnsembleReconWeightsHelper.GetDefaultReconWeights;
            this.Linkage = ClusteringLinkage.Complete;
            this.Distance = Similarity.Euclidean;
            this.MinimumNumberOfInstances = 1;
        }

        public override void InitFromArgs(Args args)
        {
            base.InitFromArgs(args);

            if (args.Exist("PermutationEpsilon"))
            {
                double[] epsilons = (double[])args.GetParameter("PermutationEpsilon");
                this.permEpsilon = new double[epsilons.Length];
                Array.Copy(epsilons, this.permEpsilon, epsilons.Length);
            }

            if (args.Exist("Distance"))
                this.Distance = (Func<double[], double[], double>)args.GetParameter("Distance");

            if (args.Exist("Linkage"))
                this.Linkage = (Func<int[], int[], DistanceMatrix, double[][], double>)args.GetParameter("Linkage");

            if (args.Exist("WeightGenerator"))
                this.WeightGenerator = (WeightGenerator)args.GetParameter("WeightGenerator");

            if (args.Exist("ReconWeights"))
                this.ReconWeights = (Func<IReduct, double[], double[]>)args.GetParameter("ReconWeights");

            if (args.Exist("ReductSize"))
                this.ReductSize = (int)args.GetParameter("ReductSize");

            if (args.Exist("MinimumNumberOfInstances"))
                this.MinimumNumberOfInstances = (int)args.GetParameter("MinimumNumberOfInstances");

        }

        private IReduct GetNextReduct()
        {
            //TODO do not use permutations list - generate new permutations and epsilons here
            
            int idx = this.GetNextReductId();
            this.Epsilon = this.permEpsilon[idx - 1];
            Permutation permutation = this.Permutations[idx - 1];
            int cutoff = RandomSingleton.Random.Next(0, permutation.Length - 1);

            //IReduct reduct = this.CreateReductObject(new int[] { }, this.permEpsilon[idx - 1], idx.ToString());
            this.WeightGenerator.Generate();
            
            int[] attributes = new int[cutoff + 1];
            for (int i = 0; i <= cutoff; i++)
                attributes[i] = permutation[i];
            
            ReductCrisp reduct = new ReductCrisp(this.DataStore, attributes, this.WeightGenerator.Weights, this.Epsilon);
            reduct.Id = idx.ToString();
            foreach (EquivalenceClass eq in reduct.EquivalenceClassMap)
                eq.RemoveObjectsWithMinorDecisions();
            for (int i = attributes.Length - 1; i >= 0; i--)
                reduct.TryRemoveAttribute(attributes[i]);

            return reduct;
        }        

        private bool HasConverged()
        {
            //TODO Add Stop criteria
            if(this.NextReductId >= this.permEpsilon.Length)            
            //if (this.ReductPool.Count() >= 20)
                return true;
            return false;
        }

        public override void Generate()
        {            
            this.ReductPool = new ReductStore();

            this.hCluster = new HierarchicalClusteringIncrementalExt(this.Distance, this.Linkage);
            this.hCluster.MinimumNumberOfInstances = this.MinimumNumberOfInstances;

            while (!this.HasConverged())
            {
                IReduct reduct = this.GetNextReduct();
                if (reduct.Attributes.GetCardinality() > 0)
                {
                    double[] errorvector = this.ReconWeights(reduct, reduct.Weights);
                                        
                    if (this.hCluster.AddToCluster(Convert.ToInt32(reduct.Id), errorvector))
                    {
                        this.ReductPool.AddReduct(reduct);
                                                
                        /*
                        //TODO Remove this
                        DendrogramChart chart = new DendrogramChart(this.hCluster, 1920, 1200);
                        //chart.Colors = new List<Color>(new Color[] { Color.Blue, Color.Red, Color.Orange, Color.Brown, Color.Beige});
                        Bitmap chartBitmap = chart.GetAsBitmap();
                        if (reduct.Id.Length == 1)
                            chartBitmap.Save(String.Format(@"F:\Temp\Dendrogram_Incremental_{0}_00{1}.bmp", this.DataStore.Name, reduct.Id));
                        else if (reduct.Id.Length == 2)
                            chartBitmap.Save(String.Format(@"F:\Temp\Dendrogram_Incremental_{0}_0{1}.bmp", this.DataStore.Name, reduct.Id));
                        else
                            chartBitmap.Save(String.Format(@"F:\Temp\Dendrogram_Incremental_{0}_{1}.bmp", this.DataStore.Name, reduct.Id));
                        */
                    }
                }
            }            
        }

        public override IReduct CreateReduct(Permutation permutation)
        {
            throw new NotImplementedException("CreteReduct() method was not implemented.");
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
            {
                IReduct reduct = store.GetReduct(i);
                errors[i] = this.ReconWeights(reduct, reduct.Weights);
            }
            return errors;
        }

        protected virtual bool IsReduct(IReduct reduct, IReductStore reductStore, bool useCache)
        {                       
            if (reductStore.IsSuperSet(reduct))
                return true;

            double partitionQuality = this.GetPartitionQuality(reduct);
            double tinyDouble = 0.0001 / this.DataStore.NumberOfRecords;
            if (partitionQuality >= (((1.0 - reduct.Epsilon) * this.DataSetQuality) - tinyDouble))
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

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id)
        {
            ReductWeights r = new ReductWeights(this.DataStore, fieldIds, this.WeightGenerator.Weights, epsilon);
            r.Id = id;
            return r;
        }
    }

    public class ReductEnsambleStreamFactory : IReductFactory
    {
        public virtual string FactoryKey
        {
            get { return "ReductEnsembleStream"; }
        }

        public virtual IReductGenerator GetReductGenerator(Args args)
        {
            ReductEnsembleStreamGenerator rGen = new ReductEnsembleStreamGenerator();
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
