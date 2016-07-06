using System;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    public interface IReductGenerator
    {
        decimal Epsilon { get; set; }
        long ReductGenerationTime { get; }

        void InitFromArgs(Args args);

        void Run();

        IReductStore ReductPool { get; }

        IReductStoreCollection GetReductStoreCollection(int numberOfEnsembles = Int32.MaxValue);

        IReduct CreateReduct(int[] permutation, decimal epsilon, decimal[] weights, IReductStore reductStore = null, IReductStoreCollection reductStoreCollection = null);
    }
}