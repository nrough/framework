using System;

namespace Infovision.Datamining.Roughset
{
    public interface IReductMeasure : IFactoryProduct
    {
        SortDirection SortDirection { get; }
        Double Calc(IReduct reduct);
    }
}
