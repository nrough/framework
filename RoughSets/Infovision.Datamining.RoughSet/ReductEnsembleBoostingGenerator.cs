using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Core;
using Infovision.MachineLearning.Classification;

//TODO Implement Cost of misclassification. (Imbalanced classes)
namespace Infovision.Datamining.Roughset
{
    public delegate double UpdateWeightsDelegate(double currentWeight, int numberOfOutputValues, long actualOutput, long predictedOutput, double totalError, double classificationCost);

    public delegate double CalcModelConfidenceDelegate(int numberOfOutputValues, double totalError);

    public class ReductEnsembleBoostingGenerator : ReductGenerator
    {
        public double Threshold { get; set; }
        public RuleQualityFunction IdentyficationType { get; set; }
        public RuleQualityFunction VoteType { get; set; }
        public double MinimumVoteValue { get; set; }
        public int NumberOfReductsInWeakClassifier { get; set; }
        public int MaxIterations { get; set; }
        public int IterationsPassed { get { return this.iterPassed; } }
        public int MaxNumberOfWeightResets { get; set; }
        public int NumberOfWeightResets { get; protected set; }
        public bool CheckEnsembleErrorDuringTraining { get; set; }
        public WeightGenerator WeightGenerator { get; set; }
        public UpdateWeightsDelegate UpdateWeights { get; set; }
        public CalcModelConfidenceDelegate CalcModelConfidence { get; set; }
        public bool FixedPermutations { get; set; }
        public bool UseClassificationCost { get; set; }

        protected int iterPassed;
        protected ReductStoreCollection Models { get; set; }

        private Dictionary<long, int> value2index;
        private long[] decisions;
        private int decCount;
        private int decCountPlusOne;
        private double[] classificationCosts;

        public ReductEnsembleBoostingGenerator()
            : base()
        {
            this.Threshold = 0.5;
            this.IdentyficationType = RuleQuality.ConfidenceW;
            this.VoteType = RuleQuality.CoverageW;
            this.NumberOfReductsInWeakClassifier = 1;
            this.MaxIterations = 100;
            this.MaxNumberOfWeightResets = 0;
            this.CheckEnsembleErrorDuringTraining = false;
            this.UpdateWeights = ReductEnsembleBoostingGenerator.UpdateWeightsAdaBoost_All;
            this.CalcModelConfidence = ReductEnsembleBoostingGenerator.ModelConfidenceAdaBoostM1;
            this.MinimumVoteValue = Double.MinValue;
            this.UseClassificationCost = true;
        }

        public ReductEnsembleBoostingGenerator(DataStore data)
            : this()
        {
            this.WeightGenerator = new WeightBoostingGenerator(data);
        }

        public override void InitDefaultParameters()
        {
            base.InitDefaultParameters();

            this.Threshold = 0.5;
            this.IdentyficationType = RuleQuality.ConfidenceW;
            this.VoteType = RuleQuality.CoverageW;
            this.NumberOfReductsInWeakClassifier = 1;
            this.MaxIterations = 100;
            this.MaxNumberOfWeightResets = 0;
            this.CheckEnsembleErrorDuringTraining = false;
            this.UpdateWeights = ReductEnsembleBoostingGenerator.UpdateWeightsAdaBoost_All;
            this.CalcModelConfidence = ReductEnsembleBoostingGenerator.ModelConfidenceAdaBoostM1;
            this.UseClassificationCost = true;
        }

        public override void InitFromArgs(Args args)
        {
            base.InitFromArgs(args);

            if (this.DataStore != null)
            {
                if (args.Exist(ReductGeneratorParamHelper.WeightGenerator))
                {
                    this.WeightGenerator = (WeightGenerator)args.GetParameter(ReductGeneratorParamHelper.WeightGenerator);
                }
                else
                {
                    this.WeightGenerator = new WeightBoostingGenerator(this.DataStore);
                    this.WeightGenerator.Generate();
                }

                int numOfAttr = this.DataStore.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard);
                double m0 = new InformationMeasureWeights()
                    .Calc(new ReductWeights(this.DataStore, new int[] { }, this.Epsilon, this.WeightGenerator.Weights));
                this.Threshold = 1.0 - m0;

                this.InitFromDecisionValues(this.DataStore, this.DataStore.DataStoreInfo.GetDecisionValues());
            }

