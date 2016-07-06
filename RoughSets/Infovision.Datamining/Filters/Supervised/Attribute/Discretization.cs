using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infovision.Statistics;
using MiscUtil;

namespace Infovision.Datamining.Filters.Supervised.Attribute
{
    public class Discretization<A, L>
        where A : struct, IComparable, IFormattable, IComparable<A>, IEquatable<A>
        where L : struct, IComparable, IFormattable, IComparable<L>, IEquatable<L>
    {
        #region Members

        private double[] cuts;
        private Dictionary<L, int> labelDict;
        private int[] sortedIndices;

        #endregion Members

        #region Constructors

        public Discretization()
        {
            this.labelDict = new Dictionary<L, int>();
        }

        #endregion Constructors

        #region Properties

        public bool UseBetterEncoding { get; set; }
        public bool UseKononenko { get; set; }

        public double[] Cuts
        {
            get { return cuts; }
            set { cuts = value; }
        }

        #endregion Properties

        #region Methods

        private double[] cutPointsForSubset(A[] values, L[] labels, int first, int lastPlusOne, double[] weights = null)
        {
            double[][] counts, bestCounts;
            double[] priorCounts, left, right, cutPoints;
            double currentCutPoint = Double.MinValue;
            double bestCutPoint = -1;
            double currentEntropy, bestEntropy, priorEntropy, gain;
            int bestIndex = -1;
            int numCutPoints = 0;
            double numInstances = 0;

            // Compute number of instances in set
            if ((lastPlusOne - first) < 2)
            {
                return null;
            }

            // Compute class counts.
            counts = new double[2][];
            for (int i = 0; i < counts.Length; i++)
                counts[i] = new double[this.labelDict.Count];

            for (int i = first; i < lastPlusOne; i++)
            {
                numInstances += (weights != null) ? weights[this.sortedIndices[i]] : 1.0;
                counts[1][labelDict[labels[sortedIndices[i]]]] += (weights != null) ? weights[this.sortedIndices[i]] : 1.0;
            }

            // Save prior counts
            priorCounts = new double[this.labelDict.Count];
            Array.Copy(counts[1], 0, priorCounts, 0, this.labelDict.Count);

            // Entropy of the full set
            priorEntropy = Tools.Entropy(priorCounts);
            bestEntropy = priorEntropy;

            // Find best entropy.
            bestCounts = new double[2][];
            for (int i = 0; i < bestCounts.Length; i++)
                bestCounts[i] = new double[this.labelDict.Count];

            for (int i = first; i < (lastPlusOne - 1); i++)
            {
                counts[0][labelDict[labels[sortedIndices[i]]]] += weights != null ? weights[this.sortedIndices[i]] : 1.0;
                counts[1][labelDict[labels[sortedIndices[i]]]] -= weights != null ? weights[this.sortedIndices[i]] : 1.0;

                if (values[sortedIndices[i]].CompareTo(values[sortedIndices[i + 1]]) < 0)
                {
                    currentCutPoint = (Operator.Convert<A, double>(values[sortedIndices[i]]) + Operator.Convert<A, double>(values[sortedIndices[i + 1]])) / 2.0;
                    currentEntropy = Tools.EntropyConditionedOnRows(counts);

                    if (currentEntropy < bestEntropy)
                    {
                        bestCutPoint = currentCutPoint;
                        bestEntropy = currentEntropy;
                        bestIndex = i;

                        Array.Copy(counts[0], 0, bestCounts[0], 0, this.labelDict.Count);
                        Array.Copy(counts[1], 0, bestCounts[1], 0, this.labelDict.Count);
                    }
                    numCutPoints++;
                }
            }

            // Use worse encoding?
            if (!UseBetterEncoding)
            {
                numCutPoints = (lastPlusOne - first) - 1;
            }

            // Checks if gain is zero
            gain = priorEntropy - bestEntropy;
            if (gain <= 0)
            {
                return null;
            }

            // Check if split is to be accepted
            if ((UseKononenko && KononenkosMDL(priorCounts, bestCounts, numInstances, numCutPoints))
                || (!UseKononenko && FayyadAndIranisMDL(priorCounts, bestCounts, numInstances, numCutPoints)))
            {
                // Select split points for the left and right subsets
                left = cutPointsForSubset(values, labels, first, bestIndex + 1, weights);
                right = cutPointsForSubset(values, labels, bestIndex + 1, lastPlusOne, weights);

                // Merge cutpoints and return them
                if ((left == null) && (right) == null)
                {
                    cutPoints = new double[1];
                    cutPoints[0] = bestCutPoint;
                }
                else if (right == null)
                {
                    cutPoints = new double[left.Length + 1];
                    Array.Copy(left, 0, cutPoints, 0, left.Length);
                    cutPoints[left.Length] = bestCutPoint;
                }
                else if (left == null)
                {
                    cutPoints = new double[1 + right.Length];
                    cutPoints[0] = bestCutPoint;
                    Array.Copy(right, 0, cutPoints, 1, right.Length);
                }
                else
                {
                    cutPoints = new double[left.Length + right.Length + 1];
                    Array.Copy(left, 0, cutPoints, 0, left.Length);
                    cutPoints[left.Length] = bestCutPoint;
                    Array.Copy(right, 0, cutPoints, left.Length + 1, right.Length);
                }

                return cutPoints;
            }
            else
            {
                return null;
            }
        }

        private bool KononenkosMDL(double[] priorCounts, double[][] bestCounts, double numInstances, int numCutPoints)
        {
            double distPrior, instPrior, distAfter = 0, sum, instAfter = 0;
            double before, after;
            int numClassesTotal;

            // Number of classes occuring in the set
            numClassesTotal = 0;
            foreach (double priorCount in priorCounts)
            {
                if (priorCount > 0)
                {
                    numClassesTotal++;
                }
            }

            // Encode distribution prior to split
            distPrior = Tools.Log2Binomial(numInstances + numClassesTotal - 1, numClassesTotal - 1);

            // Encode instances prior to split.
            instPrior = Tools.Log2Multinomial(numInstances, priorCounts);

            before = instPrior + distPrior;

            // Encode distributions and instances after split.
            foreach (double[] bestCount in bestCounts)
            {
                sum = Tools.Sum(bestCount);
                distAfter += Tools.Log2Binomial(sum + numClassesTotal - 1, numClassesTotal - 1);
                instAfter += Tools.Log2Multinomial(sum, bestCount);
            }

            // Coding cost after split
            after = Tools.Log2(numCutPoints) + distAfter + instAfter;

            // Check if split is to be accepted
            return (before > after);
        }

        private bool FayyadAndIranisMDL(double[] priorCounts, double[][] bestCounts, double numInstances, int numCutPoints)
        {
            double priorEntropy, entropy, gain;
            double entropyLeft, entropyRight, delta;
            int numClassesTotal, numClassesRight, numClassesLeft;

            // Compute entropy before split.
            priorEntropy = Tools.Entropy(priorCounts);

            // Compute entropy after split.
            entropy = Tools.EntropyConditionedOnRows(bestCounts);

            // Compute information gain.
            gain = priorEntropy - entropy;

            // Number of classes occuring in the set
            numClassesTotal = 0;
            foreach (double priorCount in priorCounts)
            {
                if (priorCount > 0)
                {
                    numClassesTotal++;
                }
            }

            // Number of classes occuring in the left subset
            numClassesLeft = 0;
            for (int i = 0; i < bestCounts[0].Length; i++)
            {
                if (bestCounts[0][i] > 0)
                {
                    numClassesLeft++;
                }
            }

            // Number of classes occuring in the right subset
            numClassesRight = 0;
            for (int i = 0; i < bestCounts[1].Length; i++)
            {
                if (bestCounts[1][i] > 0)
                {
                    numClassesRight++;
                }
            }

            // Entropy of the left and the right subsets
            entropyLeft = Tools.Entropy(bestCounts[0]);
            entropyRight = Tools.Entropy(bestCounts[1]);

            // Compute terms for MDL formula
            delta = Tools.Log2(System.Math.Pow(3, numClassesTotal) - 2)
                - ((numClassesTotal * priorEntropy) - (numClassesRight * entropyRight) - (numClassesLeft * entropyLeft));

            // Check if split is to be accepted
            return (gain > ((Tools.Log2(numCutPoints) + delta) / numInstances));
        }

        public void Compute(A[] values, L[] labels, double[] weights = null)
        {
            for (int i = 0, labelKey = 0; i < values.Length; i++)
            {
                if (!labelDict.ContainsKey(labels[i]))
                    labelDict.Add(labels[i], labelKey++);
            }

            sortedIndices = Enumerable.Range(0, values.Length).ToArray();
            Array.Sort(sortedIndices, (a, b) => values[a].CompareTo(values[b]));
            this.cuts = cutPointsForSubset(values, labels, 0, sortedIndices.Length, weights);

            if (this.cuts == null)
            {
                this.cuts = new double[0];
            }
        }

        public int Search(A value)
        {
            if (this.cuts == null)
                return 1;

            for (int i = 0; i < cuts.Length; i++)
                if (Operator.Convert<A, double>(value).CompareTo(cuts[i]) <= 0)
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

        #endregion Methods
    }
}