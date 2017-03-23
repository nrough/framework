using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NRough.Data;
using NRough.Core;
using NRough.Math;
using NRough.Core.Comparers;

namespace NRough.MachineLearning.Roughsets
{
    [Serializable]
    public abstract class ReductStoreBase : IReductStore
    {
        protected readonly object mutex = new object();

        #region Properties

        public abstract int Count { get; }
        public double Weight { get; set; }
        public bool AllowDuplicates { get; set; }
        public bool IsActive { get; set; }

        #endregion Properties

        #region Constructors

        public ReductStoreBase()
        {
            this.Weight = 1.0;
            this.IsActive = true;
        }

        protected ReductStoreBase(ReductStoreBase reductStore)
        {
            this.Weight = reductStore.Weight;
            this.IsActive = true;
        }

        #endregion Constructors

        #region Methods

        public abstract IEnumerator<IReduct> GetEnumerator();

        public abstract void AddReduct(IReduct reduct);

        public abstract IReduct GetReduct(int index);

        public abstract bool IsSuperSet(IReduct reduct);

        public abstract IReductStore FilterReducts(int numberOfReducts, IComparer<IReduct> comparer);

        public abstract double GetAvgMeasure(IReductMeasure reductMeasure);

        public abstract double GetWeightedAvgMeasure(IReductMeasure reductMeasure, bool includeExceltions);

        public abstract double GetSumMeasure(IReductMeasure reductMeasure, bool includeExceltions);

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
            for (int i = this.Count - 1; i >= 0; i--)
                stringBuilder.Append(String.Format("{0}: ", i)).Append(this.GetReduct(i).ToString()).Append(Environment.NewLine);
            return stringBuilder.ToString();
        }

        #endregion System.Object Methods

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
                                    new XElement("ReductStore",
                                        new XAttribute(XNamespace.Xmlns + "xsd", xsd.NamespaceName),
                                        new XAttribute(XNamespace.Xmlns + "xsi", xsi.NamespaceName),
                                        from r in this
                                        select new XElement("Reduct",
                                            r.ToString()
                                            )));
            return xmlDoc;
        }

        #endregion Methods
    }

    [Serializable]
    public class ReductStore : ReductStoreBase
    {
        #region Members

        private List<IReduct> reducts;

        #endregion Members

        #region Properties

        public override int Count
        {
            get { return reducts.Count; }
        }

        #endregion Properties

        #region Constructors

        public ReductStore()
            : base()
        {
            this.reducts = new List<IReduct>();
        }

        public ReductStore(int capacity)
            : base()
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

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Checks if passed-in reduct is a superset of reducts already existing in the store
        /// </summary>
        /// <param name="reduct"></param>
        /// <returns></returns>
        public override bool IsSuperSet(IReduct reduct)
        {            
            lock (mutex)
            {
                foreach (IReduct localReduct in this)
                {
                    if (localReduct.Epsilon <= reduct.Epsilon)
                    {
                        if (reduct.Attributes.IsSupersetOf(localReduct.Attributes))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public IReadOnlyList<IReduct> GetReducts()
        {
            lock (mutex)
            {
                return this.reducts.ToList().AsReadOnly();
            }
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
                if (this.AllowDuplicates)
                {
                    this.DoAddReduct(reduct);
                }
                else
                {                
                    if (this.CanAddReduct(reduct))
                    {
                        this.DoAddReduct(reduct);
                    }
                }
            }
        }

        protected virtual bool CanAddReduct(IReduct reduct)
        {
            if (this.AllowDuplicates)
            {
                return true;
            }

            lock (mutex)
            {
                foreach (IReduct localReduct in reducts)
                {
                    if (reduct.GetType() == localReduct.GetType()
                        && localReduct.IsException == reduct.IsException
                        && ToleranceDoubleComparer.Instance.Equals(localReduct.Epsilon, reduct.Epsilon)
                        && reduct.Attributes.IsSupersetOf(localReduct.Attributes))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void DoAddReduct(IReduct reduct)
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

        public override double GetWeightedAvgMeasure(IReductMeasure reductMeasure, bool isGap = false)
        {
            if (reductMeasure == null)
                return 0;
            
            lock (mutex)
            {
                double measureSum = 0.0;
                double count = 0.0;
                foreach (IReduct reduct in this)
                {
                    int numberOfSupportedObjects = reduct.IsEquivalenceClassCollectionCalculated
                        ? reduct.EquivalenceClasses.CountSupportedObjects()
                        : reduct.EquivalenceClasses.NumberOfObjects;

                    measureSum += (double)reductMeasure.Calc(reduct) * numberOfSupportedObjects;
                    count += numberOfSupportedObjects;                    
                }
                if (count > 0)
                    return measureSum / count;
            }
                                    
            return 0;
        }

        public override double GetSumMeasure(IReductMeasure reductMeasure, bool isGap = false)
        {
            if (reductMeasure == null)
                return 0;

            lock (mutex)
            {
                double measureSum = 0.0;
                foreach (IReduct reduct in this)
                    measureSum += (double)reductMeasure.Calc(reduct);
                return measureSum;
            }            
        }

        public override void GetMeanStdDev(IReductMeasure reductMeasure, out double mean, out double stdDev)
        {
            mean = 0.0;
            stdDev = 0.0;
            double[] values = new double[reducts.Count];
            int i = 0;
            foreach (IReduct reduct in reducts)
                values[i++] = (double)reductMeasure.Calc(reduct);
            mean = values.Average();
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
            mean = values.Average();
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
            return this.GetReducts().GetEnumerator(); // return reducts.GetEnumerator();
        }

        #endregion IEnumerable Members

        public virtual void SaveErrorVectorsInRFormat(DataStore data, Func<IReduct, double[], RuleQualityMethod, double[]> recognition, string filePath, RuleQualityMethod decisionIdentificationMethod, string separator = ";")
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.NumberOfRecords; i++)
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

        public virtual void SaveErrorVectorsInWekaFormat(DataStore data, Func<IReduct, double[], RuleQualityMethod, double[]> recognition, string filePath, RuleQualityMethod decisionIdentificationMethod, string separator = ",")
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

        #endregion Methods
    }
}