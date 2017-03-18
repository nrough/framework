using NRough.Core;
using NRough.Core.CollectionExtensions;
using NRough.MachineLearning.Permutations;
using NRough.MachineLearning.Roughsets;
using NRough.MachineLearning.Roughsets.Reducts.Comparers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NRough.Data.Filters
{
    [Serializable]
    public class ReductFeatureSelectionFilter_Tmp : FilterBase
    {
        #region Members        

        private object syncRoot = new object();
        private int[] reduct = null;
        private double epsilon = 0.0;

        #endregion

        #region Properties

        public string ReductFactoryKey { get; set; } = ReductTypes.ApproximateReductMajorityWeights;
        public int NumberOfReductsToTest { get; set; } = 1;
        public IComparer<IReduct> ReductComparer { get; set; } = ReductRuleNumberComparer.Default;
        public bool Greedy { get; set; }
        public PermutationCollection Permutations { get; set; }

        public double Epsilon
        {
            get { return epsilon; }
            set
            {
                if (epsilon > 1.0)
                    throw new ArgumentOutOfRangeException("epsilon > 1.0", "Epsilon");

                lock (syncRoot)
                {
                    if (epsilon != value)
                    {
                        epsilon = value;
                        reduct = null;
                    }
                }
            }
        }

        #endregion

        #region Constructors

        public ReductFeatureSelectionFilter_Tmp()
            : base()
        {
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
                if (reduct != null)
                {
                    return reduct;
                }
                else
                {
                    if (this.Epsilon >= 0.0)
                    {
                        PermutationCollection localPermutations = (Permutations == null)
                        ? new PermutationCollection(NumberOfReductsToTest, data.GetStandardFields())
                        : Permutations;

                        Args parms = new Args(4);
                        parms.SetParameter<DataStore>(ReductFactoryOptions.DecisionTable, data);
                        parms.SetParameter<string>(ReductFactoryOptions.ReductType, ReductFactoryKey);
                        parms.SetParameter<double>(ReductFactoryOptions.Epsilon, Epsilon);
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
                        reduct = bestReduct.Attributes.ToArray();
                    }
                    else
                    {
                        reduct = data.GetStandardFields().ToArray();
                    }

                    return reduct;
                }
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
            if (isFromCache)
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
