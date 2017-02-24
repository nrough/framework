﻿using NRough.Data;
using NRough.MachineLearning.Classification;

namespace NRough.MachineLearning
{
    public interface IPredictionModel : IModel
    {
        long Compute(DataRecordInternal record);
        long? DefaultOutput { get; set; }
        void SetClassificationResultParameters(ClassificationResult result);        
    }
}