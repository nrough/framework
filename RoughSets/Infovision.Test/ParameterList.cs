using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Infovision.Utils;

namespace Infovision.Test
{
    [Serializable]
    public class ParameterList
        : IEnumerable<ITestParameter>
    {
        #region Globals
        
        private List<ITestParameter> parameterList = new List<ITestParameter>();
        private Dictionary<int, string> index2name = new Dictionary<int, string>();
        private Dictionary<string, int> name2index = new Dictionary<string, int>();

        private List<ParameterExclusion> excludedParameters = new List<ParameterExclusion>();
        private HashSet<string> excludedParameterNames = new HashSet<string>();

        #endregion

        #region Constructors

        public ParameterList()
        {
        }

        public ParameterList(ITestParameter[] parameters)
        {
            parameterList = new List<ITestParameter>(parameters);
            for (int i = 0; i < parameters.Length; i++)
            {
                index2name[i] = parameters[i].Name;
                name2index[parameters[i].Name] = i;
            }
        }

        public ParameterList(ITestParameter[] parameters, ParameterExclusion[] exludedParams)
            : this(parameters)
        {
            this.excludedParameters = new List<ParameterExclusion>(exludedParams);
            for (int i = 0; i < exludedParams.Length; i++)
            {
                this.AddExclusionParamName(exludedParams[i]);
            }
        }

        #endregion

        #region Properties

        public int Count
        {
            get { return this.parameterList.Count; }
        }

        public IEnumerator ParmValueEnumerator
        {
            get { return new ParameterVectorEnumerator(this); }
        }

        public ITestParameter this[int index]
        {
            get { return this.parameterList[index]; }
        }

        #endregion

        #region Methods

        public string[] GetParameterNames()
        {
            string[] parameterNames = new string[this.parameterList.Count];
            for (int i = 0; i < this.parameterList.Count; i++)
            {
                parameterNames[i] = this.parameterList[i].Name;
            }
            return parameterNames;
        }

        public void Add(ITestParameter parameter)
        {
            this.name2index[parameter.Name] = this.parameterList.Count;
            this.index2name[this.parameterList.Count] = parameter.Name;
            this.parameterList.Add(parameter);
        }

        public void AddExclusion(ParameterExclusion exclusion)
        {
            this.excludedParameters.Add(exclusion);
            this.AddExclusionParamName(exclusion);
        }

        private void AddExclusionParamName(ParameterExclusion exclusion)
        {
            for (int i = 0; i < exclusion.Count; i++)
            {
                excludedParameterNames.Add(exclusion[i]);
            }
        }

        #region IEnumberable Methods

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public IEnumerator<ITestParameter> GetEnumerator()
        {
            return this.parameterList.GetEnumerator();
        }

        public bool CheckParameterExclusion(ITestParameter parameter, ParameterValueVector valueVector)
        {
            if (this.HasExclusions(parameter))
            {
                return !this.IsExcluded(valueVector);
            }

            return true;

            //return !this.HasExclusions(parameter) && !this.IsExcluded(valueVector);
        }

        private bool HasExclusions(ITestParameter parameter)
        {
            return this.excludedParameterNames.Contains(parameter.Name);
        }

        private bool IsExcluded(ParameterValueVector valueVector)
        {
            foreach (ParameterExclusion exclusion in this.excludedParameters)
            {
                if (this.Match(exclusion, valueVector))
                    return true;
            }
            return false;
        }

        private bool Match(ParameterExclusion exclusion, ParameterValueVector valueVector)
        {
            foreach (KeyValuePair<string, object> kvp in exclusion)
            {
                int parameterIndex = this.name2index[kvp.Key];

                if (parameterIndex > valueVector.Length - 1)
                    return false;

                if (valueVector[parameterIndex] == null
                    && kvp.Value != null)
                    return false;

                if (!valueVector[parameterIndex].Equals(kvp.Value))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #endregion
    }
}
