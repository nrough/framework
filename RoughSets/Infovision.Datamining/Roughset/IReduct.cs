using System;
using Raccoon.Data;
using Raccoon.Core;
using System.Collections.Generic;

namespace Raccoon.MachineLearning.Roughset
{
    public interface IReduct : ICloneable, IComparable
    {        
        DataStore DataStore { get; }
        HashSet<int> Attributes { get; }
        bool IsException { get; }
        
        //TODO Move to Bireduct Interface? (Exceptions?)
        //TODO This should be based on Eqialence Class Collection
        HashSet<int> SupportedObjects { get; }
        
        double[] Weights { get; }

        double Epsilon { get; }
        string Id { get; set; }

        EquivalenceClassCollection EquivalenceClasses { get; }
        bool IsEquivalenceClassCollectionCalculated { get; }

        bool AddAttribute(int attributeId);                
        bool TryRemoveAttribute(int attributeId);
        
        void SetEquivalenceClassCollection(EquivalenceClassCollection equivalenceClasses);

        int GetHashCode();
        bool Equals(object obj);
    }
}