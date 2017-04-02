using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Evaluation.HypothesisTesting
{
    //TODO MannWhitneyTest
    public class MannWhitneyTest
    {
        public void Calculate(double[] serie1, double[] serie2)
        {
            if (serie1 == null)
                throw new ArgumentNullException("serie1");
            if (serie2 == null)
                throw new ArgumentNullException("serie2");
            if (serie1.Length == 0)
                throw new ArgumentException("serie1.Length == 0", "serie1");
            if (serie2.Length == 0)
                throw new ArgumentException("serie2.Length == 0", "serie2");
            //if (serie1.Length != serie2.Length)
            //    throw new ArgumentException("serie1.Length != serie2.Length", "serie2");

            double[] difference = new double[serie1.Length];
            for (int i = 0; i < difference.Length; i++)
                difference[i] = serie2[i] - serie1[i];

            var ranks = difference.Select((d, i) => new { Value = d, Index = i })
                .OrderBy(x => System.Math.Abs(x.Value))
                .Select((x, i) => new { Value = x.Value, Index = x.Index, RankTmp = i + 1 })
                .GroupBy(x => System.Math.Abs(x.Value))
                .SelectMany(g => g.Select(e => new { Value = e.Value, Index = e.Index, Rank = g.Average(s => s.RankTmp) }))
                .OrderBy(x => x.Index).Select(x => x.Rank).ToArray();

            double rPlus, rMinus;
            if (difference.Count(x => x == 0) % 2 == 0)
            {
                rPlus = ranks.Where((v, i) => difference[i] > 0).Sum()
                    + 0.5 * ranks.Where((v, i) => difference[i] == 0).Sum();

                rMinus = ranks.Where((v, i) => difference[i] < 0).Sum()
                    + 0.5 * ranks.Where((v, i) => difference[i] == 0).Sum();
            }
            else
            {
                rPlus = ranks.Where((v, i) => difference[i] > 0).Sum()
                    + 0.5 * ranks.Where((v, i) => difference[i] == 0).Sum()
                    - ranks[0];

                rMinus = ranks.Where((v, i) => difference[i] < 0).Sum()
                    + 0.5 * ranks.Where((v, i) => difference[i] == 0).Sum()
                    - ranks[0];
            }

            double T = System.Math.Min(rPlus, rMinus);
            double W = T;
            double N = serie1.Length;
            double statistic = (T - (0.25 * N * (N + 1.0))) / System.Math.Sqrt((1.0 / 24.0) * N * (N + 1.0) * (2.0 * N + 1));
        }
    }
}
