using System.Collections.Generic;

namespace Infovision.Data
{
    public interface IObjectSetInfo
    {
        int NumberOfRecords { get; }
        int NumberOfDecisionValues { get; }

        int NumberOfObjectsWithDecision(long decisionValue);

        ICollection<long> GetDecisionValues();
    }
}