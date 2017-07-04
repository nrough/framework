//
// NRough Framework
// http://www.nrough.net
//
// Copyright © Sebastian Widz, 2017
// authors at nrough.net

//This file is part of NRough.

//Foobar is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 3 of the License, or
//(at your option) any later version.
//
//Foobar is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Foobar; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//
using NRough.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Classification.DecisionRules
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
