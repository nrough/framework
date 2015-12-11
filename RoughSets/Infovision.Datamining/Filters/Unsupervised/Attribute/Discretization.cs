using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiscUtil;

namespace Infovision.Datamining.Filters.Unsupervised.Attribute
{
    
    public class Discretization<T>
        where T : struct, IComparable, IFormattable, IComparable<T>, IEquatable<T>
    {
        #region Members

        private T[] cuts;
        private double weightPerInterval;
        private bool useWeightPerInterval;

        #endregion

        #region Constructors

        public Discretization()
        {
            this.useWeightPerInterval = false;
            this.weightPerInterval = 0.0;
            this.NumberOfBuckets = 10;
        }

        #endregion

        #region Properties

        public double WeightPerInterval
        {
            get { return this.weightPerInterval; }
            set 
            {
                if (value <= 0.0)
                    throw new InvalidOperationException("Weight per interval must be positive");

                this.weightPerInterval = value;
                this.useWeightPerInterval = true;

            }
        }

        public bool UseWeightPerInterval
        {
            get { return this.useWeightPerInterval; }
            set
            {
                if (value == true)
                {
                    if (this.weightPerInterval == 0)
                        throw new InvalidOperationException("Weight value must be set first");
                }
                else
                {
                    this.weightPerInterval = 0.0;
                }

                this.useWeightPerInterval = value;
            }
        }

        public int NumberOfBuckets { get; set; }

        public bool UseEqualFrequency { get; set; }

        public bool UseEntropy { get; set; }

        public T[] Cuts
        {
            get { return cuts; }
        }

        #endregion

        #region Methods

        protected void CalculateCutPointsByEntropy(T[] values, double[] weights)
        {
            if (values == null)
                throw new ArgumentException("Value array cannot be null", "values");

            if(values.Length == 0)
                throw new ArgumentException("Value array does not contain any elements", "values");

            T min = values[0];
            T max = values[0];
            double binWidth = 0.0;
            double entropy;
            double bestEntropy = Double.MaxValue;
            int bestNumOfBins = 1;            
            double[] distribution;

            //Find min and max
            for (int i = 0; i < values.Length; i++)
            {                
                if (values[i].CompareTo(max) > 0)
                    max = values[i];
                if (values[i].CompareTo(min) < 0)
                    min = values[i];
            }

            double minD = Operator.Convert<T, double>(min);
            double maxD = Operator.Convert<T, double>(max);

            //Find best bin number
            for (int i = 0; i < this.NumberOfBuckets; i++)
            {
                distribution = new double[i + 1];
                binWidth = (maxD - minD) / (double)(i + 1);
 
                //Compute distribution
                for(int j = 0; j < values.Length; j++)
                {                    
                    //TODO if missing
                    for (int k = 0; k < i + 1; k++)
                    {                                                
                        if( Operator.Convert<T, double>(values[j]) <= (minD + (((double)k + 1) * binWidth)) )
                        {
                            distribution[k] += (weights != null) ? weights[j] : 1.0;
                            break;
                        }

                    }
                }

                //Compute entropy
                entropy = 0;
                for (int k = 0; k < i + 1; k++)
                {
                    if (distribution[k] < 2)
                    {
                        entropy = Double.MaxValue;
                        break;
                    }
                    entropy -= (distribution[k] * System.Math.Log((distribution[k] - 1) / binWidth));
                }

                if(entropy < bestEntropy)
                {
                    bestEntropy = entropy;
                    bestNumOfBins = i + 1;
                }
            }
                
            //Calculate custs
            T[] cutPoints = null;
            //Fix?
            binWidth = bestNumOfBins > 0 ? (maxD - minD) / bestNumOfBins : 0.0;
            if((bestNumOfBins > 1) && (binWidth > 0))
            {                                
                cutPoints = new T[bestNumOfBins - 1];
                for(int i = 1; i < bestNumOfBins; i++)
                {
                    cutPoints[i - 1] = Operator.Convert<double, T>(minD + (binWidth * i));
                }
            }
            this.cuts = cutPoints;
        }

        protected void CalculateCutPointsByEqualWidthBinning(T[] values, double[] weights = null)
        {
            T max = Operator<T>.Zero;
            T min = Operator<T>.One;
            T currentValue;
            for(int i = 0; i<values.Length; i++)
            {
                currentValue = values[i];
                if (max.CompareTo(min) < 0)
                {
                    min = currentValue;
                    max = min;
                }
                if (currentValue.CompareTo(max) > 0)
                {
                    max = currentValue;
                }
                if(currentValue.CompareTo(min) < 0)
                {
                    min = currentValue;
                }
            }

            double minD = Operator.Convert<T, double>(min);
            double maxD = Operator.Convert<T, double>(max);

            double binWidth = (maxD - minD) / (double)this.NumberOfBuckets;
            T[] cutPoints = null;
            if((this.NumberOfBuckets > 1) && (binWidth > 0))
            {
                cutPoints = new T[this.NumberOfBuckets - 1];
                for (int i = 1; i < this.NumberOfBuckets; i++)
                    cutPoints[i - 1] = Operator.Convert<double, T> (minD + (binWidth * i));                        
            }

            this.cuts = cutPoints;
        }

        protected void CalculateCutPointsByEqualFrequencyBinning(T[] values, double[] weights = null)
        {           
            T[] data = (T[])values.Clone();
            Array.Sort<T>(data);

            double tmpSum = 0;
            if (weights != null)
                for (int i = 0; i < data.Length; i++)
                    tmpSum += weights[i];

            double sumOfWeights = (weights == null) ? data.Length : tmpSum;

            double freq = 0;
            T[] cutPoints = null;
            if (this.UseWeightPerInterval)
            {
                freq = this.WeightPerInterval;
                cutPoints = new T[(int)(sumOfWeights / freq)];
            }
            else
            {
                freq = sumOfWeights / this.NumberOfBuckets;
                cutPoints = new T[this.NumberOfBuckets - 1];
            }

            double counter = 0, last = 0;
            int cpindex = 0, lastIndex = -1;

            for (int i = 0; i < data.Length - 1; i++)
            {
                if (weights == null)
                {
                    counter += 1;
                    sumOfWeights -= 1;
                }
                else
                {
                    counter += weights[i];
                    sumOfWeights -= weights[i];
                }

                if (data[i].CompareTo(data[i + 1]) < 0)
                {
                    // Have we passed the ideal size?
                    if (counter >= freq)
                    {
                        // Is this break point worse than the last one?
                        if (((freq - last) < (counter - freq)) && (lastIndex != -1))
                        {

                            cutPoints[cpindex] = Operator.DivideInt32(Operator<T>.Add(data[lastIndex], data[lastIndex + 1]), 2);

                            //cutPoints[cpindex] = (data[lastIndex] + data[lastIndex + 1]) / 2;
                            counter -= last;
                            last = counter;
                            lastIndex = i;
                        }
                        else
                        {
                            cutPoints[cpindex] = Operator.DivideInt32(Operator<T>.Add(data[i], data[i + 1]), 2);
                            //cutPoints[cpindex] = (data[i] + data[i + 1]) / 2;
                            counter = 0;
                            last = 0;
                            lastIndex = -1;
                        }

                        cpindex++;
                        freq = (sumOfWeights + counter) / ((cutPoints.Length + 1) - cpindex);
                    }
                    else
                    {
                        lastIndex = i;
                        last = counter;
                    }
                }
            }

            // Check whether there was another possibility for a cut point
            if ((cpindex < cutPoints.Length) && (lastIndex != -1))
            {
                cutPoints[cpindex] = Operator.DivideInt32(Operator<T>.Add(data[lastIndex], data[lastIndex + 1]), 2);
                //cutPoints[cpindex] = (data[lastIndex] + data[lastIndex + 1]) / 2;
                cpindex++;
            }

            // Did we find any cutpoints?
            if (cpindex == 0)
            {
                cuts = null;
            }
            else
            {
                T[] cp = new T[cpindex];
                Array.Copy(cutPoints, cp, cpindex);
                cuts = cp;
            }
        }

        public void Compute(T[] values, double[] weights = null)
        {
            if(this.UseEntropy)
                this.CalculateCutPointsByEntropy(values, weights);
            else if (this.UseEqualFrequency)
                this.CalculateCutPointsByEqualFrequencyBinning(values, weights);
            else
                this.CalculateCutPointsByEqualWidthBinning(values, weights);
        }
        
        public int Search(T value)
        {
            if (this.cuts == null)
                return 1;
            
            for (int i = 0; i < cuts.Length; i++)
                if (value.CompareTo(cuts[i]) <= 0)
                    return i;

            return cuts.Length;
        }

        public void WriteToCSVFile(string filePath)
        {
            System.IO.File.WriteAllText(filePath, this.ToString());
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (cuts.Length > 0)
                sb.AppendLine(String.Format("{0}: <{1} {2})", 0, "-Inf", cuts[0]));

            for (int i = 1; i < cuts.Length; i++)
                sb.AppendLine(String.Format("{0}: <{1} {2})", i, cuts[i - 1], cuts[i]));

            if (cuts.Length > 0)
                sb.AppendLine(String.Format("{0}: <{1} {2})", cuts.Length, cuts[cuts.Length - 1], "+Inf"));

            return sb.ToString();
        }

        #endregion
    }
}

