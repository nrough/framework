using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    public class PermutationGeneratorFieldQuality : PermutationGenerator
    {
        public DataStore Data { get; set; }
        public decimal Epsilon { get; set; }
        public WeightGenerator WeightGenerator { get; set; }
        public int NumberOfShuffles { get; set; }

        private int[] fieldIdsSorted;

        public PermutationGeneratorFieldQuality(DataStore data, WeightGenerator weightGenerator, decimal epsilon, int numberOfShuffles = 0)
        {
            this.Data = data;
            this.WeightGenerator = weightGenerator;
            this.Epsilon = epsilon;
            this.NumberOfShuffles = numberOfShuffles;

            //Calculate quality measure for each field
            InformationMeasureWeights measure = new InformationMeasureWeights();
            this.fieldIdsSorted = new int[this.Data.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard)];
            decimal[] fieldQualityOrig = new decimal[fieldIdsSorted.Length];
            int c = 0;
            foreach (var field in this.Data.DataStoreInfo.GetFields(FieldTypes.Standard))
            {
                fieldIdsSorted[c] = field.Id;
                fieldQualityOrig[c] = measure.Calc(
                    new ReductWeights(this.Data, new int[] { field.Id }, this.Epsilon, this.WeightGenerator.Weights));
                c++;
            }

            //Sort according to fieldQuality
            Array.Sort(fieldQualityOrig, fieldIdsSorted);

            //Console.WriteLine("S: {0}", fieldIdsSorted.ToStr());

            //Console.WriteLine("W: {0}", fieldQualityOrig.ToStr());
        }
        
        protected override Permutation CreatePermutation()
        {
            int[] fieldIds = new int[fieldIdsSorted.Length];

            Array.Copy(fieldIdsSorted, fieldIds, fieldIdsSorted.Length);
            fieldIds.ShuffleFwd(this.NumberOfShuffles);                                    
            Permutation permutation = new Permutation(fieldIds);
            return permutation;

            /*
            //Normalize 
            for (int i = 0; i < fieldQuality.Length; i++)
            {
                fieldQuality[i] /= fieldQualitySum;
            }

            //Build Cumulative Distribution Function (CDF)
            double[] cdf = new double[fieldQuality.Length];
            cdf[0] = fieldQuality[0];
            for (int i = 1; i < fieldQuality.Length; i++)
                cdf[i] = cdf[i - 1] + fieldQuality[i];

            List<int> fieldList = new List<int>(fieldIds);
            List<double> cdfList = new List<double>(cdf);
            int k = fieldIds.Length - 1;
            int[] perm = new int[fieldIds.Length];
            while (cdfList.Count > 0)
            {
                double r = RandomSingleton.Random.NextDouble();
                int pos = cdfList.BinarySearch(r);
                if (pos < 0)
                {
                    pos = ~pos;

                    if ((pos == cdfList.Count)
                        || ((pos != 0) && System.Math.Abs(cdfList[pos] - r) > System.Math.Abs(cdfList[pos - 1] - r)))
                    {
                        pos = pos - 1;
                    }
                }

                perm[k] = fieldList.ElementAt(pos);
                fieldList.RemoveAt(pos);
                cdfList.RemoveAt(pos);
                k--;
            }
            */
        }
    }
}
