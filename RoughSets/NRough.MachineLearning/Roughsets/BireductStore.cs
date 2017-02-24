using System;

namespace NRough.MachineLearning.Roughsets
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

        #endregion Constructors

        #region Methods

        protected override bool CanAddReduct(IReduct reduct)
        {
            foreach (IReduct localReduct in this)
            {
                if (reduct.Attributes.Equals(localReduct.Attributes)
                    && reduct.SupportedObjects.Equals(localReduct.SupportedObjects))
                {
                    return false;
                }
            }
            return true;
        }

        #endregion Methods
    }
}