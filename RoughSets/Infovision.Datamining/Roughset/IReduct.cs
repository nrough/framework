using System;
using Infovision.Data;
using Infovision.Core;
using System.Collections.Generic;

namespace Infovision.MachineLearning.Roughset
{
    public interface IReduct : ICloneable, IComparable
    {        
        DataStore DataStore { get; }
        HashSet<int> Attributes { get; }
        bool IsException { get; }
        
        //TODO Move to Bireduct Interface? (Exceptions?)
        //TODO This should be based on Eqialence Class Collection
        HashSet<int> ObjectSet { get; }

        //TODO decide should this be stored in reduct or in data object
        double[] Weights { get; }

        double Epsilon { get; }
        string Id { get; set; }

        EquivalenceClassCollection EquivalenceClasses { get; }
        bool IsEquivalenceClassCollectionCalculated { get; }

        bool AddAttribute(int attributeId);                
        bool TryRemoveAttribute(int attributeId);
        
        void SetEquivalenceClassCollection(EquivalenceClassCollection equivalenceClasses);

        //Each implementation of Reduct must define its hash code and equal method
        int GetHashCode();
        bool Equals(object obj);
    }
}