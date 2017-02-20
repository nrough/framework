using System;
using System.Collections.Generic;
using Raccoon.Data;
using Raccoon.MachineLearning.Clustering.Hierarchical;
using Raccoon.Math;
using Raccoon.Core;
using Raccoon.MachineLearning.Permutations;

namespace Raccoon.MachineLearning.Roughset
{
    public class ReductEnsembleBoostingWithDendrogramGenerator : ReductEnsembleBoostingGenerator
    {
        private IReduct[] reducts;
        private int reductCounter;
        private bool reductsCalculated;
        private double[] weights;
        private HierarchicalClustering hCluster;

        public Func<double[], double[], double> Distance { get; set; }
        public Func<int[], int[], DistanceMatrix, double[][], double> Linkage { get; set; }

        public ReductEnsembleBoostingWithDendrogramGenerator()
            : base()
        {
            reductCounter = 0;
            reductsCalculated = false;
        }

        public ReductEnsembleBoostingWithDendrogramGenerator(DataStore data)
            : base(data)
        {
            reductCounter = 0;
            reductsCalculated = false;
        }

        public override void InitDefaultParameters()
        {
            base.InitDefaultParameters();
            this.Distance = Math.Distance.Manhattan;
            this.Linkage = ClusteringLinkage.Average;
        }

        public override void InitFromArgs(Args args)
        {
            base.InitFromArgs(args);

            if (args.Exist(ReductFactoryOptions.Distance))
                this.Distance = (Func<double[], double[], double>)args.GetParameter(ReductFactoryOptions.Distance);

            if (args.Exist(ReductFactoryOptions.Linkage))
                this.Linkage = (Func<int[], int[], DistanceMatrix, double[][], double>)args.GetParameter(ReductFactoryOptions.Linkage);
        }

        protected override void Generate()
        {
            this.GenerateReducts();
            this.BuildDendrogram();

            base.Generate();
        }

        private void BuildDendrogram()
        {
            hCluster = new HierarchicalClustering();
            hCluster.Distance = this.Distance;
            hCluster.Linkage = this.Linkage;

            Dictionary<int, double[]> instances = new Dictionary<int, double[]>(this.MaxIterations);
            for (int i = 0; i < reducts.Length; i++)
                instances.Add(i, ReductToVectorConversionMethods.GetDefaultReconWeights(reducts[i], weights, RuleQualityMethods.ConfidenceW));

            hCluster.Instances = instances;
            hCluster.Compute();
        }

        private void GenerateReducts()
        {
            reductsCalculated = false;

            weights = new double[this.DecisionTable.NumberOfRecords];
            for (int i = 0; i < this.DecisionTable.NumberOfRecords; i++)
                weights[i] = 1.0 / this.DecisionTable.NumberOfRecords;

            this.reducts = new IReduct[this.MaxIterations];
            for (int i = 0; i < this.MaxIterations; i++)
                this.reducts[i] = this.GetNextReduct(weights);

            HierarchicalCluster cluster = new HierarchicalCluster(0);
            cluster.AddMemberObject(0);
            int[] oneElementCluster = new int[1];

            for (int i = 1; i < this.MaxIterations; i++)
            {
                double d = Double.MinValue;
                int bestIndex = -1;

                for (int j = i + 1; j < this.MaxIterations; j++)
                {
                    oneElementCluster[0] = j;

                    double reductDistance = hCluster.GetClusterDistance(cluster.MemberObjects.ToArray(), oneElementCluster);
                    if (d < reductDistance)
                    {
                        d = reductDistance;
                        bestIndex = j;
                    }
                }

                cluster.AddMemberObject(bestIndex);

                IReduct tmp = reducts[i];
                reducts[i] = reducts[bestIndex];
                reducts[bestIndex] = tmp;
            }

            reductCounter = 0;
            reductsCalculated = true;
        }

        public override IReduct GetNextReduct(double[] weights)
        {
            if (reductsCalculated)
                return reducts[reductCounter++];

            return base.GetNextReduct(weights);
        }
    }

    public class ReductEnsembleBoostingWithDendrogramFactory : IReductFactory
    {
        public virtual string FactoryKey
        {
            get { return ReductTypes.ReductEnsembleBoostingWithDendrogram; }
        }

        public virtual IReductGenerator GetReductGenerator(Args args)
        {
            ReductEnsembleBoostingWithDendrogramGenerator rGen = new ReductEnsembleBoostingWithDendrogramGenerator();
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