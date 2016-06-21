using System;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class BireductStore : ReductStore
    {

        #region Constructors 

        public BireductStore()
            : base()
        {            
        }

        public BireductStore(int capacity)
            : base(capacity)
        {            
        }

        #endregion

        #region Methods

        protected override bool CanAddReduct(IReduct reduct)
        {
            foreach (IReduct localReduct in this)
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
