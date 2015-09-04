using System;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class BireductGenerator : ReductGenerator
    {       
        #region Properties

        protected override IPermutationGenerator PermutationGenerator
        {
            get { return new PermutationGeneratorFieldObject(this.DataStore, this.Epsilon); }
        }

        #endregion

        #region Constructors

        public BireductGenerator() : base()
        {
        }

        #endregion

        #region Methods

        protected override IReductStore CreateReductStore()
        {
            return new BireductStore();
        }      
        
        public override void Generate()
        {
            IReductStore reductStore = this.CreateReductStore();
                        
            foreach (Permutation permutation in this.Permutations)
            {               
                Bireduct bireduct = (Bireduct)this.CalculateReduct(permutation, reductStore);
                reductStore.AddReduct(bireduct);
            }

            this.ReductPool = reductStore;                        
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id)
        {
            Bireduct r = new Bireduct(this.DataStore, fieldIds, epsilon);
            r.Id = id;
            return r;
        }

        protected virtual IReduct CalculateReduct(Permutation permutation, IReductStore reductStore)
        {
            Bireduct bireduct = this.CreateReductObject(this.DataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), 
                                                        0, 
                                                        this.GetNextReductId().ToString()) as Bireduct;
            
            this.Reach(bireduct, permutation, reductStore);
            return bireduct;
        }

        protected virtual void Reach(Bireduct bireduct, Permutation permutation, IReductStore reductStore)
        {
            for (int i = 0; i < permutation.Length; i++)
            {
                if (permutation[i] > 0)
                {
                    bireduct.TryRemoveAttribute(permutation[i]);
                }
                else
                {
                    bireduct.AddObject(-permutation[i]);
                }
            }
        }

        #endregion
    }

    [Serializable]
    public class BireductRelativeGenerator : BireductGenerator
    {
        #region Constructors

        public BireductRelativeGenerator()
            : base()
        {
        }

        #endregion

        #region Properties

        protected override IPermutationGenerator PermutationGenerator
        {
            get { return new PermutationGeneratorFieldObjectRelative(this.DataStore, this.Epsilon); }
        }

        #endregion
    }

    [Serializable]
    public class BireductGammaGenerator : BireductGenerator
    {
        #region Constructors

        public BireductGammaGenerator() : base()
        {
        }

        #endregion

        #region Methods

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id)
        {
            
            BireductGamma r = new BireductGamma(this.DataStore, epsilon);
            r.Id = id;
            return r;
        }

        protected override IReduct CalculateReduct(Permutation permutation, IReductStore reductStore)
        {
            BireductGamma bireduct = this.CreateReductObject(this.DataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), 
                                                             this.Epsilon, 
                                                             this.GetNextReductId().ToString()) as BireductGamma;
            Reach(bireduct, permutation, reductStore);
            return bireduct;
        }

        #endregion
    }
}
