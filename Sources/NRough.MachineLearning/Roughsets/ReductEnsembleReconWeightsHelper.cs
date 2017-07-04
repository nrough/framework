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

namespace NRough.MachineLearning.Roughsets
{
    public static class ReductToVectorConversionMethods
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="reduct"></param>
        /// <param name="objectWeights"></param>
        /// <returns></returns>
        public static double[] GetDefaultReconWeights(IReduct reduct, double[] objectWeights, RuleQualityMethod decisionIdentificationMethod)
        {
            //TODO If arg_max returns more than one decisionInternalValue, this method should take this into account
            double[] result = new double[objectWeights.Length];
            for (int i = 0; i < objectWeights.Length; i++)
                result[i] = (double)objectWeights[i];
            for (int i = 0; i < reduct.DataStore.NumberOfRecords; i++)
                if (RoughClassifier.IsObjectRecognizable(reduct.DataStore, i, reduct, decisionIdentificationMethod))
                    result[i] *= -1;
            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="reduct"></param>
        /// <param name="objectWeights"></param>
        /// <returns></returns>
        public static double[] GetErrorReconWeights(IReduct reduct, double[] objectWeights, RuleQualityMethod decisionIdentificationMethod)
        {
            double[] result = new double[objectWeights.Length];
            Array.Copy(objectWeights, result, objectWeights.Length);
            for (int i = 0; i < reduct.DataStore.NumberOfRecords; i++)
                if (RoughClassifier.IsObjectRecognizable(reduct.DataStore, i, reduct, decisionIdentificationMethod))
                    result[i] = 0;
            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="reduct"></param>
        /// <param name="objectWeights"></param>
        /// <returns></returns>
        public static double[] GetCorrectReconWeights(IReduct reduct, double[] objectWeights, RuleQualityMethod decisionIdentificationMethod)
        {
            double[] result = new double[objectWeights.Length];
            for (int i = 0; i < reduct.DataStore.NumberOfRecords; i++)
                if (RoughClassifier.IsObjectRecognizable(reduct.DataStore, i, reduct, decisionIdentificationMethod))
                    result[i] = (double)objectWeights[i];
            return result;
        }

        public static double[] GetCorrectBinary(IReduct reduct, double[] objectWeights, RuleQualityMethod decisionIdentificationMethod)
        {
            double[] result = new double[objectWeights.Length];
            for (int i = 0; i < reduct.DataStore.NumberOfRecords; i++)
                if (RoughClassifier.IsObjectRecognizable(reduct.DataStore, i, reduct, decisionIdentificationMethod))
                    result[i] = 1.0;
            return result;
        }
    }
}