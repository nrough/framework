﻿// 
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

using System;
using System.Collections;
using System.Collections.Generic;

namespace NRough.Core
{
    [Serializable]
    public class Args
        : IEnumerable<KeyValuePair<string, object>>, ICloneable
    {
        private Dictionary<string, object> parameters = null;
        private List<string> parameterOrder = null;
        private Dictionary<int, string> index2parameter = null;
        private int nextIndex = 0;

        public int Count
        {
            get { return this.parameters.Count; }
        }

        public object this[string id]
        {
            get { return this.GetParameter(id); }
            set { this.SetParameter(id, value); }
        }

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
                throw new ArgumentException("Value array must have the same length as the array of keys", "values");

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

        public void SetParameter(string key, object value)
        {
            if (this.parameters.ContainsKey(key) == false)
            {
                this.parameterOrder.Add(key);
                this.index2parameter[this.nextIndex] = key;
                this.nextIndex++;
            }

            this.parameters[key] = value;
        }

        public void SetParameter<T>(string key, T value)
        {
            if (this.parameters.ContainsKey(key) == false)
            {
                this.parameterOrder.Add(key);
                this.index2parameter[this.nextIndex] = key;
                this.nextIndex++;
            }

            this.parameters[key] = value;
        }

        public void RemoveParameter(string key)
        {
            this.parameters.Remove(key);
        }

        public object GetParameter(string key)
        {
            if (this.Exist(key))
                return this.parameters[key];

            return null;
        }

        public T GetParameter<T>(string key)
        {
            if (this.Exist(key))
                return (T)this.parameters[key];
            else
                throw new KeyNotFoundException(String.Format("Key {0} was not found.", key));
        }

        public object GetParameter(int index)
        {
            string key;
            if (this.index2parameter.TryGetValue(index, out key))
                return this.parameters[key];
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return this.parameters.GetEnumerator();
        }

        public bool SetProperty<T>(string key, ref T property)
        {
            if (this.Exist(key))
            {
                property = (T)this.GetParameter(key);
                return true;
            }
            return false;
        }

        public virtual object Clone()
        {
            Args result = new Args(this.Count);
            foreach (var parameter in this)
                result.SetParameter(parameter.Key, parameter.Value);
            return result;
        }
    }
}