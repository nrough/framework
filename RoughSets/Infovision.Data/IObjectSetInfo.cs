using System;
using System.Collections.Generic;

namespace Infovision.Data
{
    public interface IObjectSetInfo
    {
        int NumberOfRecords { get; }
        int NumberOfDecisionValues { get; }
        double PriorDecisionProbability(long decisionValue);
        int NumberOfObjectsWithDecision(long decisionValue);
        ICollection<Int64> GetDecisionValues();
    }
}
