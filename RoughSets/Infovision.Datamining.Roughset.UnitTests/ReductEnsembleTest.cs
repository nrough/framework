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
            
            int numberOfPermutations = 10;            
            PermutationCollection permList = permGenerator.Generate(10);

            int[] epsilons = new int[numberOfPermutations];

            for (int i = 0; i < numberOfPermutations; i++)
            {
                epsilons[i] = rand.Next(100);
            }

            Args parms = new Args();
            parms.AddParameter("DataStore", data);
            parms.AddParameter("NumberOfThreads", 1);
            parms.AddParameter("PermutationEpsilon", epsilons);

            string generatorName = "ReductEnsemble";

            //TODO include algorithm name in Args
            //TODO Replace Args by Dictionary<string, object>
            //TODO Add parameter names as static variables
            //TODO Reduct should be struct not class (has code should be constant?)
            //TODO Make cache keys shorter
            //TODO ReductGenerator.Generator should return collection od reduct stores, usualy 1 element collection but in case of ensembles we will have more


            

            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator(generatorName, parms);
            parms.AddParameter("PermutationCollection", permGen.Generate(100));

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(generatorName, parms);
            

            IReductStore reductStore = reductGenerator.Generate(parms);

            
        }
    }
}
