using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Data
{
    public enum DatasetType
    {
        Unknown = 0,
        Training = 1,
        Validation = 2,
        Testing = 3
    }

    public static class DatasetTypeExternsions
    {
        public static string ToSymbol(this DatasetType datasetType)
        {
            switch (datasetType)
            {
                case DatasetType.Unknown:
                    return "Unknown";

                case DatasetType.Training:
                    return "Training";

                case DatasetType.Validation:
                    return "Validation";

                case DatasetType.Testing:
                    return "Testing";

                default:
                    throw new NotImplementedException("Dataset type not implemented");
            }
        }
    }
}
