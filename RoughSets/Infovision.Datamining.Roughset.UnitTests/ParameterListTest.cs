using System;
using Infovision.Test;
using NUnit.Framework;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    public class ParameterListTest
    {
        ITestParameter parmReductType;
        ITestParameter parmNumberOfReducts;
        ITestParameter parmTestNumber;
        ITestParameter parmNFold;
        ITestParameter parmNumberOfPermutations;
        ITestParameter parmEpsilon;

        public ParameterListTest()
        {
            parmNumberOfPermutations = new ParameterValue<Int32>("NumberOfPermutations", 100);
            parmNumberOfReducts = new ParameterValueList<Int32>("NumberOfReducts", 10);
            parmNFold = new ParameterValueList<Int32>("NumberOfFolds", 1);
            parmTestNumber = new ParameterNumericRange<Int32>("TestNumber", 1, 10, 1);
            parmEpsilon = new ParameterNumericRange<Int32>("ApproximationDegree", 0, 99, 1);
            parmReductType = new ParameterValueList<String>("ReductType", new String[] {"ApproximateReductPositive", 
                                                                                        "ApproximateReductMajority", 
                                                                                        "ApproximateReductRelative",
                                                                                        "Bireduct",
                                                                                        "BireductRelative",
                                                                                        "GammaBireduct"});
        }

        //[SetUp]
        //public void Init()
        //{
        //}

        [Test]
        public void InclusionTest()
        {
            ParameterList parameterList = new ParameterList(new ITestParameter[] { parmNumberOfPermutations,
                                                                                parmNumberOfReducts,
                                                                                parmNFold,
                                                                                parmTestNumber,
                                                                                parmEpsilon,
                                                                                parmReductType});

            Int32 counter = 0;
            ParameterVectorEnumerator i_parm = (ParameterVectorEnumerator)parameterList.ParmValueEnumerator;
            while (i_parm.MoveNext())
            {
                counter++;
            }

            Assert.AreEqual(6000, counter);
        }

        [Test]
        public void ExclusionTest()
        {
            ParameterList parameterList = new ParameterList();
            
            parameterList.Add(parmNumberOfPermutations);
            parameterList.Add(parmNumberOfReducts);
            parameterList.Add(parmNFold);
            parameterList.Add(parmTestNumber);
            parameterList.Add(parmEpsilon);
            parameterList.Add(parmReductType);

            ParameterExclusion exclusion1 = new ParameterExclusion();
            exclusion1.AddExclusion("ReductType", "ApproximateReductPositive");

            ParameterExclusion exclusion2 = new ParameterExclusion(new String[] { "ReductType", "TestNumber" }, new Object[] { "ApproximateReductMajority", 10 });
            
            parameterList.AddExclusion(exclusion1);
            parameterList.AddExclusion(exclusion2);


            int counter = 0;
            ParameterVectorEnumerator i_parm = (ParameterVectorEnumerator)parameterList.ParmValueEnumerator;
            while (i_parm.MoveNext())
            {
                ParameterValueVector parameterVector = (ParameterValueVector)i_parm.Current;
                counter++;
            }

            Assert.AreEqual(4900, counter);
        }
    }
}
