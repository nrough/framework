﻿using System;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    public interface IReduct : ICloneable, IComparable
    {
        IObjectSetInfo ObjectSetInfo { get; }
        DataStore DataStore { get; }
        PascalSet<int> Attributes { get; }
        bool IsException { get; }
        

        //TODO Move to Bireduct Interface? (Exceptions?)
        ObjectSet ObjectSet { get; }

        //TODO decide should this be stored in reduct or in data object
        decimal[] Weights { get; }

        decimal Epsilon { get; }
        string Id { get; set; }

        EquivalenceClassCollection EquivalenceClasses { get; }
        bool IsEquivalenceClassCollectionCalculated { get; }

        bool AddAttribute(int attributeId);

        bool ContainsAttribute(int attributeId);

        bool ContainsObject(int objectIndex);

        bool TryRemoveAttribute(int attributeId);

        //bool TryRemoveAttributes(int[] attributeId);

        //TODO We want to display Reducts statistics
        //Cardinality
        //Number of objects recognizable in DS
        //Number of objects not recognizable in DS
        //Accuracy on training data

        void SetEquivalenceClassCollection(EquivalenceClassCollection equivalenceClasses);

        //Each implementation of Reduct must define its hash code and equal method
        int GetHashCode();

        bool Equals(object obj);
    }
}