using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Evaluation.HypothesisTesting
{
    //TODO WilcoxonSignedRankTest
    public class WilcoxonSignedRankTest
    {        
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

            var jointRanks = serie1
                .Select((v, k) => new { Value = v, Serie = 1 })
                .Union(serie2
                    .Select((v, k) => new { Value = v, Serie = 2 }))
                .OrderBy(x => x.Value)
                .Select((x, i) => new { Value = x.Value, Serie = x.Serie, RankTmp = i + 1 })
                .GroupBy(x => x.Value)
                .SelectMany(g => g.Select(e => new { Value = e.Value, Serie = e.Serie, Rank = g.Average(s => s.RankTmp) }));

            var ranks1 = jointRanks.Where(x => x.Serie == 1).Select(x => x.Rank);
            var ranks2 = jointRanks.Where(x => x.Serie == 2).Select(x => x.Rank);

            var W1 = ranks1.Sum();
            var W2 = ranks2.Sum();

            var U1 = W1 - (0.5 * serie1.Length * (serie1.Length + 1));
            var U2 = W2 - (0.5 * serie2.Length * (serie2.Length + 1));

            double statistic = 0.0;
            if (serie1.Length > 8 && serie2.Length > 8)
            {
                var meanU1 = 0.5 * serie1.Length * serie2.Length;
                var strDevU1 = (1.0 / 12) * (serie1.Length * serie2.Length * (serie1.Length + serie2.Length + 1));
                statistic = (U1 - meanU1) / System.Math.Sqrt(strDevU1);
            }
            else
            {
                statistic = System.Math.Min(U1, U2);
            }
           
        }
    }
}
