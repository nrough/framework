using System;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    public interface IReductGenerator
    {                        
        decimal Epsilon { get; set; }
        void InitFromArgs(Args args);
        void Generate();
        IReductStore ReductPool { get; }
        IReductStoreCollection GetReductStoreCollection(int numberOfEnsembles = Int32.MaxValue);
        IReduct CreateReduct(int[] permutation, decimal epsilon, decimal[] weights, IReductStore reductStore = null);
    }
}
