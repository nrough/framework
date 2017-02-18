using NUnit.Framework;
using Raccoon.Core;
using Raccoon.Data;
using Raccoon.MachineLearning.Classification;
using Raccoon.MachineLearning.Classification.DecisionRules;
using Raccoon.MachineLearning.Classification.DecisionTrees;
using Raccoon.MachineLearning.Classification.Ensembles;
using Raccoon.MachineLearning.Evaluation;
using Raccoon.MachineLearning.Roughset;
using Raccoon.MachineLearning.Weighting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            //compute reducts
            var reducts =
                ReductFactory.GetReductGenerator(parm).GetReducts();

            //output reducts and attributes
            foreach (IReduct reduct in reducts)
                Console.WriteLine(reduct.Attributes.ToArray().ToStr());
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
            forest.Learn(train, train.SelectAttributes(a => a.IsStandard));

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

            //create ada boost ensemble 
            var adaBoost = new AdaBoost<ReductDecisionRules>(prototype);
            adaBoost.Learn(train, train.SelectAttributes(a => a.IsStandard));

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

            //classify test data set
            var result = Classifier.Default.Classify(adaBoost, test);

            //print result header & result
            Console.WriteLine(ClassificationResult.TableHeader());
            Console.WriteLine(result);
        }
    }
}