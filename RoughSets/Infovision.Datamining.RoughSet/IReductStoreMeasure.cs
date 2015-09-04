using System;

namespace Infovision.Datamining.Roughset
{
    public interface IReductStoreMeasure : IFactoryProduct
    {
        SortDirection SortDirection { get; }
        Double Calc(IReductStore reductStore);
    }
}
