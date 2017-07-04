//
// NRough Framework
// http://www.nrough.net
//
// Copyright © Sebastian Widz, 2017
// authors at nrough.net

//This file is part of NRough.

//Foobar is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 3 of the License, or
//(at your option) any later version.
//
//Foobar is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Foobar; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//
using System;
using NRough.Data;
using NRough.Core;
using NRough.MachineLearning.Weighting;
using NRough.MachineLearning.Roughsets;
using NRough.Core.CollectionExtensions;

namespace NRough.MachineLearning.Permutations
{
    public class PermutationGeneratorFieldQuality : PermutationGenerator
    {
        public DataStore Data { get; set; }
        public double Epsilon { get; set; }
        public WeightGenerator WeightGenerator { get; set; }
        public int NumberOfShuffles { get; set; }

        private int[] fieldIdsSorted;

        public PermutationGeneratorFieldQuality(DataStore data, WeightGenerator weightGenerator, double epsilon, int numberOfShuffles = 0)
        {
            this.Data = data;
            this.WeightGenerator = weightGenerator;
            this.Epsilon = epsilon;
            this.NumberOfShuffles = numberOfShuffles;

            //Calculate quality measure for each field
            this.fieldIdsSorted = new int[this.Data.DataStoreInfo.CountAttributes(a => a.IsStandard)];
            double[] fieldQualityOrig = new double[fieldIdsSorted.Length];
            int c = 0;
            foreach (var field in this.Data.DataStoreInfo.SelectAttributeIds(a => a.IsStandard))
            {
                fieldIdsSorted[c] = field;
                fieldQualityOrig[c] = InformationMeasureWeights.Instance.Calc(
                    new ReductWeights(this.Data, new int[] { field }, this.Epsilon, this.WeightGenerator.Weights));
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