using System;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class BireductGenerator : ReductGenerator
    {
        #region Constructors

        public BireductGenerator(DataStore dataStore)
            : base(dataStore)
        {
        }

        #endregion

        #region Properties

        protected override IPermutationGenerator PermutationGenerator
        {
            get { return new PermutationGeneratorFieldObject(this.DataStore, this.ApproximationDegree); }
        }

        #endregion

        #region Methods

        //protected override IReductStore CreateReductStore(Args args)
        protected override IReductStore CreateReductStore()
        {
            return new BireductStore();
        }

        /*
        protected PermutationCollection FindOrCreatePermutationList(Args args)
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
        */

        //public override IReductStoreCollection Generate(Args args)
        public override void Generate()
        {
            //PermutationCollection permutationList = this.FindOrCreatePermutationList(args);
            IReductStore reductStore = this.CreateReductStore();
            
            //foreach (Permutation permutation in permutationList)
            foreach (Permutation permutation in this.Permutations)
            {               
                Bireduct bireduct = (Bireduct)this.CalculateReduct(permutation, reductStore);
                reductStore.AddReduct(bireduct);
            }

            this.ReductPool = reductStore;
            
            ReductStoreCollection reductStoreCollection = new ReductStoreCollection();
            reductStoreCollection.AddStore(reductStore);
            this.ReductStoreCollection = reductStoreCollection;        
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double approxDegree, string id)
        {
            Bireduct r = new Bireduct(this.DataStore, fieldIds, approxDegree);
            r.Id = id;
            return r;
        }

        protected virtual IReduct CalculateReduct(Permutation permutation, IReductStore reductStore)
        {
            Bireduct bireduct = this.CreateReductObject(this.DataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), 0, this.GetNextReductId().ToString()) as Bireduct;
            this.Reach(bireduct, permutation, reductStore);
            return bireduct;
        }

        protected virtual void Reach(Bireduct bireduct, Permutation permutation, IReductStore reductStore)
        {
            for (int i = 0; i < permutation.Length; i++)
            {
                if (permutation[i] > 0)
                {
                    bireduct.RemoveAttribute(permutation[i]);
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

        public BireductRelativeGenerator(DataStore dataStore)
            : base(dataStore)
        {
        }

        #endregion

        #region Properties

        protected override IPermutationGenerator PermutationGenerator
        {
            get { return new PermutationGeneratorFieldObjectRelative(this.DataStore, this.ApproximationDegree); }
        }

        #endregion
    }

    [Serializable]
    public class BireductGammaGenerator : BireductGenerator
    {
        #region Constructors

        public BireductGammaGenerator(DataStore dataStore)
            : base(dataStore)
        {
        }

        #endregion

        #region Methods

        protected override IReduct CreateReductObject(int[] fieldIds, double approxDegree, string id)
        {
            
            BireductGamma r = new BireductGamma(this.DataStore, approxDegree);
            r.Id = id;
            return r;
        }

        protected override IReduct CalculateReduct(Permutation permutation, IReductStore reductStore)
        {
            BireductGamma bireduct = this.CreateReductObject(this.DataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), 
                                                             this.ApproximationDegree, 
                                                             this.GetNextReductId().ToString()) as BireductGamma;
            Reach(bireduct, permutation, reductStore);
            return bireduct;
        }

        #endregion
    }
}
