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
using System.Collections.Generic;
using NRough.Data;
using NRough.Core;
using System.Linq;

namespace NRough.MachineLearning.Roughsets
{
    [Serializable]
    public class BireductGamma : Bireduct
    {
        #region Constructors

        public BireductGamma(DataStore dataStore, IEnumerable<int> fieldIds, int[] objectIndexes, double epsilon)
            : base(dataStore, fieldIds, objectIndexes, epsilon)
        {
        }

        public BireductGamma(DataStore dataStore, IEnumerable<int> fieldIds, double epsilon)
            : this(dataStore, fieldIds, new int[] { }, epsilon)
        {
        }

        public BireductGamma(DataStore dataStore, double epsilon)
            : this(dataStore, dataStore.DataStoreInfo.SelectAttributeIds(a => a.IsStandard), new int[] { }, epsilon)
        {
        }

        public BireductGamma(BireductGamma gammaBireduct)
            : this(gammaBireduct.DataStore, gammaBireduct.Attributes.ToArray(), gammaBireduct.SupportedObjects.ToArray(),
                   gammaBireduct.Epsilon)
        {
        }

        #endregion Constructors

        #region Methods

        protected override bool CheckRemoveAttribute(int attributeId)
        {
            if (base.CheckRemoveAttribute(attributeId) == false)
                return false;

            HashSet<int> newAttributeSet = new HashSet<int>(this.Attributes);
            newAttributeSet.Remove(attributeId);

            //TODO Performance killer !!!!!!
            EquivalenceClassCollection localPartition = new EquivalenceClassCollection(this.DataStore);
            localPartition.Calc(newAttributeSet, this.DataStore);

            foreach (int objectIdx in this.SupportedObjects)
            {
                var dataVector = this.DataStore.GetFieldValues(objectIdx, newAttributeSet);
                EquivalenceClass reductStat = localPartition.Find(dataVector);

                if (reductStat.NumberOfDecisions > 1)
                {
                    return false;
                }
            }

            return true;
        }

        protected override bool CheckAddObject(int objectIndex)
        {
            if (this.SupportedObjects.Contains(objectIndex))
                return false;
            var dataVector = this.DataStore.GetFieldValues(objectIndex, this.Attributes);

            //TODO Performance killer !!!!!!
            EquivalenceClassCollection localPartition = new EquivalenceClassCollection(this.DataStore);
            localPartition.Calc(this.Attributes, this.DataStore);

            EquivalenceClass eqClass = localPartition.Find(dataVector);

            if (eqClass.NumberOfDecisions > 1)
            {
                return false;
            }

            return true;
        }

        #endregion Methods

        #region System.Object Methods

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            BireductGamma gammaBireduct = obj as BireductGamma;
            if (gammaBireduct == null)
                return false;

            return base.Equals((Bireduct)obj);
        }

        #endregion System.Object Methods

        #region ICloneable Members

        /// <summary>
        /// Clones the GammaBireduct, performing a deep copy.
        /// </summary>
        /// <returns>A new newInstance of a GammaBireduct, using a deep copy.</returns>
        public override object Clone()
        {
            return new BireductGamma(this);
        }

        #endregion ICloneable Members
    }
}