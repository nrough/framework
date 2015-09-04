using System;

namespace Infovision.Datamining.Roughset
{
    public interface IReductStoreMeasure : IFactoryProduct
    {
        SortDirection SortDirection { get; }
        double Calc(IReductStore reductStore);
    }
}
