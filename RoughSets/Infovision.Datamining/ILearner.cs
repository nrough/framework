using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;

namespace Infovision.Datamining
{
    public interface ILearner
    {
        double Learn(DataStore data, int[] attributes);
    }
}
