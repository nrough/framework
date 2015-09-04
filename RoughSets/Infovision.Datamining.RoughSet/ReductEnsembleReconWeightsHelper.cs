using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static double[] GetDefaultReconWeights(IReduct reduct, double[] objectWeights)
        {
            double[] result = new double[objectWeights.Length];
            Array.Copy(objectWeights, result, objectWeights.Length);
            foreach (EquivalenceClass e in reduct.EquivalenceClassMap)            
                foreach (int objectIdx in e.GetObjectIndexes(e.MajorDecision))                
                    result[objectIdx] *= -1;
            return result;
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reduct"></param>
        /// <param name="objectWeights"></param>
        /// <returns></returns>
        public static double[] GetErrorReconWeights(IReduct reduct, double[] objectWeights)
        {
            double[] result = new double[objectWeights.Length];
            Array.Copy(objectWeights, result, objectWeights.Length);                           

            foreach (EquivalenceClass e in reduct.EquivalenceClassMap)            
                foreach (int i in e.GetObjectIndexes(e.MajorDecision))
                    result[i] = 0;                                                
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reduct"></param>
        /// <param name="objectWeights"></param>
        /// <returns></returns>
        public static double[] GetCorrectReconWeights(IReduct reduct, double[] objectWeights)
        {
            double[] result = new double[objectWeights.Length];            
            foreach (EquivalenceClass e in reduct.EquivalenceClassMap)
                foreach (int i in e.GetObjectIndexes(e.MajorDecision))
                    result[i] = objectWeights[i];
            return result;
        }


        public static double[] GetCorrectBinary(IReduct reduct, double[] objectWeights)
        {
            double[] result = new double[objectWeights.Length];
            foreach (EquivalenceClass e in reduct.EquivalenceClassMap)
                foreach (int i in e.GetObjectIndexes(e.MajorDecision))
                    result[i] = 1.0;
            return result;
        }
        
    }
}
