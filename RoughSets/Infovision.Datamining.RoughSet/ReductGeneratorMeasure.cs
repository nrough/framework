using System;
using System.Text;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public abstract class ReductGeneratorMeasure : ReductGenerator
    {
        #region Members

        private IInformationMeasure informationMeasure;        
        private double dataSetQuality = Double.MinValue;        

        #endregion

        #region Constructors

        protected ReductGeneratorMeasure()
            : base()
        {
        }

        #endregion

        #region Properties        

        protected double DataSetQuality
        {
            get
            {
                if (this.dataSetQuality < -1.0)
                {
                    this.CalcDataSetQuality();                
                }

                return this.dataSetQuality;
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
        }

        protected virtual void CalcDataSetQuality()
        {
            IReduct reduct = this.CreateReductObject(this.DataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), 0, this.GetNextReductId().ToString());
            this.dataSetQuality = this.informationMeasure.Calc(reduct);            
        }        
        
        protected virtual void CreateReductStoreFromPermutationCollection(PermutationCollection permutationList)
        {                        
            IReductStore reductStore = this.CreateReductStore();
            foreach (Permutation permutation in permutationList)
            {
                IReduct reduct = this.CalculateReduct(permutation, reductStore, this.UseCache, this.Epsilon);
                reductStore.AddReduct(reduct);
            }

            this.ReductPool = reductStore;                        
        }
                
        public override void Generate()
        {            
            this.CreateReductStoreFromPermutationCollection(this.Permutations);
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id)
        {
            Reduct r = new Reduct(this.DataStore, fieldIds, epsilon);
            r.Id = id;
            return r;
        }

        public override IReduct CreateReduct(Permutation permutation)
        {
            IReductStore localReductStore = this.CreateReductStore();
            return this.CalculateReduct(permutation, localReductStore, false, this.Epsilon);
        }

        protected virtual IReduct CalculateReduct(Permutation permutation, IReductStore reductStore, bool useCache, double epsilon)
        {
            IReduct reduct = this.CreateReductObject(new int[] { }, 
                                                     epsilon,
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
                if (reduct.TryRemoveAttribute(attributeId))
                {
                    if (!this.IsReduct(reduct, reductStore, useCache))
                    {
                        reduct.AddAttribute(attributeId);
                    }
                }
            }            
        }

        public virtual bool CheckIsReduct(IReduct reduct)
        {
            double partitionQuality = this.GetPartitionQuality(reduct);
            double tinyDouble = 0.0001 / this.DataStore.NumberOfRecords;
            if (partitionQuality >= (((1.0 - this.Epsilon) * this.DataSetQuality) - tinyDouble))
                return true;
            return false;
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
            if (reductStore.IsSuperSet(reduct))
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
