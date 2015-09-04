using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{       
    public class ReductEnsembleGenerator : ReductGenerator
    {                        
        private int[] permEpsilon;
        private IPermutationGenerator permutationGenerator;
        private double dataSetQuality = 1.0;        
        private WeightGenerator weightGenerator;
        private ReductStore internalStore;

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

        //public override IReductStore Generate(Args args)
        public override IReductStoreCollection Generate(Args args)
        {
            internalStore = new ReductStore();
            PermutationCollection permutations = this.FindOrCreatePermutationCollection(args);            
            return this.CreateReductStoreFromPermutationCollection(permutations, args);           
        }

        protected virtual IReductStoreCollection CreateReductStoreFromPermutationCollection(PermutationCollection permutations, Args args)
        {                        
            int k = -1;            
            foreach (Permutation permutation in permutations)
            {                
                double localApproxLevel = (double) this.permEpsilon[++k] / (double) 100;

                IReduct reduct = this.CreateReductObject(new int[] { });
                
                //Reach
                for (int i = 0; i < permutation.Length; i++)
                {
                    reduct.AddAttribute(permutation[i]);

                    if (this.IsReduct(reduct, internalStore, false, localApproxLevel, false))
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
                        if (!this.IsReduct(reduct, internalStore, false, localApproxLevel, false))
                        {
                            reduct.AddAttribute(attributeId);
                        }
                    }
                }
             
                internalStore.DoAddReduct(reduct);                
            }

            //TODO Split onto individual reduct stores
            ReductStoreCollection reductStoreCollection = new ReductStoreCollection();
            //ReductStore reductStore = new ReductStore();
            reductStoreCollection.AddStore(internalStore);

            return reductStoreCollection;
        }

        protected virtual bool IsReduct(IReduct reduct, IReductStore reductStore, bool useCache, double approximationLevel, bool checkExistingReducts)
        {

            if (checkExistingReducts && reductStore.IsSuperSet(reduct))
            {                
                return true;
            }
                        
            double partitionQuality = this.GetPartitionQuality(reduct);
            double tinyDouble = (0.0001 / (double)this.DataStore.NumberOfRecords);
            if (partitionQuality >= ((((double)1 - approximationLevel) * this.DataSetQuality) - tinyDouble))
            {                
                return true;
            }
            
            return false;
        }

        protected virtual double GetPartitionQuality(IReduct reduct)
        {
            //TODO 
            //return this.InformationMeasure.Calc(reduct);

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
                    if (sum > (maxValue + (0.0001 / (double)reduct.ObjectSetInfo.NumberOfRecords)))
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
            IReduct reduct = this.CreateReductObject(this.DataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard));
            this.DataSetQuality = this.GetPartitionQuality(reduct);
        }

        protected override IReductStore CreateReductStore(Args args)
        {
            return new ReductStore();
        }

        protected override IReduct CreateReductObject(int[] fieldIds)
        {
            //return new Reduct(this.DataStore, fieldIds);
            return new ReductWeights(this.DataStore, fieldIds, this.WeightGenerator.Weights);            
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
