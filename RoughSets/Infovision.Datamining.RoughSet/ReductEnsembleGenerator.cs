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

namespace Infovision.Datamining.Roughset
{       
    [Serializable]
    public class ReductEnsembleGenerator : ReductGenerator
    {                        
        private int[] permEpsilon;
        private IPermutationGenerator permutationGenerator;
        private double dataSetQuality = 1.0;        
        private WeightGenerator weightGenerator;
        private ReductStore internalStore;
        private HierarchicalClustering hCluster;

        public HierarchicalClustering Dendrogram
        {
            get { return this.hCluster; }
        }

        protected IPermutationGenerator PermutationGenerator
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
            get; set;
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

        protected WeightGenerator WeightGenerator
        {
            get
            {
                if (this.weightGenerator == null)
                {
                    this.weightGenerator = new WeightGeneratorMajority(this.DataStore);
                }

                return this.weightGenerator;
            }
        }
        
        public ReductEnsembleGenerator(DataStore data, int[] epsilon)
            : base(data)
        {
            permEpsilon = new int[epsilon.Length];
            Array.Copy(epsilon, permEpsilon, epsilon.Length);            
        }
        
        public override IReductStoreCollection Generate(Args args)
        {
            internalStore = new ReductStore();
            PermutationCollection permutations = this.FindOrCreatePermutationCollection(args);                                    
            Func<double[], double[], double> distance = SimilarityIndex.ReductSimDelegate(0.5);
            Func<int[], int[], DistanceMatrix, double> linkage = ClusteringLinkage.Min;

            if (args.Exist("Distance"))
                distance = (Func<double[], double[], double>) args.GetParameter("Distance");

            if (args.Exist("Linkage"))
                linkage = (Func<int[], int[], DistanceMatrix, double>) args.GetParameter("Linkage");
            
            int k = -1;            
            foreach (Permutation permutation in permutations)
            {                
                double localApproxLevel = this.permEpsilon[++k] / 100.0;

                IReduct reduct = this.CreateReductObject(new int[] { }, localApproxLevel);
                
                //Reach
                for (int i = 0; i < permutation.Length; i++)
                {
                    reduct.AddAttribute(permutation[i]);

                    if (this.IsReduct(reduct, internalStore, false))
                    {
                        break;
                    }
                }

                //Reduce
                int len = permutation.Length - 1;
                for (int i = len; i >= 0; i--)
                {
                    int attributeId = permutation[i];
                    if (reduct.RemoveAttribute(attributeId))
                    {
                        if (!this.IsReduct(reduct, internalStore, false))
                        {
                            reduct.AddAttribute(attributeId);
                        }
                    }
                }
             
                internalStore.DoAddReduct(reduct);                
            }

            internalStore = internalStore.RemoveDuplicates();

            double[][] errorVectors = this.GetErrorVectorsFromReducts(internalStore, false);
            hCluster = new HierarchicalClustering(distance, linkage);
            hCluster.Compute(errorVectors);
            Console.WriteLine(hCluster.ToString());
            
            ReductStoreCollection reductStoreCollection = new ReductStoreCollection();            
            reductStoreCollection.AddStore(internalStore);

            return reductStoreCollection;
        }        

        private double[][] GetErrorVectorsFromReducts(ReductStore store, bool useErrorValues)
        {
            double[][] errors = new double[store.Count][];
            for (int i = 0; i < store.Count; i++)
            {
                errors[i] = new double[this.DataStore.NumberOfRecords];
                IReduct reduct = store.GetReduct(i);
                
                if (useErrorValues)
                {
                    Array.Copy(this.WeightGenerator.Weights, errors[i], this.DataStore.NumberOfRecords);
                }

                foreach (EquivalenceClass e in reduct.EquivalenceClassMap)
                {
                    double maxValue = 0;
                    long maxDecision = -1;
                    foreach (long decisionValue in e.DecisionValues)
                    {
                        double sum = 0;
                        foreach (int objectIdx in e.GetObjectIndexes(decisionValue))
                        {
                            sum += reduct.Weights[objectIdx];
                        }
                        
                        if (sum > maxValue + (0.0001 / reduct.ObjectSetInfo.NumberOfRecords))
                        {
                            maxValue = sum;
                            maxDecision = decisionValue;
                        }
                    }
                    
                    foreach (int objectIdx in e.GetObjectIndexes(maxDecision))
                    {
                        if (useErrorValues)
                        {
                            errors[i][objectIdx] = 0;
                        }
                        else
                        {
                            errors[i][objectIdx] = reduct.Weights[objectIdx]; 
                        }
                    }                    
                }
            }

            return errors;
        }

        protected virtual bool IsReduct(IReduct reduct, IReductStore reductStore, bool useCache)
        {            
            if (reductStore.IsSuperSet(reduct, true))
            {                
                return true;
            }
                        
            double partitionQuality = this.GetPartitionQuality(reduct);
            double tinyDouble = 0.0001 / this.DataStore.NumberOfRecords;
            if (partitionQuality >= (((1.0 - reduct.ApproximationDegree) * this.DataSetQuality) - tinyDouble))
            {                
                return true;
            }
            
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
                double maxValue = 0;
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
            IReduct reduct = this.CreateReductObject(this.DataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), 0);
            this.DataSetQuality = this.GetPartitionQuality(reduct);
        }

        protected override IReductStore CreateReductStore(Args args)
        {
            return new ReductStore();
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double approxDegree)
        {
            //return new Reduct(this.DataStore, fieldIds);
            return new ReductWeights(this.DataStore, fieldIds, this.WeightGenerator.Weights, approxDegree);            
        }

        protected virtual PermutationCollection FindOrCreatePermutationCollection(Args args)
        {
            PermutationCollection permutationList = null;
            if (args.Exist("PermutationCollection"))
            {
                permutationList = (PermutationCollection)args.GetParameter("PermutationCollection");
            }
            else if (args.Exist("NumberOfReducts"))
            {
                int numberOfReducts = (int)args.GetParameter("NumberOfReducts");
                permutationList = this.PermutationGenerator.Generate(numberOfReducts);
            }
            else if (args.Exist("NumberOfPermutations"))
            {
                int numberOfPermutations = (int)args.GetParameter("NumberOfPermutations");
                permutationList = this.PermutationGenerator.Generate(numberOfPermutations);
            }

            if (permutationList == null)
            {
                throw new NullReferenceException("PermutationCollection is null");
            }

            return permutationList;
        }
    }

    public class ReductEnsambleFactory : IReductFactory
    {
        public virtual string FactoryKey
        {
            get { return "ReductEnsemble"; }
        }

        public virtual IReductGenerator GetReductGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter("DataStore");
            int[] epsilons = (int[])args.GetParameter("PermutationEpsilon");
            ReductEnsembleGenerator reductGenerator = new ReductEnsembleGenerator(dataStore, epsilons);
            return reductGenerator;
        }

        public virtual IPermutationGenerator GetPermutationGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter("DataStore");
            return new PermutationGenerator(dataStore);
        }
    }
}
