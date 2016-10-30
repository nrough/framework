using Infovision.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionRules
{
    public class DecisionList : IEnumerable<DecisionRule>
    {
        private List<DecisionRule> rules;

        public double Accuracy { get; set; }

        public DecisionList()
        {
            this.rules = new List<DecisionRule>();
        }

        public DecisionList(int capacity)
        {
            this.rules = new List<DecisionRule>(capacity);
        }

        public DecisionList(IEnumerable<DecisionRule> rules)
        {
            this.rules = new List<DecisionRule>(rules);
        }

        public DecisionList(DecisionRule rule)
            : this(1)
        {            
            this.Add(rule);
        }

        public void Add(DecisionRule rule)
        {
            this.rules.Add(rule);
        }

        public long Compute(DataRecordInternal record)
        {
            foreach (var rule in rules)
                if (rule.Compute(record))
                    return rule.Output;
            return -1;
        }

        public IEnumerator<DecisionRule> GetEnumerator()
        {
            return this.rules.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.rules.GetEnumerator();
        }
    }
}
