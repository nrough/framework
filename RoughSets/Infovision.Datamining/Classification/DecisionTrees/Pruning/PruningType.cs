using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Classification.DecisionTrees.Pruning
{
    public enum PruningType
    {
        None = 0,
        ErrorBasedPruning = 1,
        ReducedErrorPruning = 2,
    }

    public static class PruningTypeExternsions
    {
        public static string ToSymbol(this PruningType comparison)
        {
            switch (comparison)
            {
                case PruningType.None:
                    return "NONE";

                case PruningType.ErrorBasedPruning:
                    return "EBP";

                case PruningType.ReducedErrorPruning:
                    return "REP";

                default:
                    throw new NotImplementedException("Pruning type not implemented");
            }
        }
    }
}
