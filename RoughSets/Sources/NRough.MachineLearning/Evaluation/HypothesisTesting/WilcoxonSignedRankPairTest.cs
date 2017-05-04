using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MathNet.Numerics.Distributions;
using System.Collections;

namespace NRough.MachineLearning.Evaluation.HypothesisTesting
{
    public enum HypothesisType
    {
        FirstIsGreaterThanSecond,
        FirstIsSmallerThanSecond,
        AreDifferent
    }    

    public class WilcoxonSignedRankPairTest : IFormattable
    {
        private int exactLimit;

        public HypothesisType AlternativeHypothesis { get; set; }
        public double Alpha { get; set; }
        
        public bool ContinuityCorrection { get; set; }
        public bool SplitTiesEvenly { get; set; }
        public bool Exact { get; set; }        
        public bool ExactDistributionBasedOnRanks { get; set; }
        public int ExactLimit
        {
            get { return exactLimit; }
            set
            {
                if (value > 30)
                    throw new ArgumentOutOfRangeException("ExactLimit", "value > 30");
                exactLimit = value;
            }
        }

        public double Statistic { get; private set; }
        public double Mean { get; private set; }
        public double Variance { get; private set; }
        public double StdDev { get; private set; }
        public double PValue { get; private set; }
        public int N { get; private set; }

        public bool CanRejectNullHypothesis { get { return PValue < Alpha; } }        
                
        public WilcoxonSignedRankPairTest()
        {
            Alpha = 0.05;
            AlternativeHypothesis = HypothesisType.AreDifferent;
            ContinuityCorrection = true;
            ExactLimit = 20;

            SplitTiesEvenly = true;
            ExactDistributionBasedOnRanks = false;
        }

        public void Compute(double[] serie1, double[] serie2)
        {
            if (serie1 == null)
                throw new ArgumentNullException("serie1");
            if (serie2 == null)
                throw new ArgumentNullException("serie2");
            if (serie1.Length == 0)
                throw new ArgumentException("serie1.Length == 0", "serie1");
            if (serie2.Length == 0)
                throw new ArgumentException("serie2.Length == 0", "serie2");
            if (serie1.Length != serie2.Length)
                throw new ArgumentException("serie1.Length != serie2.Length", "serie2");
            
            double[] difference = new double[serie1.Length];
            int[] sign = new int[serie1.Length];

            for (int i = 0; i < difference.Length; i++)
            {
                double diff = serie1[i] - serie2[i];
                difference[i] = System.Math.Abs(diff);
                sign[i] = System.Math.Sign(diff);
            }

            int zeros = sign.Count(x => x == 0);

            double[] ranks;
            if (SplitTiesEvenly)
            {
                var ranksAll = difference.Select((d, i) => new { Value = d, Index = i, Sign = sign[i] })
                    .OrderBy(x => x.Value)
                    .Select((x, i) => new { Value = x.Value, Index = x.Index, Sign = x.Sign, RankTmp = i + 1 })
                    .GroupBy(x => x.Value)
                    .SelectMany(g => g.Select(e => new { Value = e.Value, Index = e.Index, Sign = e.Sign, Rank = g.Average(s => s.RankTmp) }));

                ranks = (zeros % 2) == 0
                    ? ranksAll.OrderBy(x => System.Math.Abs(x.Sign) * 2)
                        .Where((v, i) => (i >= zeros / 2))
                        .Select(v => v.Rank).ToArray()
                    : ranksAll.OrderBy(x => System.Math.Abs(x.Sign) * 2)
                        .Where((v, i) => (i > zeros / 2))
                        .Select(v => v.Rank).ToArray();

                if (zeros % 2 == 0)
                {
                    Statistic = ranksAll.Where(v => v.Sign > 0).Sum(v => v.Rank)
                        + 0.5 * ranksAll.Where(v => v.Sign == 0).Sum(v => v.Rank);
                }
                else
                {
                    Statistic = ranksAll.Where(v => v.Sign > 0).Sum(v => v.Rank)
                        + 0.5 * (ranksAll.Where(v => v.Sign == 0).Sum(v => v.Rank)
                            - ranksAll.First(v => v.Sign == 0).Value);
                }                                    
            }
            else
            {                
                var ranksAll = difference.Select((d, i) => new { Value = d, Index = i, Sign = sign[i] })
                    .OrderBy(x => x.Value)
                    .Select((x, i) => new { Value = x.Value, Index = x.Index, Sign = x.Sign, RankTmp = (x.Value != 0) ? (i + 1 - zeros) : 0 })
                    .GroupBy(x => x.Value)
                    .SelectMany(g => g.Select(e => new { Value = e.Value, Index = e.Index, Sign = e.Sign, Rank = g.Average(s => s.RankTmp) }));                

                ranks = ranksAll.Where(v => v.Sign != 0)
                    .Select(v => v.Rank).ToArray();                
                Statistic = ranksAll.Where(v => v.Sign > 0).Sum(v => v.Rank);
            }

            N = ranks.Count();

            if (N < 5)
            {
                PValue = 1.0;
                return;
            }

            Mean = N * (N + 1.0) / 4.0;
            //same as (N * (N + 1.0) * (2.0 * N + 1.0)) / 24.0
            Variance = Mean * ((2.0 * N + 1.0) / 6.0);
            StdDev = System.Math.Sqrt(Variance);

            if (!Exact && N <= ExactLimit)
                Exact = true;                                   
                       
            if (N <= ExactLimit || Exact)
            {                
                double[] table = ExactDistributionBasedOnRanks 
                    ? InitExactDistribution(ranks) 
                    : InitExactDistribution(Array.ConvertAll<int, double>(Enumerable.Range(1, ranks.Length).ToArray(), i => (double)i));

                switch (AlternativeHypothesis)
                {
                    case HypothesisType.AreDifferent:
                        double cdf = ExactCDF(Statistic, table);
                        double ccdf = ExactCCDF(Statistic, table);
                        PValue = System.Math.Min(cdf, ccdf) * 2;
                        break;

                    case HypothesisType.FirstIsGreaterThanSecond:
                        PValue = ExactCCDF(Statistic, table);
                        break;

                    case HypothesisType.FirstIsSmallerThanSecond:
                        PValue = ExactCDF(Statistic, table);                        
                        break;
                }                
            }
            else
            {
                double z = 0;
                if (ContinuityCorrection)
                {
                    switch (AlternativeHypothesis)
                    {
                        case HypothesisType.AreDifferent:
                            z = (Statistic > Mean)
                                ? ((Statistic - 0.5) - Mean) / StdDev
                                : ((Statistic + 0.5) - Mean) / StdDev;
                            break;

                        case HypothesisType.FirstIsGreaterThanSecond:
                            z = ((Statistic + 0.5) - Mean) / StdDev;
                            break;

                        case HypothesisType.FirstIsSmallerThanSecond:
                            z = ((Statistic - 0.5) - Mean) / StdDev;
                            break;
                    }
                }
                else
                {
                    z = (Statistic - Mean) / StdDev;
                }
                
                var normal = new Normal();
                switch (AlternativeHypothesis)
                {
                    case HypothesisType.AreDifferent:
                        double cdf = normal.CumulativeDistribution(z);
                        PValue = System.Math.Min(cdf, 1.0 - cdf) * 2.0;
                        break;

                    case HypothesisType.FirstIsGreaterThanSecond:
                        PValue = 1.0 - normal.CumulativeDistribution(z);
                        break;

                    case HypothesisType.FirstIsSmallerThanSecond:
                        PValue = normal.CumulativeDistribution(z);
                        break;
                }                
            }                                              
        }        

