using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Experimenter
{
    public interface IExperimentRunnable : IEnumerator
    {
        void Run();
        object GetResult();
        string GetResultInfo();
        string GetResultHeader();
    }
}
