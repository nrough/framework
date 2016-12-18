﻿using System;

namespace Infovision.MachineLearning
{        
    public abstract class ModelBase : IModel, ICloneable
    {
        public string ModelName { get; set; }        

        public ModelBase()
        {
            this.ModelName = this.GetType().Name;
        }

        public ModelBase(string modelName)
        {
            if (String.IsNullOrEmpty(modelName))
                throw new ArgumentNullException("modelName", "String.IsNullOrEmpty(modelName) == true");
            this.ModelName = modelName;
        }

        public object Clone()
        {
            var clone = (ModelBase)this.MemberwiseClone();
            this.HandleCloned(clone);
            return clone;
        }

        protected virtual void HandleCloned(ModelBase clone)
        {
        }
    }    
}
