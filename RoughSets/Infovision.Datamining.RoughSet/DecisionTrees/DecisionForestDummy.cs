using Infovision.Data;
using Infovision.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    public class DecisionForestDummy<T> : DecisionForestReduct<T>
        where T : IDecisionTree, new()
    {
        protected bool firstReduct = true;
        protected int[][] attributes = null;
        protected int localIterationNum = 0;

        protected override IReduct CalculateReduct(DataStore data)
        {
            if (this.firstReduct == true)
            {
                if (this.PermutationCollection != null)
                {
                    bool shuffle = (this.PermutationCollection.GetAverageLength() == data.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard));

                    this.attributes = new int[this.PermutationCollection.Count][];
                    int i = 0;
                    foreach (var permutation in this.PermutationCollection)
                    {
                        this.attributes[i++] =
                            shuffle ? permutation.ToArray().RandomSubArray(RandomSingleton.Random.Next(1, permutation.Length))
                                    : permutation.ToArray();
                    }
                }
                else
                {
                    this.attributes = new int[this.Size][];
                    for (int i = 0; i < this.Size; i++)
                    {
                        this.attributes[i] = data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray();
                        int len = this.attributes[i].Length;
                        this.attributes[i] = this.attributes[i].RandomSubArray(RandomSingleton.Random.Next(1, len));
                    }
                }

                this.firstReduct = false;
            }

            IReduct reduct = new ReductWeights(data, this.attributes[this.localIterationNum], 0, data.Weights);
            this.localIterationNum++;

            return reduct;
        }

        public override double Learn(DataStore data, int[] attributes)
        {
            double result = base.Learn(data, attributes);

            this.firstReduct = true;
            this.localIterationNum = 0;

            return result;
        }
    }
}
