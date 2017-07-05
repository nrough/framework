// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

using System;

namespace NRough.MachineLearning
{
    [Serializable]
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
