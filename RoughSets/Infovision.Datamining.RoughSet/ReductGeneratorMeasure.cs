using System;
using System.Text;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public abstract class ReductGeneratorMeasure : ReductGenerator
    {
        #region Globals

        private IInformationMeasure informationMeasure;
        private IPermutationGenerator permutationGenerator;
        private double dataSetQuality = 1.0;
        private bool isQualityCalculated = false;

        #endregion

        #region Constructors

        protected ReductGeneratorMeasure(DataStore dataStore)
            : base(dataStore)
        {
        }

        #endregion

        #region Properties

        protected IPermutationGenerator PermutationGenerator
        {
            //get { return new PermutationGeneratorReverse(this.DataStore.DataStoreInfo.GetFieldIds(false)); }
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
            get { return this.isQualityCalculated; }
            set { this.isQualityCalculated = value; }
        }

        protected double DataSetQuality
        {
            get
            {
                if (!this.isQualityCalculated)
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

        protected IInformationMeasure InformationMeasure
        {
            get { return this.informationMeasure; }
            set { this.informationMeasure = value; }
        }

        #endregion

        #region Methods

        protected override IReductStore CreateReductStore(Args args)
        {
            return new ReductStore();

            /*
            Int32 numberOfThreads = args.Exist("NumberOfThreads") ? (int)args.GetParameter("NumberOfThreads") : 1;
            if (numberOfThreads > 1)
            {
                return new ReductStoreMulti(numberOfThreads);
            }
            else
            {
                return new ReductStore();
            }
            */
        }

        protected virtual void CalcDataSetQuality()
        {
            IReduct reduct = this.CreateReductObject(this.DataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard));
            this.DataSetQuality = this.informationMeasure.Calc(reduct);
        }

        protected virtual PermutationList FindOrCreatePermutationList(Args args)
        {
            PermutationList permutationList = null;
            if (args.Exist("PermutationList"))
            {
                permutationList = (PermutationList)args.GetParameter("PermutationList");
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
                throw new NullReferenceException("PermutationList is null");
            }

            return permutationList;
        }

        protected virtual IReductStore CreateReductStoreFromPermutationList(PermutationList permutationList, Args args)
        {
            IReductStore reductStore = this.CreateReductStore(args);
            foreach (Permutation permutation in permutationList)
            {
                IReduct reduct = this.CalculateReduct(permutation, reductStore);
                reductStore.AddReduct(reduct);
            }

            return reductStore;
        }

        public override IReductStore Generate(Args args)
        {
            PermutationList permutationList = this.FindOrCreatePermutationList(args);
            return this.CreateReductStoreFromPermutationList(permutationList, args);
        }

        protected override IReduct CreateReductObject(int[] fieldIds)
        {
            return new Reduct(this.DataStore, fieldIds);
        }

        protected virtual IReduct CalculateReduct(Permutation permutation, IReductStore reductStore)
        {
            IReduct reduct = this.CreateReductObject(new int[]{});

            this.Reach(reduct, permutation, reductStore);
            this.Reduce(reduct, permutation, reductStore);

            return reduct;
        }

        protected virtual void Reach(IReduct reduct, Permutation permutation, IReductStore reductStore)
        {
            DataStoreInfo info = this.DataStore.DataStoreInfo;

            for (int i = 0; i < permutation.Length; i++)
            {
                reduct.AddAttribute(permutation[i]);

                if (this.IsReduct(reduct, reductStore))
                {
                    return;
                }
            }
        }

        protected virtual void Reduce(IReduct reduct, Permutation permutation, IReductStore reductStore)
        {
            int len = permutation.Length - 1;
            for (int i = len; i >= 0; i--)
            {
                int attributeId = permutation[i];
                if (reduct.RemoveAttribute(attributeId))
                {
                    if (!this.IsReduct(reduct, reductStore))
                    {
                        reduct.AddAttribute(attributeId);
                    }
                }
            }

            /*
            for (int i = permutation.Length - 1; i >= 0; i--)
            {
                if (reduct.ContainsAttribute(permutation[i]))
                {
                    reduct.RemoveAttribute(permutation[i]);
                    if (!this.IsReduct(reduct, reductStore))
                    {
                        reduct.AddAttribute(permutation[i]);
                    }
                }
            }
            */
        }

        protected virtual bool IsReduct(IReduct reduct, IReductStore reductStore)
        {
            string key = this.GetReductCacheKey(reduct);
            ReductCacheInfo reductInfo = ReductCache.Instance.Get(key) as ReductCacheInfo;
            if (reductInfo != null)
            {
                if (reductInfo.CheckIsReduct(this.ApproximationLevel) == NoYesUnknown.Yes)
                    return true;
                if (reductInfo.CheckIsReduct(this.ApproximationLevel) == NoYesUnknown.No)
                    return false;
            }

            if (reductStore.IsSuperSet(reduct))
            {
                this.UpdateReductCacheInfo(reductInfo, key, true);
                return true;
            }

            double partitionQuality = this.GetPartitionQuality(reduct);

            if (partitionQuality >= ((((double)1 - this.ApproximationLevel) * this.DataSetQuality) - (0.0001 / (double)this.DataStore.NumberOfRecords)))
            // >>>> if (partitionQuality >= (((double)1 - this.ApproximationLevel - (0.0001 / (double)this.DataStore.NumberOfRecords)) * this.DataSetQuality) )
            {
                this.UpdateReductCacheInfo(reductInfo, key, true);
                return true;
            }

            this.UpdateReductCacheInfo(reductInfo, key, false);
            return false;
        }

        protected virtual double GetPartitionQuality(IReduct reduct)
        {
            return this.InformationMeasure.Calc(reduct);
        }

        private void UpdateReductCacheInfo(ReductCacheInfo reductInfo, string key, bool isReduct)
        {
            if (reductInfo != null)
            {
                reductInfo.SetApproximationRanges(isReduct, this.ApproximationLevel);
            }
            else
            {
                ReductCache.Instance.Set(key, new ReductCacheInfo(isReduct, this.ApproximationLevel));
            }
        }

        protected virtual string GetReductCacheKey(IReduct reduct)
        {
            StringBuilder stringBuilder = new StringBuilder();

            // >>>> stringBuilder.Append("m=").Append(this.informationMeasure.ToString());

            stringBuilder.Append("m=").Append(this.GetType().Name);
            stringBuilder.Append("|d=").Append(this.DataStore.Name);
            stringBuilder.Append("|a=").Append(reduct.AttributeSet.CacheKey);

            return stringBuilder.ToString();
        }

        #endregion
    }

    [Serializable]
    public class ReductGeneratorRelative : ReductGeneratorMeasure
    {
        #region Constructors

        public ReductGeneratorRelative(DataStore dataStore)
            : base(dataStore)
        {
            this.InformationMeasure = (IInformationMeasure)InformationMeasureBase.Construct(InformationMeasureType.Relative);
        }

        #endregion
    }

    [Serializable]
    public class ReductGeneratorMajority : ReductGeneratorMeasure
    {
        #region Constructors

        public ReductGeneratorMajority(DataStore dataStore)
            : base(dataStore)
        {
            this.InformationMeasure = (IInformationMeasure)InformationMeasureBase.Construct(InformationMeasureType.Majority);
        }

        #endregion
    }

    [Serializable]
    public class ReductGeneratorPositive : ReductGeneratorMeasure
    {
        #region Constructors

        public ReductGeneratorPositive(DataStore dataStore)
            : base(dataStore)
        {
            this.InformationMeasure = (IInformationMeasure)InformationMeasureBase.Construct(InformationMeasureType.Positive);
        }

        #endregion
    }
}