            if (args.Exist(ReductGeneratorParamHelper.WeightGenerator))
                this.WeightGenerator = (WeightGenerator)args.GetParameter(ReductGeneratorParamHelper.WeightGenerator);

            if (args.Exist(ReductGeneratorParamHelper.Threshold))
                this.Threshold = (double)args.GetParameter(ReductGeneratorParamHelper.Threshold);

            if (args.Exist(ReductGeneratorParamHelper.IdentificationType))
                this.IdentyficationType = (RuleQualityFunction)args.GetParameter(ReductGeneratorParamHelper.IdentificationType);

            if (args.Exist(ReductGeneratorParamHelper.VoteType))
                this.VoteType = (RuleQualityFunction)args.GetParameter(ReductGeneratorParamHelper.VoteType);

            if (args.Exist(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier))
                this.NumberOfReductsInWeakClassifier = (int)args.GetParameter(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier);

            if (args.Exist(ReductGeneratorParamHelper.MaxIterations))
                this.MaxIterations = (int)args.GetParameter(ReductGeneratorParamHelper.MaxIterations);

            if (args.Exist(ReductGeneratorParamHelper.CheckEnsembleErrorDuringTraining))
                this.CheckEnsembleErrorDuringTraining = (bool)args.GetParameter(ReductGeneratorParamHelper.CheckEnsembleErrorDuringTraining);

            if (args.Exist(ReductGeneratorParamHelper.UpdateWeights))
                this.UpdateWeights = (UpdateWeightsDelegate)args.GetParameter(ReductGeneratorParamHelper.UpdateWeights);

            if (args.Exist(ReductGeneratorParamHelper.CalcModelConfidence))
                this.CalcModelConfidence = (CalcModelConfidenceDelegate)args.GetParameter(ReductGeneratorParamHelper.CalcModelConfidence);

            if (args.Exist(ReductGeneratorParamHelper.MaxNumberOfWeightResets))
                this.MaxNumberOfWeightResets = (int)args.GetParameter(ReductGeneratorParamHelper.MaxNumberOfWeightResets);

            if (args.Exist(ReductGeneratorParamHelper.MinimumVoteValue))
                this.MinimumVoteValue = (double)args.GetParameter(ReductGeneratorParamHelper.MinimumVoteValue);

            if (args.Exist(ReductGeneratorParamHelper.FixedPermutations))
                this.FixedPermutations = (bool)args.GetParameter(ReductGeneratorParamHelper.FixedPermutations);

            if (args.Exist(ReductGeneratorParamHelper.UseClassificationCost))
                this.UseClassificationCost = (bool)args.GetParameter(ReductGeneratorParamHelper.UseClassificationCost);
        }

        protected void InitFromDecisionValues(DataStore data, ICollection<long> decisionValues)
        {
            this.decCount = decisionValues.Count;
            this.decCountPlusOne = decisionValues.Count + 1;
            this.decisions = new long[this.decCountPlusOne];
            this.decisions[0] = -1;
            long[] decArray = decisionValues.ToArray();
            double[] decDistribution = new double[this.decCount];
            for (int i = 0; i < this.decCount; i++)
                decDistribution[i] = (int)data.DataStoreInfo.DecisionInfo.Histogram.GetBinValue(decArray[i]);
            Array.Sort(decDistribution, decArray);
            Array.Copy(decArray, 0, decisions, 1, decCount);
            value2index = new Dictionary<long, int>(decCountPlusOne);
            value2index.Add(-1, 0);
            for (int i = 0; i < decArray.Length; i++)
                value2index.Add(decArray[i], i + 1);

            this.classificationCosts = new double[this.decCountPlusOne];
            this.classificationCosts[0] = 0;
            if (this.classificationCosts.Length > 1)
                this.classificationCosts[1] = 1;
            for (int i = 2; i < this.decCountPlusOne; i++)
                this.classificationCosts[i] = 1.0 / (decDistribution[i - 1] / decDistribution[0]);
        }

