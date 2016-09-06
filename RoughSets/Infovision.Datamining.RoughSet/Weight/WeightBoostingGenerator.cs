using System;
using Infovision.Data;

namespace Infovision.Datamining.Roughset
{
    public class WeightBoostingGenerator : WeightGenerator
    {
        private bool isFirst;

        public WeightBoostingGenerator(DataStore dataStore)
            : base(dataStore)
        {
            this.isFirst = true;
        }

        public override void Generate()
        {
            if (this.isFirst)
            {
                double w = 1.0 / this.DataStore.NumberOfRecords;
                for (int i = 0; i < this.DataStore.NumberOfRecords; i++)
                    this.w[i] = w;
                this.isFirst = false;
            }
        }
    }
}