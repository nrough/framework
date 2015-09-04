using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class ReductWeights : Reduct
    {
        #region Constructors

        public ReductWeights(DataStore dataStore, int[] fieldIds, double[] objectWeights)
            : base(dataStore, fieldIds)
        {
            this.objectWeights = objectWeights;
        }

        public ReductWeights(DataStore dataStore, int[] fieldIds)
            : this(dataStore, fieldIds, null)
        {
        }

        public ReductWeights(DataStore dataStore)
            : this(dataStore, new int[] { })
        {
        }

        public ReductWeights(ReductWeights reduct)
            : base((Reduct)reduct)
        {
        }

        #endregion

        #region ICloneable Members
        /// <summary>
        /// Clones the Reduct, performing a deep copy.
        /// </summary>
        /// <returns>A new instance of a FieldSet, using a deep copy.</returns>
        public override object Clone()
        {
            return new ReductWeights(this);
        }
        #endregion

        #region System.Object Methods

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            ReductWeights reduct = obj as ReductWeights;
            if (reduct == null)
                return false;

            for (int i = 0; i < this.objectWeights.Length; i++)
                if (reduct.Weights[i] != this.Weights[i])
                    return false;

            return base.Equals(obj);


            //TODO weights should also be checked   
        }

        public override int GetHashCode()
        {
            //TODO weights should also be checked what is optimal way of calculating hash, and not scanning the whole array of weights

            return HashHelper.GetHashCode(HashHelper.GetHashCode(this.AttributeSet.Data), HashHelper.GetHashCode(this.Weights));
        }

        #endregion
    }
}
