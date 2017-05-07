using NUnit.Framework;
using NRough.Core;
using NRough.Data;
using NRough.MachineLearning.Classification;
using NRough.MachineLearning.Classification.DecisionTrees;
using NRough.MachineLearning.Classification.Ensembles;
using NRough.MachineLearning.Clustering.Hierarchical;
using NRough.MachineLearning.Evaluation;
using NRough.MachineLearning.Permutations;
using NRough.MachineLearning.Roughsets;
using NRough.MachineLearning.Roughsets.Diversify;
using NRough.MachineLearning.Roughsets.Reducts;
using NRough.MachineLearning.Weighting;
using System;
using System.Linq;
using System.Collections.Generic;
using NRough.MachineLearning.Discretization;
using NRough.MachineLearning.Filters;
using NRough.Core.CollectionExtensions;
using NRough.MachineLearning.Roughsets.Reducts.Comparers;

namespace NRough.Tests.MachineLearning
{
    [TestFixture]
    public class CodeSamples
    {
        [Test]
        public void ApproximateDecisionReduct()
        {
            //load data
            var data = Data.Benchmark.Factory.Golf();

            //set parameters for reduct factory
            var parm = new Args();
            parm.SetParameter(ReductFactoryOptions.DecisionTable, data);
            parm.SetParameter(ReductFactoryOptions.ReductType,
                ReductTypes.ApproximateDecisionReduct);
            parm.SetParameter(ReductFactoryOptions.FMeasure,
                (FMeasure)FMeasures.Majority);
            parm.SetParameter(ReductFactoryOptions.Epsilon, 0.05);
            parm.SetParameter(ReductFactoryOptions.NumberOfReducts, 5);

            //compute reducts
            var reducts =
                ReductFactory.GetReductGenerator(parm).GetReducts();

            //output reducts and attributes
            foreach (IReduct reduct in reducts)
                Console.WriteLine(reduct.Attributes.ToArray().ToStr());
        }

        [Test]
        public void ApproxximateDecisionReductFromClassHierarchy()
        {
            var data = Data.Benchmark.Factory.Golf();
            var weightGen = new WeightGeneratorConstant(data, 1.0 / (double)data.NumberOfRecords);
            weightGen.Generate();
            data.SetWeights(weightGen.Weights);

            ApproximateDecisionReduct model = new ApproximateDecisionReduct();
            model.FMeasure = FMeasures.MajorityWeighted;
            model.Epsilon = 0.0;

            var reducts = model.Learn(data, data.SelectAttributeIds(a => a.IsStandard).ToArray());
            foreach (var reduct in reducts)
                Console.WriteLine(reduct);
        }

        [Test]
        public void WeightedApproximateDecisionReduct()
        {
            //load benchmark data
            var data = Data.Benchmark.Factory.Zoo();
            //set object weights using r(u) weighting schema
            data.SetWeights(new WeightGeneratorRelative(data).Weights);

            //split data into training and testing sets
            DataStore train, test;
            var splitter = new DataSplitterRatio(data, 0.8);
            splitter.Split(out train, out test);

            //set parameters for reduct factory
            var parm = new Args();
            parm.SetParameter(ReductFactoryOptions.DecisionTable, train);
            parm.SetParameter(ReductFactoryOptions.ReductType,
                ReductTypes.ApproximateDecisionReduct);
            parm.SetParameter(ReductFactoryOptions.FMeasure,
                (FMeasure)FMeasures.MajorityWeighted);
            parm.SetParameter(ReductFactoryOptions.Epsilon, 0.05);
            parm.SetParameter(ReductFactoryOptions.NumberOfReducts, 5);

            //compute reducts
            var reductGenerator = ReductFactory.GetReductGenerator(parm);
            var reducts = reductGenerator.GetReducts();

            //select 10 reducts with least number of attributes
            var bestReducts = reducts
                .OrderBy(r => r.Attributes.Count)
                .Take(10);

            //create decision rules based on reducts
            var decisionRules = new ReductDecisionRules(bestReducts);
            //if test instance is not recognized set output as unclassified
            decisionRules.DefaultOutput = null;

            //classify test data
            var result = Classifier.Default
                .Classify(decisionRules, test);

            //output accuracy and coverage
            Console.WriteLine("Accuracy: {0}", result.Accuracy);
            Console.WriteLine("Coverage: {0}", result.Coverage);
        }

        [Test]
        public void DecisionTree()
        {
            //load data
            var data = DataStore.Load("data.txt", DataFormat.CSV);
            //create 10-fold 25-repeated cross validation
            var cv = new CrossValidation(data, 10, 25);

            //create C4.5 decision tree and run cv evaluation
            var c45 = new DecisionTreeC45();
            var result = cv.Run<DecisionTreeC45>(c45);

            //output result
            Console.WriteLine("Train Error: {0}", result.Error);
        }

