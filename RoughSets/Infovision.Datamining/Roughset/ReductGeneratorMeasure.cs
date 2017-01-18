using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raccoon.Data;
using Raccoon.Core;
using Raccoon.MachineLearning.Permutations;

namespace Raccoon.MachineLearning.Roughset
{
    [Serializable]
    public abstract class ReductGeneratorMeasure : ReductGenerator
    {
        #region Members

        private IInformationMeasure informationMeasure;
        private double dataSetQuality;
        protected EquivalenceClassCollection initialEqClasses;

        #endregion Members

        #region Constructors

        protected ReductGeneratorMeasure()
            : base()
        {
            this.dataSetQuality = Double.MinValue;
            this.UsePerformanceImprovements = true;
        }

        #endregion Constructors

        #region Properties

        protected double DataSetQuality
        {
            get
            {
                if (!this.IsDataSetQualityCalculated())
                    this.CalcDataSetQuality();

                return this.dataSetQuality;
            }
        }

        protected IInformationMeasure InformationMeasure
        {
            get { return this.informationMeasure; }
            set { this.informationMeasure = value; }
        }

        public bool UsePerformanceImprovements { get; set; }

        #endregion Properties

        #region Methods

        public override void InitFromArgs(Args args)
        {
            base.InitFromArgs(args);

            if (args.Exist(ReductGeneratorParamHelper.DataSetQuality))
                this.dataSetQuality = args.GetParameter<double>(ReductGeneratorParamHelper.DataSetQuality);

            if (args.Exist(ReductGeneratorParamHelper.InitialEquivalenceClassCollection))
            {
                this.initialEqClasses = args.GetParameter<EquivalenceClassCollection>(
                    ReductGeneratorParamHelper.InitialEquivalenceClassCollection);
                this.dataSetQuality = this.InformationMeasure.Calc(this.initialEqClasses);
            }

            this.CalcDataSetQuality();
        }

        protected bool IsDataSetQualityCalculated()
        {
            return this.dataSetQuality > 0;
        }

        protected virtual void CalcDataSetQuality()
        {
            if (!this.IsDataSetQualityCalculated())
            {
                IReduct tmpReduct = this.CreateReductObject(this.DataStore.GetStandardFields(), 0, "tmpReduct");
                this.dataSetQuality = this.informationMeasure.Calc(tmpReduct);
            }
        }

        protected virtual void CreateReductStoreFromPermutationCollection(PermutationCollection permutationList)
        {
            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = RaccoonConfiguration.MaxDegreeOfParallelism
            };

            IReductStore reductStore = this.CreateReductStore(permutationList.Count);
            //foreach (Permutation permutation in permutationList)
            Parallel.ForEach(permutationList, options, permutation =>
            {
                IReduct reduct = this.CalculateReduct(permutation.ToArray(), reductStore, this.UseCache, this.Epsilon);
                reductStore.AddReduct(reduct);
            }
            );

            this.ReductPool = reductStore;
        }

        protected override void Generate()
        {
            this.CreateReductStoreFromPermutationCollection(this.Permutations);
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id)
        {
            Reduct r = new Reduct(this.DataStore, fieldIds, epsilon);
            r.Id = id;
            return r;
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id, EquivalenceClassCollection equivalenceClasses)
        {
            Reduct r = new Reduct(this.DataStore, fieldIds, epsilon, this.DataStore.Weights, equivalenceClasses);
            r.Id = id;
            return r;
        }

        public override IReduct CreateReduct(int[] permutation, double epsilon, double[] weights, IReductStore reductStore = null, IReductStoreCollection reductStoreCollection = null)
        {
            IReductStore localReductStore = this.CreateReductStore();
            return this.CalculateReduct(permutation, localReductStore, false, epsilon);
        }