        private double[] InitExactDistribution(double[] ranks)
        {
            if (ranks.Length > 21)
                throw new ArgumentException("ranks > 21", "ranks");

            //int size = (int)System.Math.Pow(2, ranks.Length);
            int size = 1 << ranks.Length;
            double[] table = new double[size];
            Parallel.For(0, size, i => 
            {
                table[i] = WilcoxonSignedRankPairTest.RankSumPositive(i, ranks);
            });

            Array.Sort(table);
            return table;
        }

        private double ExactCDF(double x, double[] table)
        {
            for (int i = 0; i < table.Length; i++)
                if (x < table[i])
                    return i / (double)table.Length;
            return 1.0;
        }

        private double ExactCCDF(double x, double[] table)
        {
            for (int i = table.Length - 1; i >= 0; i--)
                if (x > table[i])
                    return (table.Length - i - 1) / (double)table.Length;
            return 1.0;
        }
        
        private static double RankSumPositive(int bitInt, double[] ranks)
        {
            double sum = 0.0;
            for (int i = 0; i < ranks.Length; i++)
            {                
                if (((bitInt >> i) & 1) == 1)
                    sum += ranks[i];
            }
            return sum;
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            switch (format)
            {
                case "DEBUG":
                    var sb = new StringBuilder();
                    sb.AppendFormat("Alpha: {0}", Alpha); sb.Append(Environment.NewLine);
                    sb.AppendFormat("p-value: {0}", PValue); sb.Append(Environment.NewLine);
                    sb.AppendFormat("{0}: {1}", AlternativeHypothesis, CanRejectNullHypothesis); sb.Append(Environment.NewLine);
                    sb.AppendFormat("Statistic value: {0}", Statistic); sb.Append(Environment.NewLine);
                    sb.AppendFormat("Effective number of samples: {0}", N); sb.Append(Environment.NewLine);                    
                    sb.AppendFormat("Mean: {0}", Mean); sb.Append(Environment.NewLine);
                    sb.AppendFormat("Variance: {0}", Variance); sb.Append(Environment.NewLine);
                    sb.AppendFormat("Std Dev: {0}", StdDev); sb.Append(Environment.NewLine);
                    sb.AppendFormat("Exact : {0}", Exact); sb.Append(Environment.NewLine);
                    sb.AppendFormat("SplitTiesEvenly : {0}", SplitTiesEvenly); sb.Append(Environment.NewLine);
                    sb.AppendFormat("ExactDistributionBasedOnRanks : {0}", ExactDistributionBasedOnRanks); sb.Append(Environment.NewLine);
                    return sb.ToString();

                case "G":
                    return this.ToString();
            }

            return this.ToString();
        }

        public override string ToString()
        {
            return "W+ for paired observations";
        }
    }
}
