using System;
using System.Text;
using Infovision.Core;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class Permutation : ICloneable
    {
        #region Globals

        private int[] elements;

        #endregion Globals

        #region Constructors

        public Permutation(int[] elements)
        {
            this.elements = new int[elements.Length];
            Buffer.BlockCopy(elements, 0, this.elements, 0, elements.Length * sizeof(int));
        }

        #endregion Constructors

        #region Properties

        public int this[int index]
        {
            get { return elements[index]; }
        }

        public int Length
        {
            get { return elements.Length; }
        }

        #endregion Properties

        #region Methods

        public int[] ToArray()
        {
            return elements;
        }

        public virtual Permutation Reverse()
        {
            int[] localPermutation = new int[this.Length];
            int j = this.Length - 1;
            for (int i = 0; i < this.Length; i++)
            {
                localPermutation[i] = this[j];
                j--;
            }
            return new Permutation(localPermutation);
        }

        #region System.Object Methods

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (int i in elements)
                stringBuilder.Digits(i).Append(' ');
            return stringBuilder.ToString();
        }

        #endregion System.Object Methods

        #region ICloneable Members

        /// <summary>
        /// Clones the Permutation, performing a deep copy.
        /// </summary>
        /// <returns>A new newInstance of a Permutation, using a deep copy.</returns>
        public object Clone()
        {
            Permutation p = new Permutation(this.elements);
            return p;
        }

        #endregion ICloneable Members

        #endregion Methods
    }
}