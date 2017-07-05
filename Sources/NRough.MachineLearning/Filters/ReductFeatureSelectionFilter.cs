// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

using NRough.Core;
using NRough.Core.CollectionExtensions;
using NRough.MachineLearning.Permutations;
using NRough.MachineLearning.Roughsets;
using NRough.MachineLearning.Roughsets.Reducts.Comparers;
using NRough.MachineLearning.Weighting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NRough.Data.Filters
{
    [Serializable]
    public class ReductFeatureSelectionFilter : FilterBase
    {
        #region Members
        private Dictionary<string, int[]> reductCache;
        private Dictionary<string, PermutationCollection> permutationCache;
        private object syncRoot = new object();
        #endregion

        #region Properties

        public string ReductFactoryKey { get; set; } = ReductTypes.ApproximateReductMajorityWeights;
        public double Epsilon { get; set; } = 0.0;
        public int NumberOfReductsToTest { get; set; } = 1;        
        public IComparer<IReduct> ReductComparer { get; set; } = ReductRuleNumberComparer.Default;
        public bool UseCache { get; set; } = true;
        public bool Greedy { get; set; }
        public PermutationCollection Permutations { get; set; }
        public bool UseExceptionRules { get; set; }

        #endregion

        #region Constructors

        public ReductFeatureSelectionFilter()
            : base()
        {
            reductCache = new Dictionary<string, int[]>();
            permutationCache = new Dictionary<string, PermutationCollection>();
        }
        #endregion

        #region Methods
        public override DataStore Apply(DataStore data)
        {
            TraceData(data, false);
            int[] attributes = GetReduct(data).Union(new int[] { data.DataStoreInfo.DecisionFieldId }).ToArray();            
            return new KeepColumns(attributes).Apply(data);
        }

        public override void Compute(DataStore data)
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
                    PermutationCollection localPermutations;
                    if (Permutations == null)
                    {
                        if (!permutationCache.TryGetValue(GetAttributePermutationCacheKey(data), out localPermutations))
                        {
                            localPermutations = new PermutationCollection(NumberOfReductsToTest, data.GetStandardFields());
                            permutationCache.Add(GetAttributePermutationCacheKey(data), localPermutations);
                            TracePermutations(localPermutations, false);
                        }
                        else
                        {
                            TracePermutations(localPermutations, true);
                        }
                    }
                    else
                    {
                        localPermutations = Permutations;
                    }


                    Args parms = new Args();
                    parms.SetParameter<DataStore>(ReductFactoryOptions.DecisionTable, data);
                    parms.SetParameter<string>(ReductFactoryOptions.ReductType, ReductFactoryKey);
                    parms.SetParameter<double>(ReductFactoryOptions.Epsilon, Epsilon);
                    parms.SetParameter<bool>(ReductFactoryOptions.UseExceptionRules, UseExceptionRules);                    
                    parms.SetParameter<PermutationCollection>(
                        ReductFactoryOptions.PermutationCollection, localPermutations);                    

                    IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
                    if (Greedy)
                    {
                        var rGen = generator as ReductGeneratorMeasure;
                        if (rGen != null)
                        {
                            rGen.UsePerformanceImprovements = false;
                            rGen.Greedy = false;
                        }
                    }

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
                    Trace.WriteLine(string.Format("Perm: {0}", perm.ToArray().ToStr()));
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
            Trace.WriteLine(data.DataStoreInfo.SelectAttributeIds().ToArray().ToStr());
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
