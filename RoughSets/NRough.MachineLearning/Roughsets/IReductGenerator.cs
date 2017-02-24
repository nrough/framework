﻿using System;
using NRough.Core;

namespace NRough.MachineLearning.Roughsets
{
    public interface IReductGenerator
    {
        double Epsilon { get; set; }
        long ReductGenerationTime { get; }

        void InitFromArgs(Args args);

        void Run();

        IReductStore ReductPool { get; }

        IReductStoreCollection GetReductStoreCollection(int numberOfEnsembles = Int32.MaxValue);

        IReduct CreateReduct(int[] permutation, double epsilon, double[] weights, IReductStore reductStore = null, IReductStoreCollection reductStoreCollection = null);
    }
}