using System;
using Infovision.Data;

namespace Infovision.Datamining.Roughset
{
    public interface IReduct : ICloneable, IComparable
    {
        IObjectSetInfo ObjectSetInfo { get; }
        DataStore DataStore { get; }
        FieldSet Attributes { get; }        
        
        //TODO Move to Bireduct Interface
        ObjectSet ObjectSet { get; }
        
        //TODO decide should this be stored in reduct or in dataStore object
        decimal[] Weights { get; }
        decimal Epsilon { get; }
        string Id { get; }
        
        EquivalenceClassCollection EquivalenceClasses { get; }

        bool AddAttribute(int attributeId);
        bool ContainsAttribute(int attributeId);
        bool ContainsObject(int objectIndex);
        bool TryRemoveAttribute(int attributeId);        

        //TODO We want to display Reducts statistics
        //Cardinality
        //Number of objects recognizable in DS
        //Number of objects not recognizable in DS
        //Accuracy on training data

        //Each implementation of Reduct must define its hash code and equal method
        int GetHashCode();
        bool Equals(object obj);
    }
}
