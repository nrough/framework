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
        private double dataSetQuality = 1.0;
        private bool isQualityCalculated = false;

        #endregion

        #region Constructors

        protected ReductGeneratorMeasure()
            : base()
        {
        }

        #endregion

        #region Properties

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
                
        protected override IReductStore CreateReductStore()
        {
            return new ReductStore();

            /*
            int numberOfThreads = args.Exist("NumberOfThreads") ? (int)args.GetParameter("NumberOfThreads") : 1;
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
            IReduct reduct = this.CreateReductObject(this.DataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), 0, this.GetNextReductId().ToString());
            this.DataSetQuality = this.informationMeasure.Calc(reduct);
        }        
        
        protected virtual void CreateReductStoreFromPermutationCollection(PermutationCollection permutationList)
        {                        
            IReductStore reductStore = this.CreateReductStore();
            foreach (Permutation permutation in permutationList)
            {
                IReduct reduct = this.CalculateReduct(permutation, reductStore, this.UseCache, this.ApproximationDegree);
                reductStore.AddReduct(reduct);
            }

            this.ReductPool = reductStore;                        
        }
                
        public override void Generate()
        {            
            this.CreateReductStoreFromPermutationCollection(this.Permutations);
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double approxDegree, string id)
        {
            Reduct r = new Reduct(this.DataStore, fieldIds, approxDegree);
            r.Id = id;
            return r;
        }

        protected virtual IReduct CalculateReduct(Permutation permutation, IReductStore reductStore, bool useCache, double approxDegree)
        {
            IReduct reduct = this.CreateReductObject(new int[] { }, 
                                                     approxDegree,
                                                     this.GetNextReductId().ToString());

            this.Reach(reduct, permutation, reductStore, useCache);
            this.Reduce(reduct, permutation, reductStore, useCache);

            return reduct;
        }

        protected virtual void Reach(IReduct reduct, Permutation permutation, IReductStore reductStore, bool useCache)
        {
            for (int i = 0; i < permutation.Length; i++)
            {
                reduct.AddAttribute(permutation[i]);

                if (this.IsReduct(reduct, reductStore, useCache))
                {
                    return;
                }
            }
        }

        protected virtual void Reduce(IReduct reduct, Permutation permutation, IReductStore reductStore, bool useCache)
        {
            int len = permutation.Length - 1;
            for (int i = len; i >= 0; i--)
            {
                int attributeId = permutation[i];
                if (reduct.RemoveAttribute(attributeId))
                {
                    if (!this.IsReduct(reduct, reductStore, useCache))
                    {
                        reduct.AddAttribute(attributeId);
                    }
                }
            }            
        }

        protected virtual bool IsReduct(IReduct reduct, IReductStore reductStore, bool useCache)
        {
            string key = this.GetReductCacheKey(reduct);
            ReductCacheInfo reductInfo = null;

            if (useCache)
            {
                reductInfo = ReductCache.Instance.Get(key) as ReductCacheInfo;
                if (reductInfo != null)
                {
                    if (reductInfo.CheckIsReduct(this.ApproximationDegree) == NoYesUnknown.Yes)
                        return true;
                    if (reductInfo.CheckIsReduct(this.ApproximationDegree) == NoYesUnknown.No)
                        return false;
                }
            }

            if (reductStore.IsSuperSet(reduct))
            {
                if(useCache)
                    this.UpdateReductCacheInfo(reductInfo, key, true);
                return true;
            }

            double partitionQuality = this.GetPartitionQuality(reduct);
            double tinyDouble = 0.0001 / this.DataStore.NumberOfRecords;              
            if (partitionQuality >= (((1.0 - this.ApproximationDegree) * this.DataSetQuality) - tinyDouble))            
            {
                if(useCache)
                    this.UpdateReductCacheInfo(reductInfo, key, true);
                return true;
            }

            if(useCache)
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
                reductInfo.SetApproximationRanges(isReduct, this.ApproximationDegree);
            }
            else
            {
                ReductCache.Instance.Set(key, new ReductCacheInfo(isReduct, this.ApproximationDegree));
            }
        }


        protected virtual string GetReductCacheKey(IReduct reduct)
        {
            StringBuilder stringBuilder = new StringBuilder();            
            stringBuilder.Append("m=").Append(this.GetType().Name);
            stringBuilder.Append("|d=").Append(this.DataStore.Name);
            stringBuilder.Append("|a=").Append(reduct.Attributes.CacheKey);
            return stringBuilder.ToString();
        }

        #endregion
    }

    [Serializable]
    public class ReductGeneratorRelative : ReductGeneratorMeasure
    {
        #region Constructors

        public ReductGeneratorRelative()
            : base()
        {
            //TODO Move to SetDefaultParameters()
            this.InformationMeasure = (IInformationMeasure)InformationMeasureBase.Construct(InformationMeasureType.Relative);
        }

        #endregion
    }

    [Serializable]
    public class ReductGeneratorMajority : ReductGeneratorMeasure
    {
        #region Constructors

        public ReductGeneratorMajority()
            : base()
        {
            //TODO Move to SetDefaultParameters()
            this.InformationMeasure = (IInformationMeasure)InformationMeasureBase.Construct(InformationMeasureType.Majority);
        }

        #endregion
    }

    [Serializable]
    public class ReductGeneratorPositive : ReductGeneratorMeasure
    {
        #region Constructors

        public ReductGeneratorPositive()
            : base()
        {
            //TODO Move to SetDefaultParameters()
            this.InformationMeasure = (IInformationMeasure)InformationMeasureBase.Construct(InformationMeasureType.Positive);
        }

        #endregion
    }
}
