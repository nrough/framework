using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Infovision.Datamining.Filters.Unsupervised.Attribute.Tests
{
    [TestFixture]
    public class DiscretizationTest
    {
        [Test]
        public void CalculateCutPointsByEqualFrequencyBinningTest()
        {
            double[] data = { 5, 5, 5, 5, 5, 7, 8, 9, 10, 12, 13, 14, 16, 40, 41, 42, 43, 44, 45, 45, 46, 47, 48, 49 };
            
            Discretization disc = new Discretization();                        
            disc.Compute(data, numberOfBins: 3);

            Console.WriteLine(disc.ToStringCuts());
            
            for (int i = 0; i < data.Length; i++)
            {
                Console.WriteLine("{0} {1}", data[i], disc.Search(data[i]));
            }

            double[] dataNotExisting = { 0, 1, 2, 3, 4, 5, 6, 11, 15, 16, 17, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34, 50, 60, 70, 80 };

            for (int i = 0; i < data.Length; i++)
            {
                Console.WriteLine("{0} {1}", dataNotExisting[i], disc.Search(dataNotExisting[i]));
            }
        }    
    }
}
