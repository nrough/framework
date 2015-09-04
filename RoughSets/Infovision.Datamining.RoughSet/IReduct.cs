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
        double[] Weights { get; }
        double ApproximationDegree { get; }
        string Id { get; }
        
        EquivalenceClassMap EquivalenceClassMap { get; }

        bool AddAttribute(int attributeId);               
        
        bool RemoveAttribute(int attributeId);
        bool ContainsAttribute(int attributeId);
        bool ContainsObject(int objectIndex);

        //Each implementation of Reduct must define its hash code and equal method
        int GetHashCode();
        bool Equals(object obj);
    }
}
