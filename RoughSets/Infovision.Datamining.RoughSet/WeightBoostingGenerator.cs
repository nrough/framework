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
                for (int i = 0; i < this.DataStore.NumberOfRecords; i++)
                    this.w[i] = 1.0 / this.DataStore.NumberOfRecords;
                this.isFirst = false;
            }                       
        }
    }
}
