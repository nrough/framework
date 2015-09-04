using System;
using System.Collections;
using System.Collections.Generic;

namespace Infovision.Test
{
    [Serializable]
    public class ParameterExclusion 
        : IEnumerable<KeyValuePair<string, object>>
    {
        private List<KeyValuePair<string, object>> excludedValues;

        public ParameterExclusion()
        {
            this.excludedValues = new List<KeyValuePair<string, object>>();
        }

        public ParameterExclusion(string[] parameterNames, object[] parameterValues)
        {
            if (parameterNames.Length != parameterValues.Length)
            {
                throw new ArgumentException("parameterValues array must have the same lengh as parameterIndexes array", "parameterValues");
            }

            this.excludedValues = new List<KeyValuePair<string, object>>(parameterNames.Length);

            for (int i = 0; i < parameterNames.Length; i++)
            {
                this.AddExclusion(parameterNames[i], parameterValues[i]);
            }
        }

        public int Count
        {
            get { return this.excludedValues.Count; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) GetEnumerator();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return this.excludedValues.GetEnumerator();
        }

        public string this[int index]
        {
            get { return this.excludedValues[index].Key; }
        }

        public void AddExclusion(string parameterName, object parameterValue)
        {
            if (String.IsNullOrEmpty(parameterName))
            {
                throw new ArgumentException("parameterName must be greater than zero", "parameterName");
            }

            KeyValuePair<string, object> kvp = new KeyValuePair<string, Object>(parameterName, parameterValue);
            this.excludedValues.Add(kvp);
        }
    }
}
