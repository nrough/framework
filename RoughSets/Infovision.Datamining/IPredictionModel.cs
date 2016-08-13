using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;

namespace Infovision.Datamining
{
    public interface IPredictionModel
    {
        long Compute(DataRecordInternal record);
        ClassificationResult Classify(DataStore data, decimal[] weights = null);
    }
}
