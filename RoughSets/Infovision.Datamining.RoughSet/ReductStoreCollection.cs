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
        //List<IReductStore> ActiveModels();
    }

    [Serializable]
    public class ReductStoreCollection : IReductStoreCollection
    {
        List<IReductStore> stores;

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
            this.stores.Add(reductStore);            
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
            return stores.FindAll(x => x.IsActive == true);
        }

        public double GetAvgMeasure(IReductMeasure reductMeasure, bool includeExceptions = false)
        {
            if (reductMeasure == null)
                return 0.0;

            double measureSum = 0.0;
            int count = 0;
            foreach(IReductStore reducts in this)
                foreach (IReduct reduct in reducts)
                {
                    if (reduct.IsException && includeExceptions == false)
                        continue;

                    measureSum += (double)reductMeasure.Calc(reduct);
                    count++;
                }

            if (count > 0)
                return measureSum / (double)count;

            return 0;
        }
    }
}
