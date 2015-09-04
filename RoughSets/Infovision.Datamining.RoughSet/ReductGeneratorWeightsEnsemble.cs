using System;
using System.Collections;
using System.Collections.Generic;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{

    public abstract class ReductGeneratorWeightsEnsemble : ReductGeneratorWeights
    {
        protected WeightGeneratorEnsemble wgen;
        //private int numberOfAttributes;
        private List<IReductStore> permutationReductStore;

        #region Constructors

        public ReductGeneratorWeightsEnsemble(DataStore dataStore)
            : base(dataStore)
        {

        }

        #endregion

        #region Properties
        
        public virtual int NumberOfIterations
        {
            get;
            set;
        }

        public List<IReductStore> PermutationReductStore
        {
            get
            {
                return this.permutationReductStore;
            }
        }

        #endregion

        #region Methods

        protected override IReduct CreateReductObject(int[] fieldIds)
        {
            return base.CreateReductObject(fieldIds);
        }

        //public override IReductStore Generate(Args args)
        public override IReductStoreCollection Generate(Args args)
        {
            //numberOfAttributes = (int)((double)DataStore.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard) * this.ApproximationLevel);

            PermutationCollection permutationList = this.FindOrCreatePermutationCollection(args);            
            return this.CreateReductStoreFromPermutationCollection(permutationList, args);                        
        }

        protected override IReduct CalculateReduct(Permutation permutation, IReductStore reductStore, bool useCache)
        {
            /*
            int[] elements = new int[this.numberOfAttributes];
            for (int i = 0; i < this.numberOfAttributes; i++)
            {
                elements[i] = permutation[i];
            }
            Permutation p = new Permutation(elements);
            
            return base.CalculateReduct(p, reductStore); 
            */

            return base.CalculateReduct(permutation, reductStore, useCache);
        }

        protected override IReductStoreCollection CreateReductStoreFromPermutationCollection(PermutationCollection permutationList, Args args)
        {
            bool useCache = false;
            if (args.Exist("USECACHE"))
                useCache = true;

            IReductStore reductStore = this.CreateReductStore(args);
            permutationReductStore = new List<IReductStore>(permutationList.Count);

            foreach (Permutation permutation in permutationList)
            {
                IReductStore localReductStore = this.CreateReductStore(args);
                for (int i = 0; i < this.NumberOfIterations; i++)
                {
                    IReduct reduct = this.CalculateReduct(permutation, localReductStore, useCache);
                    localReductStore.AddReduct(reduct);
                    this.wgen.NewReduct(reduct);
                }

                permutationReductStore.Add(localReductStore);
            }

            ReductStoreCollection reductStoreCollection = new ReductStoreCollection();
            reductStoreCollection.AddStore(reductStore);

            return reductStoreCollection;
        }

        #endregion
    }
    
    [Serializable]
    public class ReductGeneratorWeightsEnsembleRelative : ReductGeneratorWeightsEnsemble
    {
        #region Constructors

        public ReductGeneratorWeightsEnsembleRelative(DataStore dataStore)
            : base(dataStore)
        {
            
        }

        #endregion

        #region Properties

        protected override WeightGenerator WeightGenerator
        {
            get
            {
                if (this.wgen == null)
                {
                    this.wgen = new WeightGeneratorEnsembleRelative(this.DataStore);
                }

                return this.wgen;
            }
        }

        #endregion
    }

    [Serializable]
    public class ReductGeneratorWeightsEnsembleMajority : ReductGeneratorWeightsEnsemble
    {
        #region Constructors

        public ReductGeneratorWeightsEnsembleMajority(DataStore dataStore)
            : base(dataStore)
        {

        }

        #endregion

        #region Properties

        protected override WeightGenerator WeightGenerator
        {
            get
            {
                if (this.wgen == null)
                {
                    this.wgen = new WeightGeneratorEnsembleMajority(this.DataStore);
                }

                return this.wgen;
            }
        }

        #endregion
    }
}
