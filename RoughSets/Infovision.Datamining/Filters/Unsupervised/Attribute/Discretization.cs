using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Filters.Unsupervised.Attribute
{
    
    public class Discretization
    {
        private double[] cuts;

        public Discretization()
        {
        }

        public double[] Cuts
        {
            get { return cuts; }
        }


        /*
    protected void calculateCutPointsByEqualFrequencyBinning(int index) {

    // Copy data so that it can be sorted
    Instances data = new Instances(getInputFormat());

    // Sort input data
    data.sort(index);

    // Compute weight of instances without missing values
    double sumOfWeights = 0;
    for (int i = 0; i < data.numInstances(); i++) {
      if (data.instance(i).isMissing(index)) {
    break;
      } else {
    sumOfWeights += data.instance(i).weight();
      }
    }
    double freq;
    double[] cutPoints = new double[m_NumBins - 1];
    if (getDesiredWeightOfInstancesPerInterval() > 0) {
      freq = getDesiredWeightOfInstancesPerInterval();
      cutPoints = new double[(int)(sumOfWeights / freq)];
    } else {
      freq = sumOfWeights / m_NumBins;
      cutPoints = new double[m_NumBins - 1];
    }

    // Compute break points
    double counter = 0, last = 0;
    int cpindex = 0, lastIndex = -1;
    for (int i = 0; i < data.numInstances() - 1; i++) {

      // Stop if value missing
      if (data.instance(i).isMissing(index)) {
    break;
      }
      counter += data.instance(i).weight();
      sumOfWeights -= data.instance(i).weight();

      // Do we have a potential breakpoint?
      if (data.instance(i).value(index) < 
      data.instance(i + 1).value(index)) {

    // Have we passed the ideal size?
    if (counter >= freq) {

      // Is this break point worse than the last one?
      if (((freq - last) < (counter - freq)) && (lastIndex != -1)) {
        cutPoints[cpindex] = (data.instance(lastIndex).value(index) +
                  data.instance(lastIndex + 1).value(index)) / 2;
        counter -= last;
        last = counter;
        lastIndex = i;
      } else {
        cutPoints[cpindex] = (data.instance(i).value(index) +
                  data.instance(i + 1).value(index)) / 2;
        counter = 0;
        last = 0;
        lastIndex = -1;
      }
      cpindex++;
      freq = (sumOfWeights + counter) / ((cutPoints.length + 1) - cpindex);
    } else {
      lastIndex = i;
      last = counter;
    }
      }
    }

    // Check whether there was another possibility for a cut point
    if ((cpindex < cutPoints.length) && (lastIndex != -1)) {
      cutPoints[cpindex] = (data.instance(lastIndex).value(index) +
                data.instance(lastIndex + 1).value(index)) / 2;      
      cpindex++;
    }

    // Did we find any cutpoints?
    if (cpindex == 0) {
      m_CutPoints[index] = null;
    } else {
      double[] cp = new double[cpindex];
      for (int i = 0; i < cpindex; i++) {
    cp[i] = cutPoints[i];
      }
      m_CutPoints[index] = cp;
    }
  }
  */
        protected void CalculateCutPointsByEqualFrequencyBinning(double[] values, int numberOfBins)
        {
            //copy the data so it can be sorted
            double[] data = (double[])values.Clone();

            //sort the data
            Array.Sort<double>(data);

            // Compute weight of instances without missing values
            double sumOfWeights = data.Length;
            double freq = sumOfWeights / numberOfBins;
            double[] cutPoints = new double[numberOfBins - 1];

            // Compute break points
            double counter = 0, last = 0;
            int cpindex = 0, lastIndex = -1;

            for (int i = 0; i < data.Length - 1; i++)
            {
                counter += 1;
                sumOfWeights -= 1;

                // Do we have a potential breakpoint?
                if (data[i] < data[i + 1])
                {
                    // Have we passed the ideal size?
                    if (counter >= freq)
                    {
                        // Is this break point worse than the last one?
                        if (((freq - last) < (counter - freq)) && (lastIndex != -1))
                        {
                            cutPoints[cpindex] = (data[lastIndex] + data[lastIndex + 1]) / 2;
                            counter -= last;
                            last = counter;
                            lastIndex = i;
                        }
                        else
                        {
                            cutPoints[cpindex] = (data[i] + data[i + 1]) / 2;
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
                cutPoints[cpindex] = (data[lastIndex] + data[lastIndex + 1]) / 2;
                cpindex++;
            }

            // Did we find any cutpoints?
            if (cpindex == 0)
            {
                cuts = null;
            }
            else
            {
                double[] cp = new double[cpindex];
                for (int i = 0; i < cpindex; i++)
                {
                    cp[i] = cutPoints[i];
                }

                cuts = cp;
            }
        }

        public void Compute(double[] values, int numberOfBins)
        {
            this.CalculateCutPointsByEqualFrequencyBinning(values, numberOfBins);
        }
        
        public int Search(double value)
        {
            int ret = -1;

            for (int i = 0; i <= cuts.Length; i++)
            {
                if (i != 0 && i != cuts.Length)
                {
                    if (cuts[i - 1] < value
                        && cuts[i] >= value)
                    {
                        return i;
                    }
                }
                else if(i == 0)
                {
                    if(cuts[i] > value)
                    {
                        return i;
                    }
                }
                else if (i == cuts.Length)
                {
                    if (cuts[i-1] < value)
                        return i;
                }
            }

            return ret;
        }

        public string ToStringCuts()
        {
            StringBuilder sb = new StringBuilder();

            if(cuts.Length > 0)
                sb.AppendLine(String.Format("{0} {1} {2}", 0, "-Inf", cuts[0]));

            for (int i = 1; i < cuts.Length; i++)
            {
                sb.AppendLine(String.Format("{0} {1} {2}", i, cuts[i - 1], cuts[i]));                
            }

            if (cuts.Length > 0)
                sb.AppendLine(String.Format("{0} {1} {2}", cuts.Length, cuts[cuts.Length-1], "+Inf"));

            return sb.ToString();
        }

        public void WriteToCSVFile(string filePath)
        {
            System.IO.File.WriteAllText(filePath, ToStringCuts());
        }
    }
}

