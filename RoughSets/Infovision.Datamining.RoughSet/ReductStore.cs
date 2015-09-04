﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public abstract class ReductStoreBase : IReductStore
    {
        private readonly object syncRoot = new Object();

        #region Properties

        public abstract int Count { get; }

        public object SyncRoot
        {
            get { return syncRoot; }
        }
        
        #endregion

        #region Methods

        public abstract IEnumerator<IReduct> GetEnumerator();
        public abstract void AddReduct(IReduct reduct);        
        public abstract IReduct GetReduct(int index);
        public abstract bool IsSuperSet(IReduct reduct);
        public abstract IReductStore FilterReducts(int numberOfReducts, IComparer<IReduct> comparer);
        public abstract double GetAvgMeasure(IReductMeasure reductMeasure);
        public abstract bool Exist(IReduct reduct);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator<IReduct>)GetEnumerator();
        }

        #region System.Object Methods

        public override String ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (IReduct reduct in this)
            {
                stringBuilder.AppendLine(reduct.ToString());
            }
            return stringBuilder.ToString();
        }
        #endregion

        public virtual int CompareTo(Object obj)
        {
            return 0;
        }

        public void Save(string fileName)
        {
            XDocument xmlDoc = this.GetXDocument();
            xmlDoc.Save(fileName);
        }

        public XDocument GetXDocument()
        {
            XNamespace xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
            XNamespace xsd = XNamespace.Get("http://www.w3.org/2001/XMLSchema");

            XDocument xmlDoc = new XDocument(
                                    new XDeclaration("1.0", "utf-8", null),
                                    new XElement("Reducts",
                                        new XAttribute(XNamespace.Xmlns + "xsd", xsd.NamespaceName),
                                        new XAttribute(XNamespace.Xmlns + "xsi", xsi.NamespaceName),
                                        from r in this
                                        select new XElement("Reduct",
                                            r.ToString()
                                            )));
            return xmlDoc;
        }

        #endregion
    }

    [Serializable]
    public class ReductStore : ReductStoreBase
    {
        #region Globals

        private List<IReduct> reductSet;

        #endregion

        #region Constructors

        public ReductStore()
        {
            this.reductSet = new List<IReduct>();
        }

        protected ReductStore(ReductStore reductStore)
            : this()
        {
            foreach (IReduct reduct in reductStore)
            {
                IReduct reductClone = (IReduct)reduct.Clone();
                this.AddReduct(reductClone);
            }
        }

        #endregion

        #region Properties

        public override int Count
        {
            get { return reductSet.Count; }
        }

        public List<IReduct> ReductSet
        {
            get { return this.reductSet; }
        }

        #endregion

        #region Methods
       
        /// <summary>
        /// Checks if passed-in reduct is a superset of reducts already existing in the store
        /// </summary>
        /// <param name="reduct"></param>
        /// <returns></returns>
        public override bool IsSuperSet(IReduct reduct)
        {
            bool ret = false;

            lock (this.SyncRoot)
            {
                foreach (IReduct localReduct in reductSet)
                {                    
                    if (reduct.AttributeSet.Superset(localReduct.AttributeSet))
                    {
                        ret = true;
                        break;
                    }
                }                   
            }
            
            return ret;
        }

        public override IReduct GetReduct(int index)
        {
            return this.reductSet[index];
        }

        public override void AddReduct(IReduct reduct)
        {
            if (this.CanAddReduct(reduct))
            {
                this.DoAddReduct(reduct);
            }            
        }

        protected virtual bool CanAddReduct(IReduct reduct)
        {
            if (this.IsSuperSet(reduct))
                return false;
            return true;
        }

        internal virtual void DoAddReduct(IReduct reduct)
        {
            reductSet.Add(reduct);
        }

        public IReductStore FilterReducts(int numberOfReducts, IReductMeasure reductMeasure)
        {
            if (numberOfReducts == 0 || numberOfReducts >= this.Count)
            {
                return new ReductStore(this);
            }
            
            Dictionary<IReduct, double> reductOrderMap = new Dictionary<IReduct, double>(reductSet.Count);
            foreach (IReduct reduct in reductSet)
            {
                reductOrderMap[reduct] = reductMeasure.Calc(reduct);
            }

            IOrderedEnumerable<KeyValuePair<IReduct, double>> sortedReducts;
            switch (reductMeasure.SortDirection)
            {
                case SortDirection.Ascending:
                    sortedReducts = reductOrderMap.OrderBy(kvp => kvp.Value);
                    break;

                case SortDirection.Descending:
                    sortedReducts = reductOrderMap.OrderByDescending(kvp => kvp.Value);
                    break;
                
                default :
                    sortedReducts = reductOrderMap.OrderBy(kvp => kvp.Value);
                    break;
            }

            ReductStore result = new ReductStore();
            int i = 0;
            foreach (KeyValuePair<IReduct, double> kvp in sortedReducts)
            {
                IReduct reductClone = (IReduct)kvp.Key.Clone();

                result.AddReduct(reductClone);
                
                i++;
                if (i >= numberOfReducts)
                {
                    break;
                }
            }

            return result;
        }

        public override IReductStore FilterReducts(int numberOfReducts, IComparer<IReduct> comparer)
        {
            if (numberOfReducts == 0 || numberOfReducts >= this.Count)
            {
                return new ReductStore(this);
            }

            reductSet.Sort(comparer);

            ReductStore result = new ReductStore();
            int i = 0;
            foreach (IReduct reduct in reductSet)
            {
                IReduct reductClone = (IReduct)reduct.Clone();

                result.AddReduct(reductClone);

                i++;
                if (i >= numberOfReducts)
                {
                    break;
                }
            }

            return result;
        }

        public override double GetAvgMeasure(IReductMeasure reductMeasure)
        {            
            if (reductMeasure == null)
                return 0;

            double measureSum = 0;
            foreach (IReduct reduct in reductSet)
            {
                measureSum += reductMeasure.Calc(reduct);
            }

            if (this.Count > 0)
            {
                return measureSum / (double) this.Count;
            }

            return 0;
        }

        
        public override bool Exist(IReduct reduct)
        {
            return reductSet.Exists(r => r.Equals(reduct));

            /*
            foreach (IReduct r in reductSet)
            {
                if (r.Equals(reduct))
                {
                    return true;
                }
            }

            return false;
            */
        }

        #region IEnumerable Members
        /// <summary>
        /// Returns an IEnumerator to enumerate through the reduct store.
        /// </summary>
        /// <returns>An IEnumerator instance.</returns>
        public override IEnumerator<IReduct> GetEnumerator()
        {
            return reductSet.GetEnumerator();
        }

        #endregion        
        
        #endregion
    }
}