        [Test]
        public void RandomForest()
        {
            //load data from a CSV file
            var data = DataStore.Load(@"german.data", DataFormat.CSV);

            //Initialize Random Forest
            var forest = new DecisionForestRandom<DecisionTreeC45>();
            forest.Size = 500;

            //Split data
            DataStore train, test;
            var splitter = new DataSplitterRatio(data, 0.8);
            splitter.Split(out train, out test);

            //Build Random Forest
            forest.Learn(train, 
                train.SelectAttributeIds(a => a.IsStandard).ToArray());

            //Test and output results
            var result = Classifier.Default.Classify(forest, test);
            Console.WriteLine(result);
        }

        [Test]
        public void ReductAdaBoost()
        {
            //load training and testing DNA (spieces) data sets 
            var train = Data.Benchmark.Factory.DnaTrain();
            var test = Data.Benchmark.Factory.DnaTest();

            //set weights 
            var weightGen = new WeightGeneratorConstant(train);
            weightGen.Value = (double)1 / (double)train.NumberOfRecords;
            train.SetWeights(weightGen.Weights);

            //create parameters for reduct factory
            var parm = new Args();
            parm.SetParameter(ReductFactoryOptions.ReductType,
                ReductTypes.ApproximateDecisionReduct);
            parm.SetParameter(ReductFactoryOptions.FMeasure,
                (FMeasure)FMeasures.MajorityWeighted);
            parm.SetParameter(ReductFactoryOptions.Epsilon, 0.05);
            parm.SetParameter(ReductFactoryOptions.NumberOfReducts, 100);
            parm.SetParameter(ReductFactoryOptions.ReductComparer, 
                ReductRuleNumberComparer.Default);
            parm.SetParameter(ReductFactoryOptions.SelectTopReducts, 1);

            //create weak classifier prototype
            var prototype = new ReductDecisionRules();
            prototype.ReductGeneratorArgs = parm;
            prototype.DecisionIdentificationMethod = RuleQualityMethods.Confidence;

            //create ada boost ensemble 
            var adaBoost = new AdaBoost<ReductDecisionRules>(prototype);
            adaBoost.Learn(train, 
                train.SelectAttributeIds(a => a.IsStandard).ToArray());

            //classify test data set
            var result = Classifier.Default.Classify(adaBoost, test);

            //print result header & result
            Console.WriteLine(ClassificationResult.TableHeader());
            Console.WriteLine(result);
        }

        public void DiversiifyReductsHierarchicalClustering()
        {
            //load training and testing DNA (spieces) data sets 
            var train = Data.Benchmark.Factory.DnaTrain();
            var test = Data.Benchmark.Factory.DnaTest();

            //create reduct diversification 
            var reductDiversifier = new HierarchicalClusterReductDiversify();
            reductDiversifier.DecisionTable = train;
            reductDiversifier.Distance = Math.Distance.Hamming;
            reductDiversifier.Linkage = ClusteringLinkage.Average;
            reductDiversifier.ReductToVectorMethod 
                = ReductToVectorConversionMethods.GetCorrectBinary;
            reductDiversifier.NumberOfReducts = 10;

            //create parameters for reduct factory 
            //including reduct diversification
            var parm = new Args();
            parm.SetParameter(ReductFactoryOptions.ReductType,
                ReductTypes.ApproximateDecisionReduct);
            parm.SetParameter(ReductFactoryOptions.FMeasure,
                (FMeasure)FMeasures.MajorityWeighted);
            parm.SetParameter(ReductFactoryOptions.Epsilon, 0.05);
            parm.SetParameter(ReductFactoryOptions.NumberOfReducts, 100);
            parm.SetParameter(ReductFactoryOptions.Diversify,
                reductDiversifier);

            var rules = new ReductDecisionRules();
            rules.ReductGeneratorArgs = parm;
            rules.DecisionIdentificationMethod = RuleQualityMethods.Confidence;
            rules.RuleVotingMethod = RuleQualityMethods.Coverage;

            //classify test data set
            var result = Classifier.Default.Classify(rules, test);

            //show results
            Console.WriteLine(result);
        }

        [Test]
        public void Bireduct()
        {
            //load training and testing DNA (spieces) data sets 
            var train = Data.Benchmark.Factory.DnaTrain();
            var test = Data.Benchmark.Factory.DnaTest();

            //create parameters for reduct factory
            var parm = new Args();
            parm.SetParameter(ReductFactoryOptions.ReductType, 
                ReductTypes.Bireduct);
            parm.SetParameter(ReductFactoryOptions.NumberOfReducts, 100);

            //generate bireducts
            IReductGenerator bireductGen = 
                ReductFactory.GetReductGenerator(parm);
            var bireducts = bireductGen.GetReducts();

            //for each bireduct show its attributes and supported objects
            foreach (var bireduct in bireducts)
            {
                Console.WriteLine(bireduct.Attributes.ToArray().ToStr());
                Console.WriteLine(bireduct.SupportedObjects.ToArray().ToStr());
            }
        }

