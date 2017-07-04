//
// NRough Framework
// http://www.nrough.net
//
// Copyright © Sebastian Widz, 2017
// authors at nrough.net

//This file is part of NRough.

//Foobar is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 3 of the License, or
//(at your option) any later version.
//
//Foobar is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Foobar; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//
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