        protected override void Generate()
        {
            this.Models = new ReductStoreCollection(this.MaxIterations);

            double alphaSum = 0.0;
            iterPassed = 0;
            this.NumberOfWeightResets = 0;
            double error = -1.0;
            int K = this.DataStore.DataStoreInfo.NumberOfDecisionValues;

            this.WeightGenerator.Generate();
            double[] weights = this.WeightGenerator.Weights;

            long[] decisionValues = this.DataStore.DataStoreInfo.GetDecisionValues().ToArray();
            object tmpLock = new object();

            long[] predictions = new long[this.DataStore.NumberOfRecords];

            do
            {
                IReductStoreCollection reductStoreCollection = this.CreateModel(weights, this.NumberOfReductsInWeakClassifier);

                RoughClassifier classifier = new RoughClassifier(reductStoreCollection, this.IdentyficationType, this.VoteType, decisionValues);
                classifier.MinimumVoteValue = this.MinimumVoteValue;
                ClassificationResult result = classifier.Classify(this.DataStore, weights);
                error = result.WeightMisclassified + result.WeightUnclassified;

                //Console.WriteLine("Iteration {0}: {1} error", iterPassed + 1, error);

                double alpha = this.CalcModelConfidence(K, error);

                if (error >= this.Threshold)
                {
                    this.NumberOfWeightResets++;

                    if (this.NumberOfWeightResets > this.MaxNumberOfWeightResets)
                    {
                        if (iterPassed == 0)
                        {
                            this.AddModel(reductStoreCollection.First(), alpha);
                            iterPassed = 1;
                        }

                        break;
                    }

                    this.WeightGenerator.Reset();
                    weights = this.WeightGenerator.Weights;

                    //Console.WriteLine("Weights resets: {0}", this.NumberOfWeightResets);

                    continue;
                }

                this.AddModel(reductStoreCollection.First(), alpha);

                double sum = 0.0d;
                var rangePrtitioner = Partitioner.Create(0, weights.Length);

                ParallelOptions options = new ParallelOptions()
                {
                    MaxDegreeOfParallelism = InfovisionConfiguration.MaxDegreeOfParallelism
                };

                Parallel.ForEach(
                    rangePrtitioner,
                    options,
                    () => 0.0,
                    (range, loopState, initialValue) =>
                    {
                        double partialSum = initialValue;
                        for (int i = range.Item1; i < range.Item2; i++)
                        {
                            long actual = this.DataStore.GetDecisionValue(i);
                            double classificationCost = 1.0;
                            if (actual != result.GetPrediction(i))
                                classificationCost = this.classificationCosts[this.value2index[actual]];

                            weights[i] = (double)this.UpdateWeights((double)weights[i],
                                                                         K,
                                                                         actual,
                                                                         result.GetPrediction(i),
                                                                         error,
                                                                         classificationCost);
                            partialSum += (double)weights[i];
                        }
                        return partialSum;
                    },
                    (localPartialSum) =>
                    {
                        lock (tmpLock)
                        {
                            sum += localPartialSum;
                        }
                    });

                result = null;

                //Normalize object weights
                //Parallel.For(0, this.TrainData.NumberOfRecords, i =>
                for (int i = 0; i < this.DataStore.NumberOfRecords; i++)
                {
                    weights[i] /= sum;
                }//);

                alphaSum += alpha;

                iterPassed++;

                if (this.CheckEnsembleErrorDuringTraining)
                {
                    if (this.Models.Count > 1)
                    {
                        // Normalize weights for models confidence
                        foreach (IReductStore rs in this.Models.Where(r => r.IsActive))
                        {
                            rs.Weight /= alphaSum;
                        }

                        RoughClassifier classifierEnsemble = new RoughClassifier(this.Models, this.IdentyficationType, this.VoteType, decisionValues);
                        ClassificationResult resultEnsemble = classifierEnsemble.Classify(this.DataStore);

                        // De-normalize weights for models confidence
                        foreach (IReductStore rs in this.Models.Where(r => r.IsActive))
                        {
                            rs.Weight *= alphaSum;
                        }

                        if (resultEnsemble.WeightMisclassified + resultEnsemble.WeightUnclassified <= 0.0001)
                        {
                            bool modelHasConverged = true;
                            foreach (IReductStore model in this.Models)
                            {
                                model.IsActive = false;

                                // Normalize weights for models confidence
                                foreach (IReductStore rs in this.Models.Where(r => r.IsActive))
                                {
                                    rs.Weight /= (alphaSum - model.Weight);
                                }

                                RoughClassifier localClassifierEnsemble = new RoughClassifier(
                                    this.Models, this.IdentyficationType, this.VoteType, decisionValues);
                                ClassificationResult localResultEnsemble = localClassifierEnsemble.Classify(this.DataStore);

                                // De-normalize weights for models confidence
                                foreach (IReductStore rs in this.Models.Where(r => r.IsActive))
                                {
                                    rs.Weight *= (alphaSum - model.Weight);
                                }

                                model.IsActive = true;

                                if (resultEnsemble.WeightMisclassified + resultEnsemble.WeightUnclassified > 0.001)
                                {
                                    modelHasConverged = false;
                                    break;
                                }
                            }

                            if (modelHasConverged == true)
                                break;
                        }
                    }
                }
                else
                {
                    if (error == 0.0)
                        break;
                }
            } while (iterPassed < this.MaxIterations);

            // Normalize weights for models confidence
            if (alphaSum != 0.0)
            {
                foreach (IReductStore rs in this.Models)
                {
                    rs.Weight /= alphaSum;
                }
            }
        }

