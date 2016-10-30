using Infovision.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionRules
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
    }
}
