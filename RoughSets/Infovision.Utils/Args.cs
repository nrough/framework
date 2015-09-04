using System;
using System.Collections;
using System.Collections.Generic;

namespace Infovision.Utils
{
    /*
     public class LogDictionary: IDictionary<String, object>
    {
        private IDictionary<String, object> _dte;

        public LogDictionary(DynamicTableEntity dte)
        {
            _dte = (IDictionary<String, object>)dte;
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            return _dte.Remove(item.Key);
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return _dte.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
     */
    
    [Serializable]
    public class Args
        : IEnumerable<KeyValuePair<string, object>>
    {
        private Dictionary<string, object> parameters = null;
        private List<string> parameterOrder = null;
        private Dictionary<int, string> index2parameter = null;
        private int nextIndex = 0;
        
        public Args()
        {
            this.parameters = new Dictionary<string, object>();
            this.parameterOrder = new List<string>();
            this.index2parameter = new Dictionary<int, string>();
            this.nextIndex = 0;
        }

        public Args(int numberOfArgs)
        {
            this.parameters = new Dictionary<string, object>(numberOfArgs);
            this.parameterOrder = new List<string>(numberOfArgs);
            this.index2parameter = new Dictionary<int, string>(numberOfArgs);
        }

        public Args(string key, object value)
        {
            this.parameters = new Dictionary<string, object>(1);
            this.parameters[key] = value;
        }

        public Args(string[] keys, object[] values)
        {
            if (keys.Length != values.Length)
            {
                throw new ArgumentException("Value array must have the same length as the array of keys", "values");
            }

            this.parameters = new Dictionary<string, object>(keys.Length);
            this.index2parameter = new Dictionary<int, string>(keys.Length);

            for (int i = 0; i < keys.Length; i++)
            {
                this.parameters[keys[i]] = values[i];
                this.index2parameter[i] = keys[i];
            }

            this.parameterOrder = new List<string>(keys.Length);
            this.parameterOrder.AddRange(keys);
            this.nextIndex = keys.Length;
        }

        public int Count
        {
            get { return this.parameters.Count; }
        }

        public void AddParameter(string key, object value)
        {
            this.parameters[key] = value;
            this.parameterOrder.Add(key);
            this.index2parameter[this.nextIndex] = key;
            this.nextIndex++;
        }

        //TODO Remove parameters
        public void RemoveParameter(string key)
        {
            this.parameters.Remove(key);
        }

        public object GetParameter(string key)
        {
            if (this.Exist(key))
            {
                return this.parameters[key];
            }

            return null;
        }

        public object GetParameter(int index)
        {
            string key;
            if (this.index2parameter.TryGetValue(index, out key))
            {
                return this.parameters[key];
            }

            return null;
        }

        public bool Exist(string key)
        {
            return this.parameters.ContainsKey(key);
        }

        public string[] GetParameterNames()
        {
            return this.parameterOrder.ToArray();
        }

        public string GetParameterName(int index)
        {
            return this.parameterOrder[index];
        }

        /*
        public bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            return this.parameters.Remove(item.Key);
        }

        public void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            this.AddParameter(item.Key, item.Value);
        }
        
        */

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public IEnumerator<KeyValuePair<String, Object>> GetEnumerator()
        {
            return this.parameters.GetEnumerator();
        }
    }


}
