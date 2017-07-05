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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRough.Core;
using NRough.Core.CollectionExtensions;

namespace NRough.MachineLearning.Classification.DecisionRules
{
    [Serializable]
    public class DecisionListCollection : IEnumerable<DecisionList>
    {
        private List<DecisionList> decisionLists;

        public long DefaultDecision { get; set; }

        public DecisionListCollection()
        {
            this.decisionLists = new List<DecisionList>();
            this.DefaultDecision = -1;
        }

        public DecisionListCollection(int capacity)
        {
            this.decisionLists = new List<DecisionList>(capacity);
            this.DefaultDecision = -1;
        }

        public DecisionListCollection(IEnumerable<DecisionList> decisionLists)
        {
            this.decisionLists = new List<DecisionList>(decisionLists);
            this.DefaultDecision = -1;
        }

        public DecisionListCollection(DecisionList decisionList)
            : this(1)
        {
            this.Add(decisionList);
        }

        public void Add(DecisionList decisionList)
        {
            this.decisionLists.Add(decisionList);
        }

        public void Sort()
        {
            //sort descending according to accuracy
            this.decisionLists.Sort((list1, list2) => list2.Accuracy.CompareTo(list1.Accuracy));
        }

        public void Shuffle()
        {
            //sort descending according to accuracy
            this.decisionLists.Shuffle();
        }

        public IEnumerator<DecisionList> GetEnumerator()
        {
            return this.decisionLists.GetEnumerator(); 
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.decisionLists.GetEnumerator();
        }
    }
}
