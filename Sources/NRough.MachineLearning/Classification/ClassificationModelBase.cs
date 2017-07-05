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

using NRough.Data;
using NRough.Doc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Classification
{
    public abstract class ClassificationModelBase : ModelBase
    {
        public long? DefaultOutput { get; set; } = null;

        public OnTrainingDataSubmission OnTrainingDataSubmission { get; set; }
        public OnInputAttributeSubmission OnInputAttributeSubmission { get; set; }
        public OnValidationDataSubmission OnValidationDataSubmission { get; set; }
        
        public ClassificationModelBase()
            : base()
        {
        }

        public ClassificationModelBase(string modelName)
            : base(modelName)
        {
        }

        public virtual void SetClassificationResultParameters(ClassificationResult result)
        {
            result.ModelName = ModelName;
        }
    }

    [AssemblyTreeVisible(false)]
    public delegate DataStore OnTrainingDataSubmission(
        IModel model, int[] attributes, DataStore trainingData);

    [AssemblyTreeVisible(false)]
    public delegate int[] OnInputAttributeSubmission(
        IModel model, int[] attributes, DataStore trainingData);

    [AssemblyTreeVisible(false)]
    public delegate DataStore OnValidationDataSubmission(
        IModel model, int[] attributes, DataStore trainingData);
}
