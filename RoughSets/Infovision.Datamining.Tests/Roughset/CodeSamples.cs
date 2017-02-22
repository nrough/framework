using NUnit.Framework;
using Raccoon.Core;
using Raccoon.Data;
using Raccoon.MachineLearning.Classification;
using Raccoon.MachineLearning.Classification.DecisionTrees;
using Raccoon.MachineLearning.Classification.Ensembles;
using Raccoon.MachineLearning.Clustering.Hierarchical;
using Raccoon.MachineLearning.Evaluation;
using Raccoon.MachineLearning.Permutations;
using Raccoon.MachineLearning.Roughset;
using Raccoon.MachineLearning.Roughset.Diversify;
using Raccoon.MachineLearning.Roughset.Reducts;
using Raccoon.MachineLearning.Weighting;
using System;
using System.Linq;

namespace Raccoon.MachineLearning.Tests.Roughset
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

            var reducts = model.Learn(data, data.SelectAttributes(a => a.IsStandard).Select(f => f.Id).ToArray());
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
            var data = DataStore.Load("data.txt", FileFormat.CSV);
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
            var data = DataStore.Load(@"german.data", FileFormat.CSV);

            //Initialize Random Forest
            var forest = new DecisionForestRandom<DecisionTreeC45>();
            forest.Size = 500;

            //Split data
            DataStore train, test;
            var splitter = new DataSplitterRatio(data, 0.8);
            splitter.Split(out train, out test);

            //Build Random Forest
            forest.Learn(train, 
                train.SelectAttributes(a => a.IsStandard)
                    .Select(f => f.Id).ToArray());

            //Test and output results
            var result = Classifier.Default.Classify(forest, test);
            Console.WriteLine(result);
        }

        [Test]
        public void ReductAdaBoost()
        {
            //load training and testing DNA (spieces) data sets 
            var train = Data.Benchmark.Factory.Dna();
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
                train.SelectAttributes(a => a.IsStandard)
                     .Select(f => f.Id).ToArray());

            //classify test data set
            var result = Classifier.Default.Classify(adaBoost, test);

            //print result header & result
            Console.WriteLine(ClassificationResult.TableHeader());
            Console.WriteLine(result);
        }

        public void DiversiifyReductsHierarchicalClustering()
        {
            //load training and testing DNA (spieces) data sets 
            var train = Data.Benchmark.Factory.Dna();
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
            var train = Data.Benchmark.Factory.Dna();
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
            var train = Data.Benchmark.Factory.Dna();

            //genesrate permutations based on attributes and objects
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
    }
}