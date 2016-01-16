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
                decimal w = Decimal.Divide(Decimal.One, this.DataStore.NumberOfRecords);
                for (int i = 0; i < this.DataStore.NumberOfRecords; i++)
                    this.w[i] = w;
                this.isFirst = false;
            }                       
        }
    }
}
