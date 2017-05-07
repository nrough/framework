using System;
using System.Collections;

namespace NRough.MachineLearning.Experimenter.Parms
{
    [Serializable]
    public class ParametersValueEnumerator : IEnumerator
    {
        #region Globals

        private bool isFirst;
        private ParameterCollection parameters;
        private object[] currentValues;
        private int lastIdx;

        #endregion Globals

        #region Constructors

        public ParametersValueEnumerator(ParameterCollection parameters)
        {
            this.parameters = (ParameterCollection)parameters.Clone();
            currentValues = new object[parameters.Count];
            lastIdx = parameters.Count - 1;

            this.Reset();
        }

        #endregion Constructors

        #region Methods

        public object Current
        {
            get { return currentValues; }
        }

        public void Reset()
        {
            isFirst = true;
        }

        public bool MoveNext()
        {
            if (isFirst)
            {
                isFirst = false;
                for (int i = 0; i < parameters.Count; i++)
                {
                    parameters[i].Reset();
                    if (i != lastIdx)
                        parameters[i].MoveNext();
                }
            }

            while (true)
            {
                int last = lastIdx;
                bool shift = false;
                while (last >= 0)
                {
                    if (parameters[last].MoveNext())
                    {
                        break;
                    }
                    else
                    {
                        last--;
                        shift = true;
                    }
                }

                if (last < 0)
                    return false;

                if (shift)
                {
                    for (int i = last + 1; i < this.parameters.Count; i++)
                    {
                        parameters[i].Reset();
                        parameters[i].MoveNext();
                    }
                }

                if (!parameters.IsCurrentValueExcluded())
                    break;
            }

            for (int i = 0; i < this.parameters.Count; i++)
                currentValues[i] = this.parameters[i].Current;

            return true;
        }

        #endregion Methods
    }
}