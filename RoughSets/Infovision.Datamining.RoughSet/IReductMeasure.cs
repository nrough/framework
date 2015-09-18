using System;

namespace Infovision.Datamining.Roughset
{
    public interface IReductMeasure : IFactoryProduct
    {
        SortDirection SortDirection { get; }
        decimal Calc(IReduct reduct);
    }
}
