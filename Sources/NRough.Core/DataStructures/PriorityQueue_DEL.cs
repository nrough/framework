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

using NRough.Doc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NRough.Core.DataStructures
{
    [Serializable]
    [AssemblyTreeVisible(false)]
    public class PriorityQueue_DEL<TKey, TValue>
    {
        private readonly Object lockObj;
        private SortedDictionary<TKey, Queue<TValue>> priorityQueue;

        public PriorityQueue_DEL()
        {
            lockObj = new object();
            priorityQueue = new SortedDictionary<TKey, Queue<TValue>>();
        }

        public PriorityQueue_DEL(IComparer<TKey> comparer)
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