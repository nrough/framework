using Infovision.Data;
using Infovision.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DecisionForestDummyRough<T> : DecisionForestDummy<T>
        where T : IDecisionTree, new()
    {
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

            PermutationCollection permutations = new PermutationCollection(
                new Permutation(this.attributes[this.localIterationNum]));
            IReduct r = new ReductWeights(data, this.attributes[this.localIterationNum], 0, data.Weights);
            decimal q = InformationMeasureWeights.Instance.Calc(r);

            Args parms = new Args(6);
            parms.SetParameter<DataStore>(ReductGeneratorParamHelper.TrainData, data);
            parms.SetParameter<string>(ReductGeneratorParamHelper.FactoryKey, this.ReductGeneratorFactory);
            parms.SetParameter<decimal>(ReductGeneratorParamHelper.Epsilon, this.Epsilon);
            parms.SetParameter<PermutationCollection>(ReductGeneratorParamHelper.PermutationCollection, permutations);
            parms.SetParameter<bool>(ReductGeneratorParamHelper.UseExceptionRules, false);
            parms.SetParameter<decimal>(ReductGeneratorParamHelper.DataSetQuality, q);

            IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
            generator.Run();

            IReductStoreCollection reductStoreCollection = generator.GetReductStoreCollection();
            IReduct reduct = reductStoreCollection.First().First();

            this.localIterationNum++;

            return reduct;
        }
    }
}
