﻿using System;
using System.Collections;

namespace NRough.MachineLearning.Experimenter.Parms
{
    public interface IParameter : IEnumerable, IEnumerator, ICloneable
    {
        Type Type { get; }
        string Name { get; }

        bool InRange(object value);
    }

    public interface IParameter<T> : IParameter
    {
    }
}