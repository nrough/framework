using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace NRough.MachineLearning.Roughsets
{
    public interface IReductStoreCollection : IEnumerable<IReductStore>
    {
        void AddStore(IReductStore reductStore);

        IReadOnlyList<IReductStore> GetStoreList();

        int Count { get; }
        bool ReductPerStore { get; set; }

        double GetAvgMeasure(IReductMeasure reductMeasure, bool includeExceptions = true, bool exceptionOnly = false);
        double GetAvgSumPerStoreMeasure(IReductMeasure reductMeasure, bool includeExceptions = true, bool exceptionOnly = false);
        
        double GetWeightedAvgMeasure(IReductMeasure reductMeasure, bool includeExceptions = true);
        double GetWeightedAvgPerEnsembleMeasure(IReductMeasure reductMeasure, bool includeExceptions = false);

        IReductStoreCollection Filter(int numberOfReducts, IComparer<IReduct> comparer);

        IReductStoreCollection FilterInEnsemble(int ensembleSize, IComparer<IReductStore> comparer);
    }

    [Serializable]
    public class ReductStoreCollection : IReductStoreCollection
    {
        private List<IReductStore> stores;
        protected readonly object mutex = new object();

        public int Count { get { return this.stores.Count; } }

        public bool ReductPerStore { get; set; }

        public ReductStoreCollection()
        {
            this.stores = new List<IReductStore>();
        }

        public ReductStoreCollection(int capacity)
        {
            this.stores = new List<IReductStore>(capacity);
        }

        public void AddStore(IReductStore reductStore)
        {
            lock (mutex)
            {
                this.stores.Add(reductStore);
            }
        }

        public IReadOnlyList<IReductStore> GetStoreList()
        {
            lock (mutex)
            {
                return this.stores.ToList().AsReadOnly();
            }
        }

        public IEnumerator<IReductStore> GetEnumerator()
        {
            return this.GetStoreList().GetEnumerator(); // return stores.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetStoreList().GetEnumerator(); //return stores.GetEnumerator();
        }

        public double GetAvgSumPerStoreMeasure(IReductMeasure reductMeasure, bool includeExceptions = false, bool exceptionsOnly = false)
        {
            if (reductMeasure == null)
                return 0.0;
            double measureSum = 0.0;
            int count = 0;

            lock (mutex)
            {
                foreach (IReductStore reducts in this)
                {
                    foreach (IReduct reduct in reducts)
                    {
                        if (reduct.IsException && includeExceptions == false)
                            continue;

                        if (!reduct.IsException && exceptionsOnly)
                            continue;

                        measureSum += (double)reductMeasure.Calc(reduct);                        
                    }

                    count++;
                }
            }

            if (count > 0)
                return measureSum / (double)count;

            return 0;
        }

        public double GetAvgMeasure(IReductMeasure reductMeasure, bool includeExceptions = false, bool exceptionsOnly = false)
        {
            if (reductMeasure == null)
                return 0.0;
            double measureSum = 0.0;
            int count = 0;

            lock (mutex)
            {
                foreach (IReductStore reducts in this)
                {
                    foreach (IReduct reduct in reducts)
                    {
                        if (reduct.IsException && includeExceptions == false)
                            continue;

                        if (!reduct.IsException && exceptionsOnly)
                            continue;

                        measureSum += (double)reductMeasure.Calc(reduct);
                        count++;
                    }
                }
            }

            if (count > 0)
                return measureSum / (double)count;

            return 0;
        }

        public double GetWeightedAvgPerEnsembleMeasure(IReductMeasure reductMeasure, bool includeExceptions = false)
        {
            if (reductMeasure == null)
                return 0.0;
            
            double globalSum = 0.0;

            lock (mutex)
            {
                foreach (IReductStore reducts in this)
                {
                    double sum = 0.0;
                    double denominator = 0.0;
                    foreach (IReduct reduct in reducts)
                    {
                        int numberOfSupportedObjects = reduct.IsEquivalenceClassCollectionCalculated
                            ? reduct.EquivalenceClasses.CountSupportedObjects()
                            : reduct.EquivalenceClasses.NumberOfObjects;

                        if (reduct.IsException && includeExceptions == false)
                            continue;

                        sum += reductMeasure.Calc(reduct) * numberOfSupportedObjects;
                        denominator += numberOfSupportedObjects;
                    }

                    if (denominator > 0)
                        globalSum += sum / denominator;
                }
            }

            if (Count > 0)
                return globalSum / (double)Count;

            return 0;
        }

        public double GetWeightedAvgMeasure(IReductMeasure reductMeasure, bool includeExceptions = false)
        {
            if (reductMeasure == null)
                return 0.0;

            double sum = 0.0;
            double denominator = 0.0;            

            lock (mutex)
            {
                foreach (IReductStore reducts in this)
                {
                    foreach (IReduct reduct in reducts)
                    {
                        int numberOfSupportedObjects = reduct.IsEquivalenceClassCollectionCalculated
                            ? reduct.EquivalenceClasses.CountSupportedObjects()
                            : reduct.EquivalenceClasses.NumberOfObjects;

                        if (reduct.IsException && includeExceptions == false)
                            continue;

                        sum += reductMeasure.Calc(reduct) * numberOfSupportedObjects;
                        denominator += numberOfSupportedObjects;
                    }                    
                }
            }

            if (denominator > 0)
                return sum / denominator;

            return 0;
        }

        public IReductStoreCollection Filter(int numberOfReducts, IComparer<IReduct> comparer)
        {
            ReductStoreCollection result = new ReductStoreCollection(this.Count);
            lock (mutex)
            {
                foreach (IReductStore reductStore in this)
                {
                    IReductStore filteredStore = reductStore.FilterReducts(numberOfReducts, comparer);
                    result.AddStore(filteredStore);
                }
            }

            return result;
        }

        public IReductStoreCollection FilterInEnsemble(int count, IComparer<IReductStore> comparer)
        {
            if ((this.Count < count) || count == 0)
                count = this.Count;

            ReductStoreCollection result = new ReductStoreCollection(count);
            IReductStore[] reductStoreArray = this.ToArray();
            Array.Sort(reductStoreArray, comparer);
            for (int i = 0; i < count; i++)
                result.AddStore(reductStoreArray[i]);
            return result;
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
                                    new XElement("ReductStoreCollection",
                                        new XAttribute(XNamespace.Xmlns + "xsd", xsd.NamespaceName),
                                        new XAttribute(XNamespace.Xmlns + "xsi", xsi.NamespaceName),
                                        from r in this
                                        select new XElement("ReductStore",
                                            r.ToString()
                                            )));
            return xmlDoc;
        }
    }
}