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
using Infovision.Statistics;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public abstract class ReductStoreBase : IReductStore
    {
        protected object mutex = new object();

        #region Properties

        public abstract int Count { get; }
        public decimal Weight { get; set; }
        public bool AllowDuplicates { get; set; }
        public bool IsActive { get; set; }
        
        #endregion

        #region Constructors        

        public ReductStoreBase()
        {
            this.Weight = Decimal.One;
            this.IsActive = true;
        }

        public ReductStoreBase(int capacity)
        {
            this.Weight = Decimal.One;
            this.IsActive = true;
        }

        protected ReductStoreBase(ReductStoreBase reductStore)
        {
            this.Weight = reductStore.Weight;
            this.IsActive = true;
        }

        #endregion

        #region Methods

        public abstract IEnumerator<IReduct> GetEnumerator();
        public abstract void AddReduct(IReduct reduct);        
        public abstract IReduct GetReduct(int index);
        public abstract bool IsSuperSet(IReduct reduct);
        public abstract IReductStore FilterReducts(int numberOfReducts, IComparer<IReduct> comparer);
        public abstract double GetAvgMeasure(IReductMeasure reductMeasure);
        public abstract double GetWeightedAvgMeasure(IReductMeasure reductMeasure, bool includeExceltions);
        public abstract void GetMeanStdDev(IReductMeasure reductMeasure, out double mean, out double stdDev);
        public abstract void GetMeanAveDev(IReductMeasure reductMeasure, out double mean, out double aveDev);
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
            XNamespace xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-newInstance");
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
        #region Members

        private List<IReduct> reducts;

        #endregion

        #region Properties

        public override int Count
        {
            get { return reducts.Count; }
        }

        public List<IReduct> ReductSet
        {
            get { return this.reducts; }
        }        

        #endregion

        #region Constructors

        public ReductStore()
            : base()
        {
            this.reducts = new List<IReduct>();
        }

        public ReductStore(int capacity)
            : base(capacity)
        {
            this.reducts = capacity != 0 ? new List<IReduct>(capacity) : new List<IReduct>();
        }

        protected ReductStore(ReductStore reductStore)
            : base(reductStore as ReductStoreBase)
        {
            this.reducts = new List<IReduct>(reductStore.Count);
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
            lock (mutex)
            {
                foreach (IReduct localReduct in reducts)
                {
                    if (localReduct.Epsilon <= reduct.Epsilon)
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
            copy.reducts.Sort(new ReductNumericalEpsilonComparer());

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
            return this.reducts[index];
        }

        public override void AddReduct(IReduct reduct)
        {
            lock (mutex)
            {
                if (this.CanAddReduct(reduct))
                {
                    this.DoAddReduct(reduct);
                }
            }
        }

        protected virtual bool CanAddReduct(IReduct reduct)
        {
            lock (mutex)
            {
                foreach (IReduct localReduct in reducts)
                {
                    /*
                    if (this.AllowDuplicates == false
                        && EpsilonComparer.NearlyEqual(localReduct.Epsilon, reduct.Epsilon, 0.000000001)
                        && reduct.Attributes.Superset(localReduct.Attributes))
                    {                    
                        return false;                    
                    }
                    */

                    if (this.AllowDuplicates == false
                        && localReduct.Epsilon == reduct.Epsilon
                        && reduct.Attributes.Superset(localReduct.Attributes))
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }

        public virtual void DoAddReduct(IReduct reduct)
        {
            lock (mutex)
            {
                reducts.Add(reduct);
            }
        }

        public override IReductStore FilterReducts(int numberOfReducts, IComparer<IReduct> comparer)
        {
            if (numberOfReducts == 0 || numberOfReducts >= this.Count)
            {
                return new ReductStore(this);
            }

            lock (mutex)
            {
                reducts.Sort(comparer);
            }

            ReductStore result = new ReductStore(System.Math.Min(numberOfReducts, reducts.Count));
            int i = 0;
            
            foreach (IReduct reduct in reducts)
            {
                IReduct reductClone = (IReduct)reduct.Clone();
                result.AddReduct(reductClone);

                i++;
                if (i >= numberOfReducts)
                    break;
            }

            return result;
        }

        public override double GetAvgMeasure(IReductMeasure reductMeasure)
        {            
            if (reductMeasure == null)
                return 0;

            double measureSum = 0;
            foreach (IReduct reduct in reducts)
                measureSum += (double)reductMeasure.Calc(reduct);

            if (this.Count > 0)
            {
                return measureSum / this.Count;
            }

            return 0;
        }

        public override double GetWeightedAvgMeasure(IReductMeasure reductMeasure, bool includeExceptions = false)
        {
            if (reductMeasure == null)
                return 0.0;
            double measureSum = 0.0;
            int count = 0;

            lock (mutex)
            {                                
                foreach (IReduct reduct in this)
                {
                    if (reduct.IsException && includeExceptions == false)
                    {
                        count += reduct.ObjectSetInfo.NumberOfRecords;
                    }
                    else
                    {
                        measureSum += (double)reductMeasure.Calc(reduct) * reduct.ObjectSetInfo.NumberOfRecords;
                        count += reduct.ObjectSetInfo.NumberOfRecords;
                    }
                }
            }

            if (count > 0)
                return measureSum / (double)count;

            return 0;
        }

        public override void GetMeanStdDev(IReductMeasure reductMeasure, out double mean, out double stdDev)
        {
            mean = 0.0;
            stdDev = 0.0;
            double[] values = new double[reducts.Count];
            int i = 0;
            foreach (IReduct reduct in reducts)
                values[i++] = (double)reductMeasure.Calc(reduct);
            mean = Tools.Mean(values);
            stdDev = Tools.StdDev(values, mean);            
        }

        public override void GetMeanAveDev(IReductMeasure reductMeasure, out double mean, out double aveDev)
        {
            mean = 0.0;
            aveDev = 0.0;
            double[] values = new double[reducts.Count];
            int i = 0;
            foreach (IReduct reduct in reducts)
                values[i++] = (double)reductMeasure.Calc(reduct);
            mean = Tools.Mean(values);
            aveDev = Tools.AveDev(values, mean);
        }

        
        public override bool Exist(IReduct reduct)
        {
            return reducts.Exists(r => r.Equals(reduct));
        }

        #region IEnumerable Members
        /// <summary>
        /// Returns an IEnumerator to enumerate through the reduct store.
        /// </summary>
        /// <returns>An IEnumerator newInstance.</returns>
        public override IEnumerator<IReduct> GetEnumerator()
        {
            return reducts.GetEnumerator();
        }

        #endregion        

        public virtual void SaveErrorVectorsInRFormat(DataStore data, Func<IReduct, decimal[], RuleQualityFunction, double[]> recognition, string filePath, RuleQualityFunction decisionIdentificationMethod, string separator = ";")
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
                double[] errors = recognition(r, r.Weights, decisionIdentificationMethod);
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

        public virtual void SaveErrorVectorsInWekaFormat(DataStore data, Func<IReduct, decimal[], RuleQualityFunction, double[]> recognition, string filePath, RuleQualityFunction decisionIdentificationMethod, string separator = ",")
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
                double[] errors = recognition(r, r.Weights, decisionIdentificationMethod);
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
