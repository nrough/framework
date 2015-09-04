using System;
using System.Collections.Generic;

namespace Infovision.Data
{
    public interface IObjectSetInfo
    {
        int NumberOfRecords { get; }
        int NumberOfDecisionValues { get; }
        double PriorDecisionProbability(Int64 decisionValue);
        int NumberOfObjectsWithDecision(Int64 decisionValue);
        ICollection<Int64> GetDecisionValues();
    }
}
