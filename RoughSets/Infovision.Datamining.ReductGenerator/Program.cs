using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Datamining.Experimenter.Parms;
using Infovision.Datamining.Roughset;
using Infovision.Data;

namespace Infovision.Datamining.ReductGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            //args
            //data
            //filter class (reduct type)
            //max number of reducts
            //min epsilon
            //max epsilon

            string fileName = "dna_modified.trn"; //args[0]
            int minEpsiolon = 0;
            int maxEpsilon = 99;
            int stepEpsilon = 1;
            int numberOfSubsets = 100;
            
            int currentEpsilon;
            IdentificationType currentIdentType;
            string currentFactoryKey;
            
            DataStore dataStore = DataStore.Load(fileName, FileFormat.Rses1);                        
                                                                        
            IParameter parmEpsilon = new ParameterNumericRange<int>("Epsilon", minEpsiolon, maxEpsilon, stepEpsilon);
            IParameter factoryKey = new ParameterValueCollection<string>(ReductGeneratorParamHelper.FactoryKey,
                                                                            new string[] { ReductFactoryKeyHelper.ApproximateReductMajorityWeights,
                                                                                           ReductFactoryKeyHelper.ApproximateReductRelativeWeights,
                                                                                           ReductFactoryKeyHelper.ApproximateReductRelative, 
                                                                                           ReductFactoryKeyHelper.ApproximateReductMajority,
                                                                                           ReductFactoryKeyHelper.ApproximateReductPositive
                                                                                            });

            IParameter parmIdentification = new ParameterValueCollection<IdentificationType>(ReductGeneratorParamHelper.IdentificationType, new IdentificationType[] { IdentificationType.Confidence,
                                                                                                                                            IdentificationType.Coverage,
                                                                                                                                            IdentificationType.WeightConfidence,
                                                                                                                                            IdentificationType.WeightCoverage,
                                                                                                                                            IdentificationType.Support,
                                                                                                                                            IdentificationType.WeightSupport});



            ParameterCollection parameterList = new ParameterCollection(new IParameter[] { factoryKey, parmEpsilon, parmIdentification });
            
            int i = 0;
            foreach(object[] parm in parameterList.Values())
            {
                i++;

                currentFactoryKey = (string) parm[0];
                currentEpsilon = (int) parm[1];
                currentIdentType = (IdentificationType) parm[2];

                Console.WriteLine("{0} {1} {2}", currentFactoryKey, currentEpsilon, currentIdentType);

                Utils.Args config = new Utils.Args(new string[] { ReductGeneratorParamHelper.FactoryKey, 
                                                                  ReductGeneratorParamHelper.DataStore,                                                                   
                                                                  ReductGeneratorParamHelper.IdentificationType }, 
                                                   new object[] { currentFactoryKey, 
                                                                  dataStore,                                                                   
                                                                  currentIdentType });

                PermutationCollection permutations = ReductFactory.GetPermutationGenerator(config).Generate(numberOfSubsets);

                RoughClassifier roughClassifier = new RoughClassifier();
                roughClassifier.Train(dataStore, currentFactoryKey, currentEpsilon, permutations);

                foreach (IReduct reduct in roughClassifier.ReductStore)
                {
                    Console.WriteLine(reduct);    
                }

                roughClassifier.ReductStore.Save(String.Format("reducts{0}.xml", i));

                break;
            }

            Console.ReadKey();
                   
        }
    }
}
