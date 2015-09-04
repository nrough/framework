using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;

namespace Infovision.Datamining.Roughset
{
    public class WeightBoostingGenerator : WeightGenerator
    {
        public WeightBoostingGenerator(DataStore dataStore)
            : base(dataStore)
        {
        }

        public override void Generate()
        {
            throw new NotImplementedException("WeightBoostingGenerator.Generate() is not implemented");

            //TODO
            //First Weights are equal to Majority Weights
            //Each next Generate() call needs to adjust weights to current ensemble
        }
    }
}
