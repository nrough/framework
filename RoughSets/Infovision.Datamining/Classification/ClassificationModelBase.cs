﻿using Infovision.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Classification
{
    public abstract class ClassificationModelBase : ModelBase
    {
        public AttributeAndDataSelectionMethod PreLearn { get; set; }

        public ClassificationModelBase()
            : base()
        {
        }

        public ClassificationModelBase(string modelName)
            : base(modelName)
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="attributes"></param>
    /// <param name="trainingData"></param>
    /// <param name="validationData"></param>
    /// <returns></returns>
    public delegate Tuple<int[], DataStore, DataStore> AttributeAndDataSelectionMethod(
        int[] attributes, DataStore trainingData, DataStore validationData);
}