        protected virtual void AddModel(IReductStore model, double modelWeight)
        {
            model.Weight = modelWeight;
            this.Models.AddStore(model);
        }

        public virtual IReductStoreCollection GetReductGroups(int numberOfEnsembles = Int32.MaxValue)
        {
            return this.Models;
        }

        public override IReductStoreCollection GetReductStoreCollection(int numberOfEnsembles = Int32.MaxValue)
        {
            return this.Models;
        }

        public virtual IReduct GetNextReduct(double[] weights)
        {
            Permutation permutation = this.InnerParameters != null
                                    ? ReductFactory.GetPermutationGenerator(this.InnerParameters).Generate(1)[0]
                                    : new PermutationGenerator(this.DataStore).Generate(1)[0];

            return this.CreateReduct(permutation.ToArray(), this.Epsilon, weights);
        }

        public virtual IReductStoreCollection CreateModel(double[] weights, int size = 0)
        {
            if (this.InnerParameters == null)
                throw new InvalidOperationException("Parameters for internal model are not provided. Please use InnerParameters key to provide setup for internal model creation.");

            Args localParameters = (Args)this.InnerParameters.Clone();

            double[] weightsCopy = new double[weights.Length];
            Array.Copy(weights, weightsCopy, weights.Length);
            WeightGenerator localWeightGen = new WeightGenerator(this.DataStore);
            localWeightGen.Weights = weightsCopy;
            localParameters.SetParameter(ReductGeneratorParamHelper.WeightGenerator, localWeightGen);

            if (this.FixedPermutations)
            {
                if (localParameters.Exist(ReductGeneratorParamHelper.PermutationCollection) == false && size != 0)
                {
                    PermutationCollection localPermCollection = this.InnerParameters.Exist(ReductGeneratorParamHelper.PermuatationGenerator)
                        ? ((IPermutationGenerator)this.InnerParameters.GetParameter(ReductGeneratorParamHelper.PermuatationGenerator)).Generate(size)
                        : this.PermutationGenerator.Generate(size);
                    localParameters.SetParameter(ReductGeneratorParamHelper.PermutationCollection, localPermCollection);
                    this.InnerParameters.SetParameter(ReductGeneratorParamHelper.PermutationCollection, localPermCollection);
                }
                else if (this.InnerParameters.Exist(ReductGeneratorParamHelper.PermutationCollection) == false)
                {
                    throw new InvalidOperationException("No fixed permutation nor collection size to generate permutation was given");
                }
            }
            else if (size != 0)
            {
                localParameters.RemoveParameter(ReductGeneratorParamHelper.PermutationCollection);
                localParameters.SetParameter(ReductGeneratorParamHelper.NumberOfReducts, size);
                localParameters.SetParameter(ReductGeneratorParamHelper.NumberOfPermutations, size);
            }

            IReductGenerator generator = ReductFactory.GetReductGenerator(localParameters);
            generator.Run();

            return generator.GetReductStoreCollection();
        }

