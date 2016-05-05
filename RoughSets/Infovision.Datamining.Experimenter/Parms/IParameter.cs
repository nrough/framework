using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Experimenter.Parms
{
    public interface IParameter : IEnumerable, IEnumerator, ICloneable
    {
        Type Type { get; }
        string Name { get; }      
        bool InRange(object value);
    }

    public interface IParameter<T> : IParameter
    {        
    }
}
