using Infovision.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Classification
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
    }
    
    public delegate DataStore OnTrainingDataSubmission(
        IModel model, int[] attributes, DataStore trainingData);

    public delegate int[] OnInputAttributeSubmission(
        IModel model, int[] attributes, DataStore trainingData);

    public delegate DataStore OnValidationDataSubmission(
        IModel model, int[] attributes, DataStore trainingData);
}