        public override IReduct CreateReduct(int[] permutation, double epsilon, double[] weights, IReductStore reductStore = null, IReductStoreCollection reductStoreCollection = null)
        {
            if (this.InnerParameters == null)
                throw new InvalidOperationException("Parameters for internal model are not provided. Please use InnerParameters key to provide setup for internal model creation.");

            Args localParameters = (Args)this.InnerParameters.Clone();

            double[] weightsCopy = new double[weights.Length];
            Array.Copy(weights, weightsCopy, weights.Length);

            WeightGenerator localWeightGen = new WeightGenerator(this.DataStore);
            localWeightGen.Weights = weightsCopy;

            int[] attr = new int[permutation.Length];
            Array.Copy(permutation, attr, permutation.Length);

            this.InnerParameters.SetParameter(ReductGeneratorParamHelper.WeightGenerator, localWeightGen);

            double localEpsilon = epsilon;
            this.InnerParameters.SetProperty(ReductGeneratorParamHelper.Epsilon, ref localEpsilon);

            IReductGenerator generator = ReductFactory.GetReductGenerator(this.InnerParameters);
            IReduct reduct = generator.CreateReduct(permutation, localEpsilon, weights);
            reduct.Id = this.GetNextReductId().ToString();
            return reduct;
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id)
        {
            throw new NotImplementedException("ReductEnsembleBoostingGenerator.CreateReductObject(...) is not implemented");
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id, EquivalenceClassCollection equivalenceClasses)
        {
            return this.CreateReductObject(fieldIds, epsilon, id);
        }

        #region Delegate implementations

        public static double UpdateWeightsAdaBoostM1(double currentWeight, int numberOfOutputValues, long actualOutput, long predictedOutput, double totalError, double classificationCost = 1.0)
        {
            if (actualOutput == predictedOutput)
                return 1.0;
            double alpha = ReductEnsembleBoostingGenerator.ModelConfidenceAdaBoostM1(numberOfOutputValues, totalError);
            return classificationCost * currentWeight * System.Math.Exp(alpha);
        }

        public static double UpdateWeightsAdaBoost_All(double currentWeight, int numberOfOutputValues, long actualOutput, long predictedOutput, double totalError, double classificationCost = 1.0)
        {
            double alpha = ReductEnsembleBoostingGenerator.ModelConfidenceAdaBoostM1(numberOfOutputValues, totalError);
            if (actualOutput == predictedOutput)
                return currentWeight * System.Math.Exp(-alpha);
            return classificationCost * currentWeight * System.Math.Exp(alpha);
        }

        public static double UpdateWeightsAdaBoost_OnlyCorrect(double currentWeight, int numberOfOutputValues, long actualOutput, long predictedOutput, double totalError, double classificationCost = 1.0)
        {
            double alpha = ReductEnsembleBoostingGenerator.ModelConfidenceAdaBoostM1(numberOfOutputValues, totalError);
            if (actualOutput == predictedOutput)
                return currentWeight * System.Math.Exp(-alpha);
            return classificationCost * currentWeight;
        }

        public static double UpdateWeightsAdaBoost_OnlyNotCorrect(double currentWeight, int numberOfOutputValues, long actualOutput, long predictedOutput, double totalError, double classificationCost = 1.0)
        {
            if (actualOutput == predictedOutput)
                return currentWeight;
            double alpha = ReductEnsembleBoostingGenerator.ModelConfidenceAdaBoostM1(numberOfOutputValues, totalError);
            return classificationCost * currentWeight * System.Math.Exp(alpha);
        }

        public static double UpdateWeightsDummy(double currentWeight, int numberOfOutputValues, long actualOutput, long predictedOutput, double totalError, double classificationCost = 1.0)
        {
            return currentWeight;
        }

        public static double ModelConfidenceAdaBoostM1(int numberOfOutputValues, double totalError)
        {
            return System.Math.Log((1.0 - totalError) / (totalError + 0.000000000001))
                + System.Math.Log(numberOfOutputValues - 1.0);
        }

        public static double ModelConfidenceEqual(int numberOfOutputValues, double totalError)
        {
            return 1.0;
        }

        #endregion Delegate implementations
    }

    public class ReductEnsembleBoostingFactory : IReductFactory
    {
        public virtual string FactoryKey
        {
            get { return ReductFactoryKeyHelper.ReductEnsembleBoosting; }
        }

        public virtual IReductGenerator GetReductGenerator(Args args)
        {
            ReductEnsembleBoostingGenerator rGen = new ReductEnsembleBoostingGenerator();
            rGen.InitFromArgs(args);
            return rGen;
        }

        public virtual IPermutationGenerator GetPermutationGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter(ReductGeneratorParamHelper.TrainData);
            return new PermutationGenerator(dataStore);
        }
    }
}