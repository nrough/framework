using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Experimenter.Parms
{       
    [Serializable]
    public class ParameterCollection : IEnumerable<IParameter>, ICloneable
    {                
        #region Members

        private static double resizeFactor = 1.2;

        protected IParameter[] parameters;
        protected Dictionary<string, int> name2index;
        private int parmCount;
        protected IParameter[][] exclusions;
        protected int exclusionCount;        

        #endregion

        #region Constructors

        public ParameterCollection()
            : this(0, 0)
        {
        }

        public ParameterCollection(int size, int exclusionSize)
        {
            parmCount = 0;
            parameters = new IParameter[size];
            name2index = new Dictionary<string, int>(size);

            exclusions = new IParameter[exclusionSize][];
            for (int i = 0; i < exclusions.Length; i++)
            {
                exclusions[i] = new IParameter[size];
            }
            exclusionCount = 0;
        }

        public ParameterCollection(IParameter[] parameters)
            : this(parameters.Length, 20)
        {
            
            this.parmCount = parameters.Length;
            for (int i = 0; i < this.parameters.Length; i++)
            {
                this.parameters[i] = parameters[i];
                name2index[parameters[i].Name] = i;
            }
        }

        public ParameterCollection(ParameterCollection parameterList)
            : this(parameterList.Count, parameterList.exclusionCount)
        {            
            int i = 0;
            foreach (IParameter parm in parameterList)
            {
                this[i] = (IParameter) parm.Clone();
                name2index.Add(this[i].Name, i);
                i++;
            }
            this.parmCount = i;
            this.exclusionCount = parameterList.exclusionCount;

            for (i = 0; i < parameterList.exclusionCount; i++ )
            {
                for (int j = 0; j < parmCount; j++)
                {
                    if (parameterList.exclusions[i][j] != null)
                    {
                        exclusions[i][j] = (IParameter)parameterList.exclusions[i][j].Clone();
                    }
                }
            }                        
        }        

        #endregion

        #region Properties

        public int Count
        {
            get { return this.parmCount; }
        }        

        public IParameter this[int idx]
        {
            get { return this.parameters[idx]; }
            set { this.parameters[idx] = value; }
        }

        #endregion

        #region Methods

        public string[] GetParameterNames()
        {
            string[] parameterNames = new string[this.parmCount];
            for (int i = 0; i < this.parmCount; i++)
            {
                parameterNames[i] = this.parameters[i].Name;
            }
            return parameterNames;
        }

        public void Add(IParameter parameter)
        {
            if (this.parmCount >= parameters.Length)
            {
                int newSize = (int)((parameters.Length + 1) * resizeFactor);
                if (newSize <= parameters.Length)
                    newSize = parameters.Length + 1;

                Array.Resize<IParameter>(ref parameters, newSize);
            }
                
            this.name2index[parameter.Name] = parmCount;
            this.parameters[parmCount] = parameter;
            parmCount++;
        }

        public IEnumerable<object[]> Values()
        {
            ParametersValueEnumerator i_values = new ParametersValueEnumerator(this);
            while (i_values.MoveNext())
            {
                yield return (object[]) i_values.Current;
            }
        }

        protected IEnumerator GetValueEnumerator()
        {
            return new ParametersValueEnumerator(this);
        }

        public void AddExclusion(string[] parameterNames, IParameter[] parameterExclusion)
        {
            int i = 0;
            foreach (string parmName in parameterNames)
            {
                int idx = this.name2index[parmName];
                exclusions[exclusionCount][idx] = parameterExclusion[i];
                i++;
            }

            exclusionCount++;
        }

        internal bool IsCurrentValueExcluded()
        {
            for (int i = 0; i < exclusions.Length; i++)
            {
                int j;
                for(j = 0; j<parameters.Length; j++)
                {
                    if (exclusions[i][j] != null)
                    {
                        if (! exclusions[i][j].InRange(parameters[j].Current))
                        {
                            break;
                        }
                    }
                }

                if (j == parameters.Length)
                    return true;
            }

            return false;
        }

        internal bool IsValueVectorInRange(object[] valueVector)
        {
            for (int i = 0; i < exclusions.Length; i++)
            {
                int j;
                for (j = 0; j < parameters.Length; j++)
                {
                    if (exclusions[i][j] != null)
                    {
                        if (!exclusions[i][j].InRange(valueVector[j]))
                        {
                            break;
                        }
                    }
                }

                if (j == parameters.Length)
                    return true;
            }

            return false;
        }        

        #region IEnumberable Methods

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public IEnumerator<IParameter> GetEnumerator()
        {
            //.NET 2.0+ more efficient
            return ((IEnumerable<IParameter>)parameters).GetEnumerator();

            //.NET 3.5+
            //return parameters.Cast<IParameter>().GetEnumerator();
        }

        #endregion

        #region ICloneable methods

        public object Clone()
        {
            return new ParameterCollection(this);
        }

        //TODO
        /*
        public object Clone()
        {
            var clone = (ParameterCollection)this.MemberwiseClone();
            this.HandleCloned(clone);
            return clone;
        }

        protected virtual void HandleCloned(ParameterCollection clone)
        {
        }
        */

        #endregion

        #endregion
    }
}
