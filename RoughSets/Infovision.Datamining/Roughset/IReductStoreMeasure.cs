﻿namespace Infovision.MachineLearning.Roughset
{
    public interface IReductStoreMeasure : IFactoryProduct
    {
        SortDirection SortDirection { get; }

        double Calc(IReductStore reductStore);
    }
}