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
        public static double[] GetDefaultReconWeights(IReduct reduct, decimal[] objectWeights)
        {
            //TODO If arg_max returns more than one decision, this method should take this into account
            double[] result = new double[objectWeights.Length];
            //Array.Copy(objectWeights, result, objectWeights.Length);
            for (int i = 0; i <= objectWeights.Length; i++)
                result[i] = (double)objectWeights[i];
            foreach (EquivalenceClass e in reduct.EquivalenceClasses)
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
        public static double[] GetErrorReconWeights(IReduct reduct, decimal[] objectWeights)
        {
            double[] result = new double[objectWeights.Length];
            Array.Copy(objectWeights, result, objectWeights.Length);                           

            foreach (EquivalenceClass e in reduct.EquivalenceClasses)            
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
        public static double[] GetCorrectReconWeights(IReduct reduct, decimal[] objectWeights)
        {
            double[] result = new double[objectWeights.Length];            
            foreach (EquivalenceClass e in reduct.EquivalenceClasses)
                foreach (int i in e.GetObjectIndexes(e.MajorDecision))
                    result[i] = (double)objectWeights[i];
            return result;
        }


        public static double[] GetCorrectBinary(IReduct reduct, decimal[] objectWeights)
        {
            double[] result = new double[objectWeights.Length];
            foreach (EquivalenceClass e in reduct.EquivalenceClasses)
                foreach (int i in e.GetObjectIndexes(e.MajorDecision))
                    result[i] = 1.0;
            return result;
        }
        
    }
}
