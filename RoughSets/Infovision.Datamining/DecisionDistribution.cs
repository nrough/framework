using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Utils;

namespace Infovision.Datamining
{
    public class DecisionDistribution
    {
        private readonly object syncRoot = new object();
        private Dictionary<long, double> weightDistribution;
        private IDistibutionSelectStrategy selectStrategy;
        private long output;
        private bool isCached;

        public long Output
        {
            get
            {
                if (this.isCached)
                    return this.output;
                this.output =  selectStrategy.Select(this.weightDistribution);
                this.isCached = true;
                return this.output;
            }            
        }

        private DecisionDistribution()
        {
            this.Clear();
        }

        public DecisionDistribution(IDictionary<long, double> _distribution)
            : this()
        {
            this.weightDistribution = new Dictionary<long, double>(_distribution);
            this.selectStrategy = DistributionSelectMax.DefaultInstance;
        }

        public DecisionDistribution(IDictionary<long, double> _distribution, IDistibutionSelectStrategy strategy)
            : this()
        {
            this.weightDistribution = new Dictionary<long, double>(_distribution);
            this.selectStrategy = strategy;
        }

        private void Clear()
        {
            this.isCached = false;
            this.output = -1;
        }
    }

    public class DistributionSelectMax : IDistibutionSelectStrategy
    {
        private static object syncRoot = new object();
        private static DistributionSelectMax instance;

        private Comparer<double> comparer = Comparer<double>.Default;
        
        public static DistributionSelectMax DefaultInstance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new DistributionSelectMax();
                        }
                    }
                }

                return instance;
            }
        }

        public long Select(IDictionary<long, double> distribution)
        {
            if (distribution == null || distribution.Count == 0)
                return -1;
                        
            KeyValuePair<long, double> result = new KeyValuePair<long, double>(-1, 0.0);
            foreach (var kvp in distribution)
                if (this.comparer.Compare(kvp.Value, result.Value) > 0)
                    result = kvp;
            return result.Key;
        }
    }
}
