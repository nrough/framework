﻿using System;

namespace Infovision.Datamining.Roughset
{
    public static class ReductEnsembleReconWeightsHelper
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="reduct"></param>
        /// <param name="objectWeights"></param>
        /// <returns></returns>
        public static double[] GetDefaultReconWeights(IReduct reduct, decimal[] objectWeights, RuleQualityFunction decisionIdentificationMethod)
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
        public static double[] GetErrorReconWeights(IReduct reduct, decimal[] objectWeights, RuleQualityFunction decisionIdentificationMethod)
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
        public static double[] GetCorrectReconWeights(IReduct reduct, decimal[] objectWeights, RuleQualityFunction decisionIdentificationMethod)
        {
            double[] result = new double[objectWeights.Length];
            for (int i = 0; i < reduct.DataStore.NumberOfRecords; i++)
                if (RoughClassifier.IsObjectRecognizable(reduct.DataStore, i, reduct, decisionIdentificationMethod))
                    result[i] = (double)objectWeights[i];
            return result;
        }

        public static double[] GetCorrectBinary(IReduct reduct, decimal[] objectWeights, RuleQualityFunction decisionIdentificationMethod)
        {
            double[] result = new double[objectWeights.Length];
            for (int i = 0; i < reduct.DataStore.NumberOfRecords; i++)
                if (RoughClassifier.IsObjectRecognizable(reduct.DataStore, i, reduct, decisionIdentificationMethod))
                    result[i] = 1.0;
            return result;
        }
    }
}