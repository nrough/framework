using Raccoon.Data;
using Raccoon.MachineLearning.Roughset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raccoon.MachineLearning.Classification.Ensembles
{
    public class ReductDecisionRules : EnsembleBase
    {
        private IList<IReduct> reducts;

        public ReductDecisionRules()
        {
            reducts = new List<IReduct>();
        }

        public ReductDecisionRules(IReduct reduct)
            : this()
        {
            reducts.Add(reduct);
        }

        public ReductDecisionRules(IEnumerable<IReduct> reductCollection)
            : this()
        {
            foreach (var reduct in reductCollection)
                reducts.Add(reduct);
        }

        public override ClassificationResult Learn(DataStore data, int[] attributes)
        {


            return Classifier.Default.Classify(this, data);
        }
    }
}