        protected virtual IReduct CalculateReduct(int[] permutation, IReductStore reductStore, bool useCache, double epsilon)
        {
            IReduct reduct = null;

            if (this.UsePerformanceImprovements)
            {
                if (this.initialEqClasses != null
                    && this.Epsilon < 0.5
                    && permutation.Length < this.DataStore.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard) / 2)
                {
                    reduct = this.CreateReductObject(this.initialEqClasses.Attributes, epsilon, this.GetNextReductId().ToString(), this.initialEqClasses);
                    this.Reduce(reduct, permutation, reductStore, useCache);
                }
                else
                {

                    reduct = this.CreateReductObject(new int[] { }, epsilon, this.GetNextReductId().ToString());
                    this.Reach(reduct, permutation, reductStore, useCache);
                    this.Reduce(reduct, permutation, reductStore, useCache);
                }
            }
            else
            {
                if (this.initialEqClasses != null)
                {
                    reduct = this.CreateReductObject(this.initialEqClasses.Attributes, epsilon, this.GetNextReductId().ToString(), this.initialEqClasses);
                    this.Reduce(reduct, permutation, reductStore, useCache);
                }
                else
                {
                    reduct = this.CreateReductObject(permutation, epsilon, this.GetNextReductId().ToString());
                    this.Reduce(reduct, permutation, reductStore, useCache);
                }
            }

