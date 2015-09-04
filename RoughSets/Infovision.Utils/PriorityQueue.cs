using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Utils
{        
    [Serializable]
    public class PriorityQueue<TKey, TValue>
    {
        private readonly Object lockObj;
        private SortedDictionary<TKey, Queue<TValue>> priorityQueue;

        public PriorityQueue()
        {
            lockObj = new object();
            priorityQueue = new SortedDictionary<TKey, Queue<TValue>>();
        }

        public PriorityQueue(IComparer<TKey> comparer)
        {
            lockObj = new object();
            priorityQueue = new SortedDictionary<TKey, Queue<TValue>>(comparer);
        }
        
        public void Enqueue(TKey key, TValue value)
        {
            lock (lockObj)
            {
                if (key != null)
                {
                    if (priorityQueue.ContainsKey(key))
                    {
                        Queue<TValue> dataList = priorityQueue[key];
                        dataList.Enqueue(value);
                    }
                    else
                    {
                        Queue<TValue> dataList = new Queue<TValue>();
                        dataList.Enqueue(value);
                        priorityQueue.Add(key, dataList);
                    }
                }
            }
        }

        public TValue Dequeue()
        {
            lock (lockObj)
            {                
                if (priorityQueue.Count == 0)
                    return default(TValue);
                
                KeyValuePair<TKey, Queue<TValue>> kvp = priorityQueue.First();
                TValue value = kvp.Value.Dequeue();
                if (kvp.Value.Count == 0)
                {
                    priorityQueue.Remove(kvp.Key);
                }
                return value;
            }
        }

        public int Count()
        {
            lock (lockObj)
            {
                return priorityQueue.Values.Sum(q => q.Count);
            }
        }

        public TValue Peek()
        {
            lock (lockObj)
            {
                KeyValuePair<TKey, Queue<TValue>> kvp = priorityQueue.First();
                return kvp.Value.Peek();                
            }
        }
    }
}
