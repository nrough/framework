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

    
   
}
