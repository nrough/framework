using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Classification
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class ClassificationResultValueAttribute : Attribute
    {
        protected static string DefaultOutputFormat = "{0}";

        public string Alias { get; set; } = String.Empty;
        public string OutputFormat { get; set; } = ClassificationResultValueAttribute.DefaultOutputFormat;
        public bool DefaultOutput { get; set; } = false;

        public ClassificationResultValueAttribute()
            : base()
        {
        }

        public ClassificationResultValueAttribute(string alias, string outputFormat, bool defaultOutput)
            : this()
        {
            this.Alias = alias;
            this.OutputFormat = outputFormat;
            this.DefaultOutput = defaultOutput;
        }

        public ClassificationResultValueAttribute(string alias, string outputFormat)
            : this(alias, outputFormat, false)
        {
        }

        public ClassificationResultValueAttribute(string alias)
            : this(alias, ClassificationResultValueAttribute.DefaultOutputFormat, false)
        {
        }
    }
}
