using System;
using Infovision.Data;

namespace Infovision.Datamining.Roughset
{
    public interface IReduct : ICloneable
    {
        IObjectSetInfo ObjectSetInfo { get; }
        DataStore DataStore { get; }
        FieldSet AttributeSet { get; }
        //List<int> AttributeOrder { get; }
        
        //TODO Move to Bireduct Interface
        ObjectSet ObjectSet { get; }
        double[] Weights { get; }

        //TODO Move to Reduct store
        EquivalenceClassMap EquivalenceClassMap { get; }

        bool AddAttribute(int attributeId);
        ////TODO Interface should not use position as not all objects will implement it?
        //bool AddAttribute(int attributeId, int position);
        
        bool RemoveAttribute(int attributeId);
        bool ContainsAttribute(int attributeId);
        bool ContainsObject(int objectIndex);

        //Each implementation of Reduct must define its hash code and equal method
        int GetHashCode();
        bool Equals(object obj);
    }
}
