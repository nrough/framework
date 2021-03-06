﻿// 
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
using System.Linq;
using System.Text;
using NRough.Data;
using NRough.Core;
using NRough.Core.Helpers;
using NRough.Core.CollectionExtensions;

namespace NRough.MachineLearning.Roughsets
{
    [Serializable]
    public class Reduct : IReduct, IFormattable
    {
        #region Members

        private HashSet<int> attributeSet;
        protected readonly object syncRoot = new object();

        //TODO To Remove
        private double[] objectWeights;

        private DataStore dataStore;
        protected EquivalenceClassCollection eqClassMap;

        #endregion Members

        #region Properties

        public string Id { get; set; }

        public DataStore DataStore
        {
            get { return this.dataStore; }
        }

        public double[] Weights
        {
            get { return this.objectWeights; }
            protected set { this.objectWeights = value; }
        }

        public HashSet<int> Attributes
        {
            get { return this.attributeSet; }
        }

        public double Epsilon { get; private set; }
        
        public virtual HashSet<int> SupportedObjects
        {
            get { return new HashSet<int>(Enumerable.Range(0, this.dataStore.NumberOfRecords).ToArray()); }
        }        

        public virtual EquivalenceClassCollection EquivalenceClasses
        {
            get
            {
                if (this.eqClassMap == null)
                {
                    lock (syncRoot)
                    {
                        if (this.eqClassMap == null)
                        {
                            this.eqClassMap = EquivalenceClassCollection.Create(this.Attributes.ToArray(), this.DataStore, this.Weights);
                        }
                    }
                }

                return this.eqClassMap;
            }
        }

        public bool IsEquivalenceClassCollectionCalculated
        {
            get { return this.eqClassMap != null; }
        }

        public bool IsException { get; set; }

        #endregion Properties

        #region Constructors

        public Reduct(DataStore dataStore, IEnumerable<int> fieldIds, double epsilon, double[] weights, EquivalenceClassCollection equivalenceClasses)
            : this(dataStore, fieldIds, epsilon, weights)
        {
            this.eqClassMap = equivalenceClasses;
        }

        public Reduct(DataStore dataStore, IEnumerable<int> fieldIds, double epsilon, double[] weights)
        {
            this.dataStore = dataStore;
            this.attributeSet = new HashSet<int>(fieldIds);
            this.Epsilon = epsilon;

            if (weights != null)
            {
                this.objectWeights = weights;
            }
            else
            {
                this.objectWeights = new double[this.dataStore.NumberOfRecords];
                for (int i = 0; i < dataStore.NumberOfRecords; i++)
                    this.objectWeights[i] = 1.0 / this.dataStore.NumberOfRecords;
            }
        }

        public Reduct(DataStore dataStore, IEnumerable<int> fieldIds, double epsilon)
        {
            this.dataStore = dataStore;
            this.attributeSet = new HashSet<int>(fieldIds);
            this.Epsilon = epsilon;

            this.objectWeights = new double[this.dataStore.NumberOfRecords];
            double w = 1.0 / this.dataStore.NumberOfRecords;
            for (int i = 0; i < dataStore.NumberOfRecords; i++)
                this.objectWeights[i] = w;
        }

        public Reduct(DataStore dataStore, double epsilon)
            : this(dataStore, new int[] { }, epsilon)
        {
        }

        public Reduct(DataStore dataStore)
            : this(dataStore, new int[] { }, 0.0)
        {
        }

        public Reduct(Reduct reduct)
        {
            this.attributeSet = new HashSet<int>(reduct.attributeSet);
            this.dataStore = reduct.DataStore;
            this.Epsilon = reduct.Epsilon;
            this.objectWeights = new double[dataStore.NumberOfRecords];
            this.Id = reduct.Id;
            Array.Copy(reduct.Weights, this.objectWeights, reduct.DataStore.NumberOfRecords);
            this.IsException = reduct.IsException;
            this.eqClassMap = (EquivalenceClassCollection)reduct.EquivalenceClasses.Clone();
        }

        #endregion Constructors

        #region Methods

        public virtual void SetEquivalenceClassCollection(EquivalenceClassCollection equivalenceClasses)
        {
            this.eqClassMap = equivalenceClasses;
        }

        protected virtual bool CheckRemoveAttribute(int attributeId)
        {
            return this.Attributes.Contains(attributeId);
        }

        public virtual bool AddAttribute(int attributeId)
        {
            lock (syncRoot)
            {
                if (this.attributeSet.Add(attributeId))
                {
                    this.eqClassMap = null;
                    return true;
                }
            }
            return false;
        }        

