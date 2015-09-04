using System.Collections.Generic;

namespace Infovision.Datamining.Roughset
{
    public interface IReductStore : IEnumerable<IReduct>
    {
        int Count { get; }
        double Weight { get; set; }

        void AddReduct(IReduct reduct);
        IReduct GetReduct(int index);
        
        bool IsSuperSet(IReduct reduct);
        
        IReductStore FilterReducts(int numberOfReducts, IComparer<IReduct> comparer);
        double GetAvgMeasure(IReductMeasure reductMeasure);

        bool Exist(IReduct reduct);

        void Save(string fileName);
    }
}
