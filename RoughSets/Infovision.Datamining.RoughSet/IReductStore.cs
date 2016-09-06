using System.Collections.Generic;

namespace Infovision.Datamining.Roughset
{
    public interface IReductStore : IEnumerable<IReduct>
    {
        int Count { get; }
        double Weight { get; set; }
        bool AllowDuplicates { get; set; }
        bool IsActive { get; set; }

        void AddReduct(IReduct reduct);

        IReduct GetReduct(int index);

        bool IsSuperSet(IReduct reduct);

        IReductStore FilterReducts(int numberOfReducts, IComparer<IReduct> comparer);

        double GetAvgMeasure(IReductMeasure reductMeasure);

        double GetWeightedAvgMeasure(IReductMeasure reductMeasure, bool includeExceptions);

        void GetMeanStdDev(IReductMeasure reductMeasure, out double mean, out double stdDev);

        void GetMeanAveDev(IReductMeasure reductMeasure, out double mean, out double aveDev);

        bool Exist(IReduct reduct);

        void Save(string fileName);
    }
}