        public virtual bool TryRemoveAttribute(int attributeId)
        {
            lock (syncRoot)
            {
                if (this.attributeSet.Remove(attributeId))
                {
                    this.eqClassMap = null;
                    return true;
                }
            }
            return false;           
        }

        //TODO ContainsObject remove this method
        public virtual bool ContainsObject(int objectIndex)
        {
            return true;
        }

        #region ICloneable Members

        /// <summary>
        /// Clones the Reduct, performing a deep copy.
        /// </summary>
        /// <returns>A new newInstance of a FieldSet, using a deep copy.</returns>
        public virtual object Clone()
        {
            return new Reduct(this);
        }

        #endregion ICloneable Members

        #region IComparable Members

        public virtual int CompareTo(object reduct)
        {
            Reduct r = reduct as Reduct;
            return this.CompareTo(r);
        }

        public virtual int CompareTo(Reduct r)
        {
            if (r == null)
                return 1;

            int attrCount = this.Attributes.Count;
            int retval = attrCount.CompareTo(r.Attributes.Count);

            if (retval != 0)
                return retval;

            int[] thisAttributes = this.Attributes.ToArray();
            int[] rAttributes = r.Attributes.ToArray();

            for (int i = 0; i < attrCount; i++)
            {                
                if (thisAttributes[i] < rAttributes[i])
                    return -1;
                else if (thisAttributes[i] > rAttributes[i])
                    return 1;
            }

            return 0;
        }

        #endregion IComparable Members

        #region System.Object Methods

        public override string ToString()
        {
            if (this.IsException == false)
            {
                return String.Format(
                    "[Id:{0}] {1} (eps:{2:0.00}, rules:{3})",
                    this.Id,
                    this.attributeSet.Count > 0 ? this.attributeSet.ToArray().ToStr(" ") : "empty",
                    this.Epsilon,
                    (eqClassMap == null) ? "??" : eqClassMap.Count.ToString()
                    );
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(                    
                    "[Id:{0}] {1} (eps:{2:0.00}, rules:{3}) ",
                    this.Id,
                    this.attributeSet.Count > 0 ? this.attributeSet.ToArray().ToStr(" ") : "empty",
                    this.Epsilon,
                    (eqClassMap == null) ? "??" : eqClassMap.Count.ToString()
                    );

                sb.Append("[");

                bool first = true;
                foreach (EquivalenceClass eq in this.EquivalenceClasses)
                {
                    foreach (int objectIdx in eq.ObjectIndexes)
                    {
                        if (first)
                        {
                            sb.Append(objectIdx);
                            first = false;
                        }
                        else
                        {
                            sb.AppendFormat(" {0}", objectIdx);
                        }
                    }
                }
                sb.Append("]");
                return sb.ToString();
            }
        }

        public virtual string ToString(string format, IFormatProvider fp)
        {
            if (format != null && format.Equals("ext"))
            {
                StringBuilder sb = new StringBuilder();
                int[] fieldIds = this.attributeSet.ToArray();
                int attrCount = this.attributeSet.Count;
                for (int i = 0; i < attrCount; i++)
                {
                    if (i != 0)
                        sb.Append(' ');
                    sb.Append(this.DataStore.DataStoreInfo.GetFieldInfo(fieldIds[i]).Alias);
                }

                return String.Format(
                    "[Id:{0}] {1} (eps:{2:0.00})",
                    this.Id,
                    this.attributeSet.Count > 0 ? sb.ToString() : "empty",
                    this.Epsilon);
            }
            else
                return this.ToString();
        }

        public override int GetHashCode()
        {
            return HashHelper.GetHashCode(this.attributeSet.ToArray());
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            Reduct reduct = obj as Reduct;
            if (reduct == null)
                return false;

            return this.attributeSet.SetEquals(reduct.attributeSet);
        }

        #endregion System.Object Methods

        #endregion Methods
    }

    public class ReductNumericalEpsilonComparer : Comparer<IReduct>
    {
        public override int Compare(IReduct x, IReduct y)
        {
            if (x == null)
            {
                return y == null ? 0 : -1;
            }
            else
            {
                if (y == null)
                    return 1;
                else
                {
                    int retval = x.Attributes.Count.CompareTo(y.Attributes.Count);
                    if (retval != 0)
                        return retval;
                    else
                    {
                        int[] xAttributes = x.Attributes.ToArray();
                        int[] yAttributes = y.Attributes.ToArray();

                        for (int i = 0; i < x.Attributes.Count; i++)
                        {                            
                            if (xAttributes[i] < yAttributes[i])
                                return 1;
                            else if (xAttributes[i] > yAttributes[i])
                                return -1;
                        }

                        return x.Epsilon.CompareTo(y.Epsilon);
                    }
                }
            }
        }
    }
}