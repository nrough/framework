using Raccoon.Core;
using Raccoon.MachineLearning.Permutations;
using Raccoon.MachineLearning.Roughset;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Raccoon.Data.Filters
{
    [Serializable]
    public class ReductFeatureSelectionFilter : IFilter
    {
        #region Members
        private Dictionary<string, int[]> reductCache;
        private Dictionary<string, PermutationCollection> permutationCache;
        private object syncRoot = new object();
        #endregion

        #region Properties

        public string ReductFactoryKey { get; set; } = ReductFactoryKeyHelper.ApproximateReductMajorityWeights;
        public double Epsilon { get; set; } = 0.03;
        public int NumberOfReductsToTest { get; set; } = 100;        
        public IComparer<IReduct> ReductComparer { get; set; } = ReductRuleNumberComparer.Default;
        public bool UseCache { get; set; } = true;

        #endregion

        #region Constructors
        public ReductFeatureSelectionFilter()
        {
            reductCache = new Dictionary<string, int[]>();
            permutationCache = new Dictionary<string, PermutationCollection>();
        }
        #endregion

        #region Methods
        public DataStore Apply(DataStore data)
        {
            TraceData(data, false);
            int[] attributes = GetReduct(data).Union(new int[] { data.DataStoreInfo.DecisionFieldId }).ToArray();            
            return new KeepColumns(attributes).Apply(data);
        }

        public void Compute(DataStore data)
        {
            TraceData(data, true);
            GetReduct(data);            
        }

        private int[] GetReduct(DataStore data)
        {            
            lock (syncRoot)
            {
                int[] res;
                if (UseCache && reductCache.TryGetValue(GetCacheKey(data), out res))
                {
                    TraceAttributes(res, true);
                    return res;
                }

                if (this.Epsilon >= 0.0)
                {
                    PermutationCollection permutations;
                    if (!permutationCache.TryGetValue(GetAttributePermutationCacheKey(data), out permutations))
                    {
                        permutations = new PermutationCollection(NumberOfReductsToTest, data.GetStandardFields());
                        permutationCache.Add(GetAttributePermutationCacheKey(data), permutations);
                        TracePermutations(permutations, false);
                    }
                    else
                    {
                        TracePermutations(permutations, true);
                    }                                         

                    Args parms = new Args(4);
                    parms.SetParameter<DataStore>(ReductGeneratorParamHelper.TrainData, data);
                    parms.SetParameter<string>(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKey);
                    parms.SetParameter<double>(ReductGeneratorParamHelper.Epsilon, Epsilon);
                    parms.SetParameter<PermutationCollection>(
                        ReductGeneratorParamHelper.PermutationCollection, permutations);                    

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
                
                if (UseCache)
                    reductCache.Add(GetCacheKey(data), res);

                TraceAttributes(res, false);

                return res;
            }
        }

        private string GetAttributePermutationCacheKey(DataStore data)
        {
            return String.Format("{0}.{1}", data.TableId, data.Fold);
        }

        private string GetCacheKey(DataStore data)
        {            
            return String.Format("{0}.{1}.{2}", data.TableId, data.Fold, Epsilon);
        }

        [Conditional("DEBUG")]
        private void TracePermutations(PermutationCollection permutations, bool isCached)
        {
            if (isCached)
            {
                foreach (var perm in permutations)
                {
                    Trace.WriteLine(String.Format("Cached perm: {0}", perm.ToArray().ToStr()));
                }
            }
            else
            {
                foreach (var perm in permutations)
                {
                    Trace.WriteLine(String.Format("Perm: {0}", perm.ToArray().ToStr()));
                }
            }
        }

        [Conditional("DEBUG")]
        private void TraceData(DataStore data, bool compute)
        {            
            Trace.WriteLine(compute ? "Compute" : "Apply");
            Trace.WriteLine(String.Format("Epsilon : {0}", this.Epsilon));
            Trace.WriteLine(String.Format("Name : {0}", data.Name));
            Trace.WriteLine(String.Format("Fold : {0}", data.Fold.ToString()));
            Trace.WriteLine(String.Format("Id : {0}", data.TableId.ToString()));
            Trace.WriteLine(String.Format("Type : {0}", data.DatasetType.ToSymbol()));
            Trace.WriteLine(data.DataStoreInfo.GetFieldIds().ToArray().ToStr());
        }

        [Conditional("DEBUG")]
        private void TraceAttributes(int[] attributes, bool isFromCache)
        {
            if(isFromCache)
                Trace.WriteLine(String.Format("Cached subset: {0}", attributes.ToStr()));
            else
                Trace.WriteLine(String.Format("Calculated subset: {0}", attributes.ToStr()));

            // Grab the name of the calling routine:
            //string methodName = new StackTrace().GetFrame(1).GetMethod().Name;

            //Trace.WriteLine("Entering CheckState for Person:");
            //Trace.Write("\tcalled by ");
            //Trace.WriteLine(methodName);

            //Debug.Assert(lastName != null, methodName, "Last Name cannot be null");
            //Debug.Assert(lastName.Length > 0, methodName, "Last Name cannot be blank");
            //Debug.Assert(firstName != null, methodName, "First Name cannot be null");
            //Debug.Assert(firstName.Length > 0, methodName, "First Name cannot be blank");
            //Trace.WriteLine("Exiting CheckState for Person");            
        }
        #endregion
    }
}
