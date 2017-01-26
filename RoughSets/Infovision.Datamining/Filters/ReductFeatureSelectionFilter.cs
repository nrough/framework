using Raccoon.Core;
using Raccoon.MachineLearning.Permutations;
using Raccoon.MachineLearning.Roughset;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Raccoon.Data.Filters
{
    [Serializable]
    public class ReductFeatureSelectionFilter : IFilter
    {
        #region Members
        private Dictionary<string, int[]> reductCache = new Dictionary<string, int[]>();
        private object syncRoot = new object();
        #endregion

        #region Properties

        public string ReductFactoryKey { get; set; } = ReductFactoryKeyHelper.ApproximateReductMajorityWeights;
        public double Epsilon { get; set; } = 0.03;
        public int NumberOfReductsToTest { get; set; } = 100;
        public PermutationCollection AttributePermutations { get; set; } = null;
        public IComparer<IReduct> ReductComparer { get; set; } = ReductRuleNumberComparer.Default;
        public bool UseCache { get; set; } = true;

        #endregion

        #region Constructors
        public ReductFeatureSelectionFilter()
        {
        }
        #endregion

        #region Methods
        public DataStore Apply(DataStore data)
        {            
            return new KeepColumns(GetReduct(data)).Apply(data);
        }

        public void Compute(DataStore data)
        {
            GetReduct(data);
        }

        private int[] GetReduct(DataStore data)
        {            
            lock (syncRoot)
            {
                int[] res = null;
                if (UseCache && reductCache.TryGetValue(GetCacheKey(data), out res))                                     
                    return res;

                if (this.Epsilon >= 0.0)
                {
                    if (AttributePermutations != null)
                        AttributePermutations = new PermutationCollection(
                            NumberOfReductsToTest, data.GetStandardFields());

                    Args parms = new Args(4);
                    parms.SetParameter<DataStore>(ReductGeneratorParamHelper.TrainData, data);
                    parms.SetParameter<string>(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKey);
                    parms.SetParameter<double>(ReductGeneratorParamHelper.Epsilon, Epsilon);
                    parms.SetParameter<PermutationCollection>(
                        ReductGeneratorParamHelper.PermutationCollection, AttributePermutations);

                    IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
                    generator.Run();

                    var reducts = generator.GetReducts();
                    reducts.Sort(ReductComparer);

                    IReduct bestReduct = reducts.FirstOrDefault();
                    res = bestReduct.Attributes.ToArray();
                }
                else
                {
                    res = data.GetStandardFields().ToArray();
                }

                if (data.DataStoreInfo.DecisionFieldId != 0)
                    res = res.Union(new int[] { data.DataStoreInfo.DecisionFieldId }).ToArray();
                
                if (UseCache)
                    reductCache.Add(GetCacheKey(data), res);

                return res;
            }
        }

        private string GetCacheKey(DataStore data)
        {
            //TODO Should we use some kind of data HashKey?
            return String.Format("{0}.{1}.{2}.{3}.{4}", 
                data.Name, data.Fold, data.NumberOfRecords, data.DataStoreInfo.NumberOfFields, Epsilon);
        }
        #endregion
    }
}
