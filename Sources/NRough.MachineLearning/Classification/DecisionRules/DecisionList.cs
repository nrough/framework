// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

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
