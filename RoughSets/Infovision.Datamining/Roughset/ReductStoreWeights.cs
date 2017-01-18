namespace Raccoon.MachineLearning.Roughset
{
    public class ReductStoreWeights : ReductStore
    {
        //TODO beeing super set is OK, but w must be different
        protected override bool CanAddReduct(IReduct reduct)
        {
            if (this.IsSuperSet(reduct))
                return false;
            return true;
        }

        public override bool IsSuperSet(IReduct reduct)
        {
            return base.IsSuperSet(reduct);
        }
    }
}