using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infovision.Utils;

namespace Infovision.Test
{
    [Serializable]
    public class ParameterVectorEnumerator : IEnumerator
    {
        #region Globals

        private List<ParameterValueVector> valueVectorList;
        private IEnumerator<ParameterValueVector> valueVectorEnumerator;

        #endregion

        #region Constructors

        public ParameterVectorEnumerator(ParameterCollection parameterList)
        {

            this.InitValueVector(parameterList);

            this.valueVectorEnumerator = valueVectorList.GetEnumerator();
            this.valueVectorEnumerator.Reset();

        }

        #endregion

        #region Properties

        public object Current
        {
            get { return valueVectorEnumerator.Current; }
        }

        #endregion

        #region Methods

        private void InitValueVector(ParameterCollection parameterList)
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
                            ParameterValueVector newValueVector = (ParameterValueVector)existingVector.Clone();
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
        }

        public bool MoveNext()
        {
            return valueVectorEnumerator.MoveNext();            
        }

        #endregion
        #endregion
    }
}
