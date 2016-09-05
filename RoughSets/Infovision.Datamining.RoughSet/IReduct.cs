﻿using System;
using Infovision.Data;
using Infovision.Utils;
using System.Collections.Generic;

namespace Infovision.Datamining.Roughset
{
    public interface IReduct : ICloneable, IComparable
    {
        IObjectSetInfo ObjectSetInfo { get; }
        DataStore DataStore { get; }
        HashSet<int> Attributes { get; }
        bool IsException { get; }
        
        //TODO Move to Bireduct Interface? (Exceptions?)
        //TODO This should be based on Eqialence Class Collection
        ObjectSet ObjectSet { get; }

        //TODO decide should this be stored in reduct or in data object
        decimal[] Weights { get; }

        decimal Epsilon { get; }
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