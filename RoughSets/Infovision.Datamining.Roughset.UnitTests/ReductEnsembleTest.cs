using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using NUnit.Framework;
using Infovision.Data;
using Infovision.Datamining.Roughset;
using Infovision.Utils;
using Infovision.Math;
using Infovision.Datamining.Clustering.Hierarchical;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    class ReductEnsembleTest
    {
        //Refactor
        //TODO include algorithm name in Args
        //TODO Replace Args by Dictionary<string, object>
        //TODO Add parameter names as static variables            
        //TODO Make cache keys shorter 

        [Test]
        public void Foo()
        {            
            Random rand = new Random();
            DataStore data = DataStore.Load(@"Data\playgolf.train", FileFormat.Rses1);
            PermutationGenerator permGenerator = new PermutationGenerator(data);
            
            int numberOfPermutations = 100;
            PermutationCollection permList = permGenerator.Generate(numberOfPermutations);

            //TODO Epsilon generaton according to some distribution
            int[] epsilons = new int[numberOfPermutations];
            for (int i = 0; i < numberOfPermutations; i++)
            {
                epsilons[i] = rand.Next(36);
            }

            Args parms = new Args();
            parms.AddParameter("DataStore", data);
            parms.AddParameter("NumberOfThreads", 1);
            parms.AddParameter("PermutationEpsilon", epsilons);
            //parms.AddParameter("Distance", SimilarityIndex.TverskyDelegate(0.5, 0.5));            
            parms.AddParameter("Distance", SimilarityIndex.ReductSimDelegate(0.5));            
            parms.AddParameter("Linkage", (Func<int[], int[], DistanceMatrix, double>)ClusteringLinkage.Min);
            parms.AddParameter("NumberOfClusters", 2);
            parms.AddParameter("GeneratorName", "ReductEnsemble");

            string generatorName = "ReductEnsemble";                       
                        
            parms.AddParameter("PermutationCollection", permList);

            //TODO Create generator based on parms, not generator name
            //TODO Store args inside generator
            //TODO Generate method without parameters
            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(generatorName, parms);
            IReductStoreCollection reductStoreCollection = reductGenerator.Generate(parms);

            int j=1;
            foreach (IReductStore reductStore in reductStoreCollection)
            {
                Console.WriteLine("Reduct Store: {0}", j++);
                Console.WriteLine("==================", j++);
                
                foreach (IReduct reduct in reductStore)
                {
                    Console.WriteLine("{0}", reduct);
                    j++;
                }
            }

            
            ReductEnsembleGenerator ensembleGenerator = reductGenerator as ReductEnsembleGenerator;
            if (ensembleGenerator != null)
            {
                Bitmap dendrogram = ensembleGenerator.Dendrogram.GetDendrogramAsBitmap(640, 480);
                dendrogram.Save(@"f:\test.bmp");                
            }



        }

        [Test]
        public void TestTinyDouble()
        {
            double a = 1 / 1234;
            double b = 1.0 / 1234;
            double c = 1.0 / (double)1234;
            double d = (double)1 / (double)1234;
            double e = 1 / 1234.0;
            double f = 1 - 0.001;


            Console.WriteLine(a);
            Console.WriteLine(b);
            Console.WriteLine(c);
            Console.WriteLine(d);
            Console.WriteLine(e);
            Console.WriteLine(f);
        }
    }
}
