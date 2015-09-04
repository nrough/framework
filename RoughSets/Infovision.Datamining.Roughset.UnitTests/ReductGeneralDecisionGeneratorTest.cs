using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Datamining.Clustering.Hierarchical;
using Infovision.Math;
using Infovision.Utils;
using NUnit.Framework;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    public class ReductGeneralDecisionGeneratorTest
    {        
        [Test]
        public void GenerateRelativeTest()
        {            
            Random randSeed = new Random();
            int seed = randSeed.Next(Int32.MaxValue);
            RandomSingleton.Seed = seed;

            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);

            PermutationGenerator permGenerator = new PermutationGenerator(data);
            int numberOfPermutations = 100;
            PermutationCollection permList = permGenerator.Generate(numberOfPermutations);

            WeightGeneratorRelative weightGenerator = new WeightGeneratorRelative(data);            

            Args parms = new Args();
            parms.AddParameter("DataStore", data);
            parms.AddParameter("NumberOfThreads", 1);
            parms.AddParameter("FactoryKey", "ReductGeneralizedDecision");
            parms.AddParameter("PermutationCollection", permList);
            parms.AddParameter("WeightGenerator", weightGenerator);                                                                       

            ReductGeneralDecisionGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductGeneralDecisionGenerator;
            reductGenerator.Generate();
                        
            Args parms2 = new Args();
            parms2.AddParameter("DataStore", data);
            parms2.AddParameter("NumberOfThreads", 1);
            parms2.AddParameter("FactoryKey", "ApproximateReductRelativeWeights");
            parms2.AddParameter("PermutationCollection", permList);
            parms2.AddParameter("WeightGenerator", weightGenerator);

            ReductGeneratorWeightsRelative rGen2 = ReductFactory.GetReductGenerator(parms2) as ReductGeneratorWeightsRelative;            

            IReductStore reductPool = reductGenerator.ReductPool;
            foreach (IReduct reduct in reductPool)
            {                
                InformationMeasureWeights m_Weights = new InformationMeasureWeights();                
                double result_W = m_Weights.Calc(reduct);                                
                rGen2.Epsilon = 1.0 - result_W;

                ReductWeights approxReduct = new ReductWeights(data, reduct.Attributes.ToArray(), weightGenerator.Weights, rGen2.Epsilon);
                approxReduct.Id = reduct.Id;

                Console.WriteLine("{0} M(C)={1} eps={2}", approxReduct, result_W, rGen2.Epsilon);
                                
                //if(rGen2.CheckIsReduct(approxReduct) == false)
                Assert.IsTrue(rGen2.CheckIsReduct(approxReduct), String.Format("{0} is not a reduct for eps={1}", approxReduct, rGen2.Epsilon));                
                
                foreach (int attributeId in approxReduct.Attributes)
                {
                    approxReduct.TryRemoveAttribute(attributeId);
                    Assert.IsFalse(rGen2.CheckIsReduct(approxReduct), String.Format("Reduct should not be reducible. Attribute {0} can be removed", attributeId));                        
                    approxReduct.AddAttribute(attributeId);
                }                
            }            
        }

        [Test]
        public void GenerateMajorityTest()
        {
            Random randSeed = new Random();
            int seed = randSeed.Next(Int32.MaxValue);
            RandomSingleton.Seed = seed;

            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);

            PermutationGenerator permGenerator = new PermutationGenerator(data);
            int numberOfPermutations = 100;
            PermutationCollection permList = permGenerator.Generate(numberOfPermutations);

            WeightGeneratorMajority weightGenerator = new WeightGeneratorMajority(data);

            Args parms = new Args();
            parms.AddParameter("DataStore", data);
            parms.AddParameter("NumberOfThreads", 1);
            parms.AddParameter("FactoryKey", "ReductGeneralizedDecision");
            parms.AddParameter("PermutationCollection", permList);
            parms.AddParameter("WeightGenerator", weightGenerator);

            ReductGeneralDecisionGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductGeneralDecisionGenerator;
            reductGenerator.Generate();

            Args parms2 = new Args();
            parms2.AddParameter("DataStore", data);
            parms2.AddParameter("NumberOfThreads", 1);
            parms2.AddParameter("FactoryKey", "ApproximateReductMajorityWeights");
            parms2.AddParameter("PermutationCollection", permList);
            parms2.AddParameter("WeightGenerator", weightGenerator);

            ReductGeneratorWeightsMajority rGen2 = ReductFactory.GetReductGenerator(parms2) as ReductGeneratorWeightsMajority;

            IReductStore reductPool = reductGenerator.ReductPool;
            foreach (IReduct reduct in reductPool)
            {
                InformationMeasureWeights m_Weights = new InformationMeasureWeights();
                double result_W = m_Weights.Calc(reduct);
                
                rGen2.Epsilon = 1.0 - result_W;

                ReductWeights approxReduct = new ReductWeights(data, reduct.Attributes.ToArray(), weightGenerator.Weights, rGen2.Epsilon);
                approxReduct.Id = reduct.Id;

                Console.WriteLine("{0} M(C)={1} eps={2}", approxReduct, result_W, rGen2.Epsilon);

                //if(rGen2.CheckIsReduct(approxReduct) == false)
                Assert.IsTrue(rGen2.CheckIsReduct(approxReduct), String.Format("{0} is not a reduct for eps={1}", approxReduct, rGen2.Epsilon));

                foreach (int attributeId in approxReduct.Attributes)
                {
                    approxReduct.TryRemoveAttribute(attributeId);
                    Assert.IsFalse(rGen2.CheckIsReduct(approxReduct), String.Format("Reduct should not be reducible. Attribute {0} can be removed", attributeId));
                    approxReduct.AddAttribute(attributeId);
                }
            }
        } 
    }
}
