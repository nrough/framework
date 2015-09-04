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
                if (reduct.Attributes.Equals(localReduct.Attributes)
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
