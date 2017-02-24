using NRough.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Classification.DecisionRules
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

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (var rule in this.rules)
            {
                if (first)
                {
                    sb.AppendFormat("if {0} {1}", rule, Environment.NewLine);
                    first = false;
                }
                else
                {
                    sb.AppendFormat("else if {0} {1}", rule, Environment.NewLine);
                }
            }

            return sb.ToString();
        }

        public string ToString(DataStoreInfo info)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (var rule in this.rules)
            {
                if (first)
                {
                    sb.AppendFormat("if {0} {1}", rule.ToString(info), Environment.NewLine);
                    first = false;
                }
                else
                {
                    sb.AppendFormat("else if {0} {1}", rule.ToString(info), Environment.NewLine);
                }
            }
            sb.AppendLine(String.Format("Accuracy: {0}", this.Accuracy));

            return sb.ToString();
        }
    }
}
