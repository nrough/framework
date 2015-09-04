using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Infovision.Utils;

namespace Infovision.Test
{
    [Serializable]
    public class ParameterValueVector : ICloneable
    {
        #region Globals
        
        private object[] valueVector;
        
        #endregion

        #region Constructors

        public ParameterValueVector(int size)
        {
            this.valueVector = new object[size];
        }

        public ParameterValueVector(object[] inititalData)
        {
            this.valueVector = new object[inititalData.Length];
            for (int i = 0; i < inititalData.Length; i++)
            {
                this.valueVector[i] = inititalData[i];
            }
        }

        public ParameterValueVector(ParameterValueVector parameterValueVector)
            : this(parameterValueVector.GetArray())
        {
        }

        #endregion

        #region Properties

        public object this[int index]
        {
            get { return valueVector[index]; }
            set { valueVector[index] = value; }
        }

        public int Length
        {
            get { return valueVector.Length; }
        }

        #endregion

        #region Methods

        public object[] GetArray()
        {
            return this.valueVector;
        }

        #region System.Object Methods

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (object element in valueVector)
            {
                stringBuilder.Append(element.ToString());
                stringBuilder.Append(' ');
            }
            return stringBuilder.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            ParameterValueVector p = obj as ParameterValueVector;
            if ((System.Object)p == null)
                return false;

            if (p.Length != this.Length)
                return false;

            for (int i = 0; i < this.Length; i++)
                if (!this[i].Equals(p[i]))
                    return false;

            return true;
        }

        public override int GetHashCode()
        {
            return HashHelper.GetHashCode<object>(this.valueVector);
        }

        #endregion

        #region ICloneable Members
        /// <summary>
        /// Clones the ParameterValueVector, performing a deep copy.
        /// </summary>
        /// <returns>A new instance of a ParameterValueVector, using a deep copy.</returns>
        public virtual object Clone()
        {
            return new ParameterValueVector(this);
        }
        #endregion

        #endregion
    }

    [Serializable]
    public class ParameterVectorEnumerator 
        : IEnumerator
    {
        #region Globals

        private List<ParameterValueVector> valueVectorList;
        private IEnumerator<ParameterValueVector> valueVectorEnumerator;

        /*
        private int[] valueIndex;
        private object[][] parmValues;
        private ParameterValueVector currentParmVector = null;
        private int position;
        */

        #endregion

        #region Constructors

        public ParameterVectorEnumerator(ParameterList parameterList)
        {
            /*
            valueIndex = new int[parameterList.Count];
            parmValues = new object[parameterList.Count][];

            for (int i = 0; i < parameterList.Count; i++)
            {
                valueIndex[i] = 0;
                
                int valueCount = parameterList[i].Count;
                parmValues[i] = new object[valueCount];

                parameterList[i].Reset();
                j = 0;
                foreach (var parameterValue in parameterList[i])
                {
                    parmValues[i][j] = parameterValue;
                    j++;
                }
            }
            */
            
            this.InitValueVector(parameterList);
            
            this.valueVectorEnumerator = valueVectorList.GetEnumerator();
            this.valueVectorEnumerator.Reset();
            
        }

        #endregion

        #region Properties

        public object Current
        {
            get { return valueVectorEnumerator.Current; }
            //get { return this.currentParmVector; }
        }

        #endregion

        #region Methods

        private void InitValueVector(ParameterList parameterList)
        {
            this.valueVectorList = new List<ParameterValueVector>();           
            
            int parameterIndex = -1;
            foreach (ITestParameter parameter in parameterList)
            {
                List<ParameterValueVector> localValueVectorList = new List<ParameterValueVector>();
                parameterIndex++;
                if (parameterIndex == 0)
                {
                    parameter.Reset();
                    foreach (var parameterValue in parameter)
                    {
                        ParameterValueVector newValueVector = new ParameterValueVector(parameterList.Count);

                        newValueVector[parameterIndex] = parameterValue;
                        
                        if (parameterList.CheckParameterExclusion(parameter, newValueVector))
                        {
                            localValueVectorList.Add(newValueVector);
                        }
                    }
                }
                else
                {
                    foreach (ParameterValueVector existingVector in valueVectorList)
                    {
                        parameter.Reset();
                        foreach (var parameterValue in parameter)
                        {
                            ParameterValueVector newValueVector = (ParameterValueVector) existingVector.Clone();
                            newValueVector[parameterIndex] = parameterValue;
                            
                            if (parameterList.CheckParameterExclusion(parameter, newValueVector))
                            {
                                localValueVectorList.Add(newValueVector);
                            }
                        }
                    }
                }

                this.valueVectorList = (List<ParameterValueVector>)localValueVectorList.Clone();
            }
        }
        
        #region IEnumerator Members

        public void Reset()
        {
            valueVectorEnumerator.Reset();

            /*
            for (int i = 0; i < valueIndex.Length; i++)
            {
                valueIndex[i] = 0;
            }
            */
        }

        public bool MoveNext()
        {

            /*
            this.currentParmVector = new ParameterValueVector(this.valueIndex.Length);

            for(int i=0; i<this.valueIndex.Length - 1; i++)
            {
                if (this.valueIndex[i + 1] < this.parmValues[i].Length)
                {
                    this.currentParmVector[i] = this.parmValues[i][]
                }
            }
            */

            return valueVectorEnumerator.MoveNext();
            //return true;            
        }

        #endregion
        #endregion
    }

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
            for (Int32 i = 0; i < parameters.Length; i++)
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
