using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Experimenter.Parms
{
    public interface IParameter : IEnumerable, IEnumerator, ICloneable //, ICollection
    {
        Type Type { get; }
        string Name { get; }
        //object GetValue(int idx);
        bool InRange(object value);
    }

    public interface IParameter<T> : IParameter
    {
        //new T GetValue(int idx);
        //new bool InRange(T value);
    }
}
