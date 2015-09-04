using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Infovision.Data;
using Infovision.Datamining.Roughset;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    class ReductEnsembleTest
    {
        [Test]
        public void Foo()
        {            
            Random rand = new Random();
            DataStore data = DataStore.Load(@"Data\playgolf.train", FileFormat.Rses1);
            PermutationGenerator permGenerator = new PermutationGenerator(data);
            
            int numberOfPermutations = 20;
            PermutationCollection permList = permGenerator.Generate(numberOfPermutations);

            int[] epsilons = new int[numberOfPermutations];

            for (int i = 0; i < numberOfPermutations; i++)
            {
                epsilons[i] = rand.Next(36);
            }

            Args parms = new Args();
            parms.AddParameter("DataStore", data);
            parms.AddParameter("NumberOfThreads", 1);
            parms.AddParameter("PermutationEpsilon", epsilons);

            string generatorName = "ReductEnsemble";

            //Refactor
            //TODO include algorithm name in Args
            //TODO Replace Args by Dictionary<string, object>
            //TODO Add parameter names as static variables            
            //TODO Make cache keys shorter            
                        
            parms.AddParameter("PermutationCollection", permList);

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(generatorName, parms);
            IReductStoreCollection reductStoreCollection = reductGenerator.Generate(parms);

            int j = 0;
            foreach (IReductStore reductStore in reductStoreCollection)
                foreach (IReduct reduct in reductStore)
                {
                    Console.WriteLine("{0}: {1}~ {2} -> {3}", j, permList[j], epsilons[j], reduct);
                    j++;
                }

        }
    }
}
