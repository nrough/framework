using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRough.Core;

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
