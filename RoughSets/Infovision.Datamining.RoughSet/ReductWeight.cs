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

        public ReductWeights(DataStore dataStore, int[] fieldIds, decimal[] objectWeights, decimal epsilon)
            : base(dataStore, fieldIds, epsilon, objectWeights)
            //: base(dataStore, fieldIds, epsilon)
        {
            /*
            if (objectWeights != null)
            {
                this.Weights = new decimal[objectWeights.Length];
                Array.Copy(objectWeights, this.Weights, objectWeights.Length);
            }
            */
        }

        public ReductWeights(DataStore dataStore, int[] fieldIds, decimal epsilon)
            : this(dataStore, fieldIds, null, epsilon)
        {
        }

        public ReductWeights(DataStore dataStore, decimal epsilon)
            : this(dataStore, new int[] { }, epsilon)
        {
        }

        public ReductWeights(ReductWeights reduct)
            : base(reduct)
        {
        }

        #endregion

        #region ICloneable Members
        /// <summary>
        /// Clones the Reduct, performing a deep copy.
        /// </summary>
        /// <returns>A new newInstance of a FieldSet, using a deep copy.</returns>
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
            
            if (base.Equals(obj) == false)
                return false;
            
            for (int i = 0; i < this.Weights.Length; i++)
                if (reduct.Weights[i] != this.Weights[i])
                    return false;

            return true;
        }

        public override int GetHashCode()
        {
            return HashHelper.GetHashCode(
                HashHelper.GetHashCode(this.Attributes.Data),
                HashHelper.GetHashCode(
                    this.Weights[0], 
                    this.Weights[(int)(this.Weights.Length - 1) / 2], 
                    this.Weights[this.Weights.Length - 1]));
        }

        #endregion
    }
}
