﻿namespace Raccoon.MachineLearning.Roughset
{
    public interface IReductMeasure : IFactoryProduct
    {
        SortDirection SortDirection { get; }

        double Calc(IReduct reduct);
    }
}