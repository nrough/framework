﻿using System;

namespace Infovision.Datamining.Roughset
{
    public interface IReductStoreMeasure : IFactoryProduct
    {
        SortDirection SortDirection { get; }
        decimal Calc(IReductStore reductStore);
    }
}
