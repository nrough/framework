using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Infovision.Data;
using Infovision.Math;

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

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < this.Count; i++)
                stringBuilder.Append(String.Format("{0}: ", i)).Append(this.GetReduct(i).ToString()).Append(Environment.NewLine);                               
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
                this.DoAddReduct(reductClone);
            }
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
                    if (localReduct.Epsilon < reduct.Epsilon)
                    {
                        if (reduct.Attributes.Superset(localReduct.Attributes))
                        {
                            ret = true;
                            break;
                        }
                    }
                }                   
            }
            return ret;
        }
        
        public virtual ReductStore RemoveDuplicates()
        {
            ReductStore copy = new ReductStore(this);
            copy.reductSet.Sort(new ReductNumericalEpsilonComparer());

            ReductStore result = new ReductStore();
            IReduct last = null;            
            foreach (IReduct reduct in copy)
            {
                if (reduct.CompareTo(last) != 0)
                {
                    result.DoAddReduct(reduct);
                }
                last = reduct;
            }

            return result;
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
            foreach (IReduct localReduct in reductSet)
            {
                if (DoubleEpsilonComparer.NearlyEqual(localReduct.Epsilon, reduct.Epsilon, 0.000000001))
                {
                    if (reduct.Attributes.Superset(localReduct.Attributes))
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }

        public virtual void DoAddReduct(IReduct reduct)
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
                return measureSum / this.Count;
            }

            return 0;
        }

        
        public override bool Exist(IReduct reduct)
        {
            return reductSet.Exists(r => r.Equals(reduct));
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

        public virtual void SaveErrorVectorsInRFormat(DataStore data, Func<IReduct, double[], double[]> recognition, string filePath, string separator = ";")
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.NumberOfRecords; i++ )
            {
                sb.Append(separator).Append(String.Format("o{0}", i + 1));
            }
            sb.Append(Environment.NewLine);

            foreach (IReduct r in this)
            {
                sb.Append("r").Append(r.Id).Append(separator);
                double[] errors = recognition(r, data.DataStoreInfo.RecordWeights);
                for (int i = 0; i < errors.Length; i++)
                {
                    sb.Append(errors[i].ToString(CultureInfo.GetCultureInfo("en-US")));
                    if (i < errors.Length - 1)
                        sb.Append(separator);
                }
                sb.Append(Environment.NewLine);
            }
            File.WriteAllText(filePath, sb.ToString());
        }

        public virtual void SaveErrorVectorsInWekaFormat(DataStore data, Func<IReduct, double[], double[]> recognition, string filePath, string separator = ",")
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.NumberOfRecords; i++)
            {
                sb.Append(String.Format("o{0}", i + 1));
                if (i < data.NumberOfRecords - 1)
                    sb.Append(separator);
            }
            sb.Append(Environment.NewLine);

            foreach (IReduct r in this)
            {                
                double[] errors = recognition(r, data.DataStoreInfo.RecordWeights);
                for (int i = 0; i < errors.Length; i++)
                {
                    sb.Append(errors[i].ToString(CultureInfo.GetCultureInfo("en-US")));
                    if (i < errors.Length - 1)
                        sb.Append(separator);
                }
                sb.Append(Environment.NewLine);
            }
            File.WriteAllText(filePath, sb.ToString());
        }
        
        #endregion
    }
}
