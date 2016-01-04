using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset
{
    public interface IReductStoreCollection : IEnumerable<IReductStore>
    {
        void AddStore(IReductStore reductStore);
        int Count { get; }
        double GetAvgMeasure(IReductMeasure reductMeasure, bool includeExceptions = true);
        IReductStoreCollection Filter(int numberOfReducts, IComparer<IReduct> comparer);
    }

    [Serializable]
    public class ReductStoreCollection : IReductStoreCollection
    {
        List<IReductStore> stores;
        protected object mutex = new object();

        public int Count { get { return this.stores.Count; } }

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

        public IEnumerator<IReductStore> GetEnumerator()
        {
            return stores.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return stores.GetEnumerator();
        }

        public List<IReductStore> ActiveModels()
        {
            lock (mutex)
            {
                return stores.FindAll(x => x.IsActive == true);
            }
        }

        public double GetAvgMeasure(IReductMeasure reductMeasure, bool includeExceptions = false)
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

                        measureSum += (double)reductMeasure.Calc(reduct);
                        count++;
                    }
                }
            }

            if (count > 0)
                return measureSum / (double)count;

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
    } 
}