            return reduct;
        }

        public virtual IReduct CalculateReduct(int[] attributes)
        {
            //IReduct reduct = this.CreateReductObject(new int[] { }, this.Epsilon, this.GetNextReductId().ToString());
            //this.Reach(reduct, attributes, null, false);

            IReduct reduct = this.CreateReductObject(attributes, this.Epsilon, this.GetNextReductId().ToString());
            this.ReduceForward(reduct, attributes, null, false);

            return reduct;
        }

        protected virtual void Reach(IReduct reduct, int[] permutation, IReductStore reductStore, bool useCache)
        {
            for (int i = 0; i < permutation.Length; i++)
            {
                reduct.AddAttribute(permutation[i]);
                if (this.IsReduct(reduct, reductStore, useCache))
                    return;
            }
        }

        protected virtual void Reduce(IReduct reduct, int[] permutation, IReductStore reductStore, bool useCache)
        {
            int len = permutation.Length - 1;
            for (int i = len; i >= 0; i--)
            {
                int attributeId = permutation[i];
                if (reduct.TryRemoveAttribute(attributeId))
                {
                    if (!this.IsReduct(reduct, reductStore, useCache))
                        reduct.AddAttribute(attributeId);
                }
            }
        }

        protected virtual void ReduceForward(IReduct reduct, int[] permutation, IReductStore reductStore, bool useCache)
        {
            //Console.WriteLine(permutation.ToStr(' '));

            for (int i = 0; i < permutation.Length; i++)
            {
                //Console.WriteLine("Try to remove {0}", permutation[i]);
                if (reduct.TryRemoveAttribute(permutation[i]))
                {
                    if (!this.IsReduct(reduct, reductStore, useCache))
                    {
                        reduct.AddAttribute(permutation[i]);
                        //Console.WriteLine("Failed to remove {0}", permutation[i]);
                    }
                    //else
                    //{
                    //    Console.WriteLine("Success to remove {0}", permutation[i]);
                    //}
                }
            }
        }

        public virtual bool CheckIsReduct(IReduct reduct)
        {
            return ToleranceDoubleComparer.Instance.Compare(
                this.GetPartitionQuality(reduct),
                (1.0 - this.Epsilon) * this.DataSetQuality) != -1;
                
            //if (partitionQuality >= (1.0 - this.Epsilon) * this.DataSetQuality)
            //    return true;
            //return false;
        }

        protected virtual bool IsReduct(IReduct reduct, IReductStore reductStore, bool useCache)
        {
            string key = String.Empty;
            ReductCacheInfo reductInfo = null;

            if (useCache)
            {
                key = this.GetReductCacheKey(reduct);
                reductInfo = ReductCache.Instance.Get(key) as ReductCacheInfo;
                if (reductInfo != null)
                {
                    if (reductInfo.CheckIsReduct(this.Epsilon) == NoYesUnknown.Yes)
                        return true;
                    if (reductInfo.CheckIsReduct(this.Epsilon) == NoYesUnknown.No)
                        return false;
                }
            }

            bool isReduct = false;
            if (this.UsePerformanceImprovements && reductStore != null && reductStore.IsSuperSet(reduct))
                isReduct = true;
            else
                isReduct = this.CheckIsReduct(reduct);

            if (useCache)
                this.UpdateReductCacheInfo(reductInfo, key, isReduct);

            return isReduct;
        }

        protected virtual double GetPartitionQuality(IReduct reduct)
        {
            return this.InformationMeasure.Calc(reduct);
        }

        private void UpdateReductCacheInfo(ReductCacheInfo reductInfo, string key, bool isReduct)
        {
            if (reductInfo != null)
            {
                reductInfo.SetApproximationRanges(isReduct, this.Epsilon);
            }
            else
            {
                ReductCache.Instance.Set(key, new ReductCacheInfo(isReduct, this.Epsilon));
            }
        }

        protected virtual string GetReductCacheKey(IReduct reduct)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("m=").Append(this.GetType().Name);
            stringBuilder.Append("|d=").Append(this.DataStore.Name);
            stringBuilder.Append("|a=").Append(reduct.Attributes.ToArray().ToStr(' '));
            return stringBuilder.ToString();
        }

        #endregion Methods
    }

    public abstract class ApproximateReductFactory : IReductFactory
    {
        public abstract string FactoryKey { get; }

        public virtual IPermutationGenerator GetPermutationGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter(ReductGeneratorParamHelper.TrainData);
            return new PermutationGeneratorReverse(dataStore);
        }

        public abstract IReductGenerator GetReductGenerator(Args args);
    }

    [Serializable]
    public class ReductGeneratorRelative : ReductGeneratorMeasure
    {
        #region Constructors

        public ReductGeneratorRelative()
            : base()
        {            
            this.InformationMeasure = InformationMeasureRelative.Instance;
        }

        #endregion Constructors
    }

    public class ApproximateReductRelativeFactory : ApproximateReductFactory
    {
        public override string FactoryKey
        {
            get { return ReductFactoryKeyHelper.ApproximateReductRelative; }
        }

        public override IReductGenerator GetReductGenerator(Args args)
        {
            ReductGeneratorRelative rGen = new ReductGeneratorRelative();
            rGen.InitFromArgs(args);
            return rGen;
        }
    }

    [Serializable]
    public class ReductGeneratorMajority : ReductGeneratorMeasure
    {
        #region Constructors

        public ReductGeneratorMajority()
            : base()
        {            
            this.InformationMeasure = InformationMeasureMajority.Instance;
        }

        #endregion Constructors
    }

    public class ApproximateReductMajorityFactory : ApproximateReductFactory
    {
        public override string FactoryKey
        {
            get { return ReductFactoryKeyHelper.ApproximateReductMajority; }
        }

        public override IReductGenerator GetReductGenerator(Args args)
        {
            ReductGeneratorMajority rGen = new ReductGeneratorMajority();
            rGen.InitFromArgs(args);
            return rGen;
        }
    }

    [Serializable]
    public class ReductGeneratorPositive : ReductGeneratorMeasure
    {
        #region Constructors

        public ReductGeneratorPositive()
            : base()
        {
            this.InformationMeasure = InformationMeasurePositive.Instance;
        }

        #endregion Constructors
    }

    public class ApproximateReductPositiveFactory : ApproximateReductFactory
    {
        public override string FactoryKey
        {
            get { return ReductFactoryKeyHelper.ApproximateReductPositive; }
        }

        public override IReductGenerator GetReductGenerator(Args args)
        {
            ReductGeneratorPositive rGen = new ReductGeneratorPositive();
            rGen.InitFromArgs(args);
            return rGen;
        }
    }
}