using System;
using System.Collections;
using System.Collections.Generic;

namespace Infovision.Test
{
    [Serializable]
    public class ParameterExclusion 
        : IEnumerable<KeyValuePair<String, Object>>
    {
        private List<KeyValuePair<String, Object>> excludedValues;

        public ParameterExclusion()
        {
            this.excludedValues = new List<KeyValuePair<String, Object>>();
        }

        public ParameterExclusion(String[] parameterNames, Object[] parameterValues)
        {
            if (parameterNames.Length != parameterValues.Length)
            {
                throw new ArgumentException("parameterValues array must have the same lengh as parameterIndexes array", "parameterValues");
            }

            this.excludedValues = new List<KeyValuePair<String, Object>>(parameterNames.Length);

            for (Int32 i = 0; i < parameterNames.Length; i++)
            {
                this.AddExclusion(parameterNames[i], parameterValues[i]);
            }
        }

        public Int32 Count
        {
            get { return this.excludedValues.Count; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) GetEnumerator();
        }

        public IEnumerator<KeyValuePair<String, Object>> GetEnumerator()
        {
            return this.excludedValues.GetEnumerator();
        }
       
        public String this[Int32 index]
        {
            get { return this.excludedValues[index].Key; }
        }

        public void AddExclusion(String parameterName, Object parameterValue)
        {
            if (String.IsNullOrEmpty(parameterName))
            {
                throw new ArgumentException("parameterName must be greater than zero", "parameterName");
            }

            KeyValuePair<String, Object> kvp = new KeyValuePair<String, Object>(parameterName, parameterValue);
            this.excludedValues.Add(kvp);
        }
    }
}
