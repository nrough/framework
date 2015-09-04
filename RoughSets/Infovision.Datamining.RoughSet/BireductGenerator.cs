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
                Bireduct bireduct = (Bireduct)this.CalculateReduct(permutation.ToArray(), reductStore);
                reductStore.AddReduct(bireduct);
            }

            this.ReductPool = reductStore;                        
        }

        //public override IReduct CreateReduct(Permutation permutation)
        public override IReduct CreateReduct(int[] permutation, double epsilon, double[] weights)
        {
            IReductStore localReductStore = this.CreateReductStore();
            return this.CalculateReduct(permutation, localReductStore);            
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id)
        {
            Bireduct r = new Bireduct(this.DataStore, fieldIds, epsilon);
            r.Id = id;
            return r;
        }

        //protected virtual IReduct CalculateReduct(Permutation permutation, IReductStore reductStore)
        protected virtual IReduct CalculateReduct(int[] permutation, IReductStore reductStore)
        {
            Bireduct bireduct = this.CreateReductObject(this.DataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), 
                                                        this.Epsilon, 
                                                        this.GetNextReductId().ToString()) as Bireduct;
            
            this.Reach(bireduct, permutation, reductStore);            
            return bireduct;
        }

        //protected virtual void Reach(Bireduct bireduct, Permutation permutation, IReductStore reductStore)
        protected virtual void Reach(Bireduct bireduct, int[]  permutation, IReductStore reductStore)
        {
            for (int i = 0; i < permutation.Length; i++)
            {
                //if (permutation[i] > 0)
                if (permutation[i] < 0)
                {
                    //bireduct.TryRemoveAttribute(permutation[i]);
                    bireduct.TryRemoveAttribute(-permutation[i]);
                }
                else
                {
                    //bireduct.TryAddObject(-permutation[i]);
                    
                    //TODO Problem: Objects numbering in permutations starts from 1
                    bireduct.TryAddObject(permutation[i]-1);
                }
            }
        }

        #endregion
    }

    public class BireductFactory : IReductFactory
    {
        public virtual string FactoryKey
        {
            get { return ReductFactoryKeyHelper.Bireduct; }
        }

        public virtual IReductGenerator GetReductGenerator(Args args)
        {
            BireductGenerator rGen = new BireductGenerator();
            rGen.InitFromArgs(args);
            return rGen;
        }

        public virtual IPermutationGenerator GetPermutationGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter(ReductGeneratorParamHelper.DataStore);

            if (args.Exist(ReductGeneratorParamHelper.ApproximationRatio))
                return new PermutationGeneratorFieldObject(dataStore, (double)args.GetParameter(ReductGeneratorParamHelper.ApproximationRatio));

            return new PermutationGeneratorFieldObject(dataStore);
        }
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

    public class BireductRelativeFactory : BireductFactory
    {
        public override string FactoryKey
        {
            get { return ReductFactoryKeyHelper.BireductRelative; }
        }

        public override IReductGenerator GetReductGenerator(Args args)
        {
            BireductRelativeGenerator rGen = new BireductRelativeGenerator();
            rGen.InitFromArgs(args);
            return rGen;
        }

        public override IPermutationGenerator GetPermutationGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter(ReductGeneratorParamHelper.DataStore);

            if (args.Exist(ReductGeneratorParamHelper.ApproximationRatio))
                return new PermutationGeneratorFieldObjectRelative(dataStore, (double)args.GetParameter(ReductGeneratorParamHelper.ApproximationRatio));

            return new PermutationGeneratorFieldObjectRelative(dataStore);
        }
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

        protected override IReduct CalculateReduct(int[] permutation, IReductStore reductStore)
        {
            BireductGamma bireduct = this.CreateReductObject(this.DataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), 
                                                             this.Epsilon, 
                                                             this.GetNextReductId().ToString()) as BireductGamma;
            Reach(bireduct, permutation, reductStore);
            return bireduct;
        }

        #endregion
    }

    public class BireductGammaFactory : BireductFactory
    {
        public override string FactoryKey
        {
            get { return ReductFactoryKeyHelper.GammaBireduct; }
        }

        public override IReductGenerator GetReductGenerator(Args args)
        {
            BireductGammaGenerator rGen = new BireductGammaGenerator();
            rGen.InitFromArgs(args);
            return rGen;
        }
    }
}
