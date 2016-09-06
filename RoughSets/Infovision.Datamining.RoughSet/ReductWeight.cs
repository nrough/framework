using System;
using System.Collections.Generic;
using Infovision.Data;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class ReductWeights : Reduct
    {
        #region Constructors

        public ReductWeights(DataStore dataStore, IEnumerable<int> fieldIds, double epsilon, double[] objectWeights)
            : base(dataStore, fieldIds, epsilon, objectWeights)
        {
        }

        public ReductWeights(DataStore dataStore, IEnumerable<int> fieldIds, double epsilon, double[] objectWeights, EquivalenceClassCollection equivalenceClasses)
            : base(dataStore, fieldIds, epsilon, objectWeights, equivalenceClasses)
        {
        }

        public ReductWeights(ReductWeights reduct)
            : base(reduct)
        {
        }

        #endregion Constructors

        #region ICloneable Members

        /// <summary>
        /// Clones the Reduct, performing a deep copy.
        /// </summary>
        /// <returns>A new newInstance of a FieldSet, using a deep copy.</returns>
        public override object Clone()
        {
            return new ReductWeights(this);
        }

        #endregion ICloneable Members

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
            return base.GetHashCode();
            /*
            return HashHelper.GetHashCode(
                HashHelper.GetHashCode(this.Attributes.Data),
                HashHelper.GetHashCode(
                    this.Weights[0],
                    this.Weights[(int)(this.Weights.Length - 1) / 2],
                    this.Weights[this.Weights.Length - 1]));
            */
        }

        #endregion System.Object Methods
    }
}