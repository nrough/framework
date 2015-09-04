using System;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class BireductStore : ReductStore
    {
        #region Methods

        protected override Boolean CanAddReduct(IReduct reduct)
        {
            foreach (IReduct localReduct in this.ReductSet)
            {
                if (reduct.AttributeSet.Equals(localReduct.AttributeSet)
                    && reduct.ObjectSet.Equals(localReduct.ObjectSet))
                {
                    return false;
                }
            }
            return true;
        }

        #endregion
    }
}
