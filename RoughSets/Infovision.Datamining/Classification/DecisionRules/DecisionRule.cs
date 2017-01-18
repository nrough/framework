using Raccoon.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raccoon.MachineLearning.Classification.DecisionRules
{
    public class DecisionRule
    {
        private List<DecisionRuleCondition> conditions;

        public DecisionDistribution OutputDistribution { get; set; }
        public long Output { get { return this.OutputDistribution.Output; } }
        public double Accuracy { get; set; }

        public DecisionRule(IEnumerable<DecisionRuleCondition> conditions, DecisionDistribution outputDistribution)
        {
            this.conditions = new List<DecisionRuleCondition>(conditions);
            this.OutputDistribution = outputDistribution;            
        }

        public DecisionRule(DecisionRuleCondition condition, DecisionDistribution outputDistribution)
        {
            this.conditions = new List<DecisionRuleCondition>(1);
            this.conditions.Add(condition);
            this.OutputDistribution = outputDistribution;
        }

        public bool Compute(DataRecordInternal record)
        {            
            foreach (var condition in this.conditions)
                if (!condition.Compute(record[condition.Attribute]))
                    return false;
            return true;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (var condition in conditions)
            {
                if (first)
                {
                    sb.AppendFormat("{0}", condition);
                    first = false;
                }
                else
                {
                    sb.AppendFormat(" && {0}", condition);
                }
            }
            sb.AppendFormat(" then [d] == {0}", this.Output);
            return sb.ToString();
        }

        public string ToString(DataStoreInfo info)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (var condition in conditions)
            {
                if (first)
                {
                    sb.AppendFormat("{0}", condition.ToString(info));
                    first = false;
                }
                else
                {
                    sb.AppendFormat(" && {0}", condition.ToString(info));
                }
            }
            sb.AppendFormat(" => [{0}] == {1}", info.DecisionInfo.Name, info.DecisionInfo.Internal2External(this.Output));
            return sb.ToString();
        }
    }
}