        [Test]
        public void BireductsFromClassHierarchy()
        {
            //load training data sets 
            var train = Data.Benchmark.Factory.DnaTrain();

            //generate permutations based on attributes and objects
            var permGenerator = new PermutationAttributeObjectGenerator(train, 0.5);
            //generate 100 permutations
            var permutations = permGenerator.Generate(100);

            //setup gamma-bireduct generator 
            //generate bireducts based on permutations
            var bireductGammaGenerator = new BireductGammaGenerator();
            bireductGammaGenerator.DecisionTable = train;
            bireductGammaGenerator.Permutations = permutations;
            var bireducts = bireductGammaGenerator.GetReducts();

            //for each bireduct show its attributes and supported objects
            foreach (var bireduct in bireducts)
            {
                Console.WriteLine(bireduct.Attributes.ToArray().ToStr());
                Console.WriteLine(bireduct.SupportedObjects.ToArray().ToStr());
            }
        }

        [Test]
        public void GeneralizedMajorityDecisionReduct()
        {
            //load training data sets 
            var train = Data.Benchmark.Factory.DnaTrain();

            //setup reduct factory parameters
            Args parms = new Args();
            parms.SetParameter(ReductFactoryOptions.DecisionTable, train);
            parms.SetParameter(ReductFactoryOptions.ReductType, 
                ReductTypes.GeneralizedMajorityDecision);
            parms.SetParameter(ReductFactoryOptions.WeightGenerator, 
                new WeightGeneratorMajority(train));
            parms.SetParameter(ReductFactoryOptions.Epsilon, 0.05);
            parms.SetParameter(ReductFactoryOptions.PermutationCollection, 
                new PermutationCollection(10, 
                    train.SelectAttributeIds(a => a.IsStandard).ToArray()));

            //generate reducts
            var reductGenerator = ReductFactory.GetReductGenerator(parms);
            var reducts = reductGenerator.GetReducts();
        }

        [Test]
        public void GeneralizedMajorityDecisionReductWithExceptions()
        {
            //load training and test data sets
            var train = Data.Benchmark.Factory.DnaTrain();
            var test = Data.Benchmark.Factory.DnaTest();

            //setup reduct factory parameters
            Args parms = new Args();
            parms.SetParameter(ReductFactoryOptions.DecisionTable, train);
            parms.SetParameter(ReductFactoryOptions.ReductType,
                ReductTypes.GeneralizedMajorityDecision);
            parms.SetParameter(ReductFactoryOptions.WeightGenerator,
                new WeightGeneratorMajority(train));
            parms.SetParameter(ReductFactoryOptions.Epsilon, 0.05);
            parms.SetParameter(ReductFactoryOptions.PermutationCollection,
                new PermutationCollection(10,
                    train.SelectAttributeIds(a => a.IsStandard)
                        .ToArray()));
            parms.SetParameter(ReductFactoryOptions.UseExceptionRules, 
                true);

            //generate reducts with exceptions
            var reductGenerator = ReductFactory.GetReductGenerator(parms);
            var reducts = reductGenerator.GetReducts();

            foreach (var reduct in reducts)
            {
                var r = reduct as ReductWithExceptions;                
                foreach (var exception in r.Exceptions)
                {
                    Console.WriteLine(exception.Attributes.ToArray().ToStr());
                    Console.WriteLine(exception.SupportedObjects.ToArray().ToStr());
                }
            }

            var rules = new ReductDecisionRules(reducts);            
            rules.DecisionIdentificationMethod = RuleQualityMethods.Confidence;
            rules.RuleVotingMethod = RuleQualityMethods.SingleVote;
            rules.Learn(train, null);

            //classify test data set
            var result = Classifier.Default.Classify(rules, test);

            //show results
            Console.WriteLine(result);
        }        

        [Test]
        public void Disctretization()
        {
            var data = Data.Benchmark.Factory.Vehicle();

            DataStore train, test;
            var splitter = new DataSplitterRatio(data, 0.8);            
            splitter.Split(out train, out test);

            var tableDiscretizer = new DecisionTableDiscretizer(
                new IDiscretizer[]
                {
                    //try to discretize using Fayyad MDL Criterion
                    new DiscretizeFayyad(),

                    //in case Fayyad MDL is to strict 
                    //use standard entropy and 5 buckets
                    new DiscretizeEntropy(5)
                });

            tableDiscretizer.FieldsToDiscretize = train
                .SelectAttributeIds(a => a.IsStandard && a.CanDiscretize());

            var filter = new DiscretizeFilter();
            filter.TableDiscretizer = tableDiscretizer;
            filter.Compute(train);

            foreach(int attributeId in tableDiscretizer.FieldsToDiscretize)
            {
                var fieldDiscretizer = filter.GetAttributeDiscretizer(attributeId);
                
                Console.WriteLine("Attribute {0} was discretized with {1}", 
                    attributeId, fieldDiscretizer.GetType().Name);
                Console.WriteLine("Computed Cuts: {0}", fieldDiscretizer.Cuts.ToStr());                  
            }

            var trainDisc = filter.Apply(train);
            var testDisc = filter.Apply(test);
        }
    }
}