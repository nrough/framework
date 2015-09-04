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

        protected virtual IPermutationGenerator PermutationGenerator
        {
            get { return new PermutationGeneratorFieldObject(this.DataStore, this.ApproximationDegree); }
        }

        #endregion

        #region Methods

        protected override IReductStore CreateReductStore(Args args)
        {
            return new BireductStore();
        }

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
        
        public override IReductStoreCollection Generate(Args args)
        {
            PermutationCollection permutationList = this.FindOrCreatePermutationList(args);
            IReductStore reductStore = this.CreateReductStore(args);
            foreach (Permutation permutation in permutationList)
            {               
                Bireduct bireduct = (Bireduct)this.CalculateReduct(permutation, reductStore);
                reductStore.AddReduct(bireduct);
            }
            
            ReductStoreCollection reductStoreCollection = new ReductStoreCollection();
            reductStoreCollection.AddStore(reductStore);
            return reductStoreCollection;        
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double approxDegree)
        {
            return new Bireduct(this.DataStore, fieldIds, approxDegree);
        }

        protected virtual IReduct CalculateReduct(Permutation permutation, IReductStore reductStore)
        {
            Bireduct bireduct = this.CreateReductObject(this.DataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), 0) as Bireduct;
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
    public class GammaBireductGenerator : BireductGenerator
    {
        #region Constructors

        public GammaBireductGenerator(DataStore dataStore)
            : base(dataStore)
        {
        }

        #endregion

        #region Methods

        protected override IReduct CreateReductObject(int[] fieldIds, double approxDegree)
        {
            return new GammaBireduct(this.DataStore, approxDegree);
        }

        protected override IReduct CalculateReduct(Permutation permutation, IReductStore reductStore)
        {
            GammaBireduct bireduct = this.CreateReductObject(this.DataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), this.ApproximationDegree) as GammaBireduct;
            Reach(bireduct, permutation, reductStore);
            return bireduct;
        }

        #endregion
    }
}
