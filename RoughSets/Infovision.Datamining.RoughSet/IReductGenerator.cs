﻿using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    public interface IReductGenerator
    {                        
        double Epsilon { get; set; }        
        void InitFromArgs(Args args);
        void Generate();
        IReductStore ReductPool { get; }        
        IReductStoreCollection GetReductStoreCollection(int numberOfEnsembles);
        IReduct CreateReduct(Permutation permutation);
    }
}
