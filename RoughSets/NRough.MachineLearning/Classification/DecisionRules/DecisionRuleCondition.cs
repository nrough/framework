using NRough.Data;
using NRough.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRough.Core.Helpers;

namespace NRough.MachineLearning.Classification.DecisionRules
{
    public class DecisionRuleCondition
    {
        public int Attribute { get; set; }
        public ComparisonType Comparison { get; set; }
        public long Value { get; set; }

        public DecisionRuleCondition(int attributeId, ComparisonType comparison, long value)
        {
            this.Attribute = attributeId;
            this.Comparison = comparison;
            this.Value = value;
        }

        public bool Compute(long value)
        {
            switch (this.Comparison)
            {
                case ComparisonType.EqualTo:
                    return (this.Value == value);

                case ComparisonType.NotEqualTo:
                    return (this.Value != value);

                case ComparisonType.LessThan:
                    return (value < this.Value);

                case ComparisonType.LessThanOrEqualTo:
                    return (value <= this.Value);

                case ComparisonType.GreaterThan:
                    return (value > this.Value);

                case ComparisonType.GreaterThanOrEqualTo:
                    return (value >= this.Value);

                default:
                    throw new NotImplementedException("Comparison type not implemented");
            }
        }

        public override string ToString()
        {
            return this.ToString(null);
        }

        public string ToString(DataStoreInfo info)
        {            
            return string.Format("([{0}] {1} {2})",                    
                (info != null) ? info.GetFieldInfo(this.Attribute).Name : this.Attribute.ToString(),
                this.Comparison.ToSymbol(),
                (info != null) ? info.GetFieldInfo(this.Attribute).Internal2External(this.Value).ToString() : this.Value.ToString());            
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            DecisionRuleCondition node = obj as DecisionRuleCondition;
            if (node == null)
                return false;

            return node.Attribute == this.Attribute && node.Value == this.Value && this.Comparison == this.Comparison;
        }

        public override int GetHashCode()
        {
            return HashHelper.GetHashCode(this.Attribute, this.Comparison, this.Value);
        }
    }
}
