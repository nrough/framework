﻿using System;
using System.Collections.Generic;
using System.Linq;
using NRough.Data;
using NRough.MachineLearning.Clustering.Hierarchical;
using NRough.Math;
using NRough.Core;
using NRough.MachineLearning.Permutations;

namespace NRough.MachineLearning.Roughsets
{
    public class ReductEnsembleBoostingWithDiversityGenerator : ReductEnsembleBoostingGenerator
    {
        private Dictionary<string, int> clusterInstances;
        private List<double[]> clusterInstances2;

        public Func<double[], double[], double> Distance { get; set; }
        public Func<int[], int[], DistanceMatrix, double[][], double> Linkage { get; set; }
        public Func<IReduct, double[], RuleQualityMethod, double[]> ReconWeights { get; set; }
        public int NumberOfReductsToTest { get; set; }
        public AgregateFunction AgregateFunction { get; set; }

        public ReductEnsembleBoostingWithDiversityGenerator()
            : base()
        {
            clusterInstances = new Dictionary<string, int>();
            clusterInstances2 = new List<double[]>();
        }

        public ReductEnsembleBoostingWithDiversityGenerator(DataStore data)
            : base(data)
        {
            clusterInstances = new Dictionary<string, int>();
            clusterInstances2 = new List<double[]>();
        }

        public override void InitDefaultParameters()
        {
            base.InitDefaultParameters();

            this.Distance = Math.Distance.Manhattan;
            this.Linkage = ClusteringLinkage.Average;
            this.ReconWeights = ReductToVectorConversionMethods.GetErrorReconWeights;
            this.NumberOfReductsToTest = 10;
            this.AgregateFunction = AgregateFunction.Max;
        }

        public override void InitFromArgs(Args args)
        {
            base.InitFromArgs(args);

            if (args.Exist(ReductFactoryOptions.Distance))
                this.Distance = (Func<double[], double[], double>)args.GetParameter(ReductFactoryOptions.Distance);

            if (args.Exist(ReductFactoryOptions.Linkage))
                this.Linkage = (Func<int[], int[], DistanceMatrix, double[][], double>)args.GetParameter(ReductFactoryOptions.Linkage);

            if (args.Exist(ReductFactoryOptions.ReconWeights))
                this.ReconWeights = (Func<IReduct, double[], RuleQualityMethod, double[]>)args.GetParameter(ReductFactoryOptions.ReconWeights);

            if (args.Exist(ReductFactoryOptions.NumberOfReductsToTest))
                this.NumberOfReductsToTest = (int)args.GetParameter(ReductFactoryOptions.NumberOfReductsToTest);

            if (args.Exist(ReductFactoryOptions.AgregateFunction))
                this.AgregateFunction = (AgregateFunction)args.GetParameter(ReductFactoryOptions.AgregateFunction);
        }

        public override IReduct GetNextReduct(double[] weights)
        {
            if (clusterInstances2.Count == 0 || this.NumberOfReductsToTest == 1)
                return base.GetNextReduct(weights);

            DistanceMatrix distanceMatrix = new DistanceMatrix(this.Distance);

            int[] cluster1 = new int[this.clusterInstances2.Count];
            for (int i = 0; i < this.clusterInstances2.Count; i++)
            {
                cluster1[i] = i;
                for (int j = i + 1; j < this.clusterInstances2.Count; j++)
                {
                    distanceMatrix[i, j] = this.Distance(this.clusterInstances2[i], this.clusterInstances2[j]);
                }
            }

            double[][] mergedInstances = this.clusterInstances2.ToArray();
            Array.Resize<double[]>(ref mergedInstances, mergedInstances.Length + this.NumberOfReductsToTest);

            IReduct[] candidates = new IReduct[this.NumberOfReductsToTest];
            double distMax = Double.MinValue;
            double distMin = Double.MaxValue;
            int maxIndex = -1;
            int minIndex = -1;
            int[] oneElementcluster = new int[1];

            for (int i = 0; i < this.NumberOfReductsToTest; i++)
            {
                candidates[i] = base.GetNextReduct(weights);
                double[] condidateVector = this.ReconWeights(candidates[i], weights, this.IdentyficationType);

                for (int j = 0; j < this.clusterInstances2.Count; j++)
                    distanceMatrix[j, this.clusterInstances2.Count + i] = this.Distance(this.clusterInstances2[j], condidateVector);

                mergedInstances[this.clusterInstances2.Count + i] = condidateVector;
                oneElementcluster[0] = this.clusterInstances2.Count + i;

                double clusterDistance = this.Linkage(this.clusterInstances.Values.ToArray(),
                                                      oneElementcluster,
                                                      distanceMatrix,
                                                      mergedInstances);
                if (distMax < clusterDistance)
                {
                    distMax = clusterDistance;
                    maxIndex = i;
                }

                if (distMin > clusterDistance)
                {
                    distMin = clusterDistance;
                    minIndex = i;
                }
            }

            switch (this.AgregateFunction)
            {
                case AgregateFunction.Min:
                    return candidates[minIndex];

                case AgregateFunction.Max:
                    return candidates[maxIndex];
            }

            return candidates[maxIndex];
        }

        protected override void AddModel(IReductStore model, double modelWeight)
        {
            base.AddModel(model, modelWeight);

            foreach (IReduct r in model)
            {
                clusterInstances.Add(r.Id, clusterInstances2.Count);
                clusterInstances2.Add(this.ReconWeights(r, r.Weights, this.IdentyficationType));
            }
        }
    }

    public class ReductEnsembleBoostingWithDiversityFactory : IReductFactory
    {
        public virtual string FactoryKey
        {
            get { return ReductTypes.ReductEnsembleBoostingWithDiversity; }
        }

        public virtual IReductGenerator GetReductGenerator(Args args)
        {
            ReductEnsembleBoostingWithDiversityGenerator rGen = new ReductEnsembleBoostingWithDiversityGenerator();
            rGen.InitFromArgs(args);
            return rGen;
        }

        public virtual IPermutationGenerator GetPermutationGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter(ReductFactoryOptions.DecisionTable);
            return new PermutationGenerator(dataStore);
        }
    }
}