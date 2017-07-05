// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

using System;
using System.Text;
using NRough.Core;
using NRough.Core.Helpers;

namespace NRough.MachineLearning.Permutations
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