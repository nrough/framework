using System;

namespace NRough.Core.Random
{
    public class RandomSingleton
    {
        private static volatile RandomSingleton randomSingleton = null;
        private static readonly object syncRoot = new object();
        private static int? seed;
        private ThreadSafeRandom systemRandom;

        private RandomSingleton()
        {
            this.systemRandom = new ThreadSafeRandom();
        }

        private RandomSingleton(int seed)
        {
            this.systemRandom = new ThreadSafeRandom(seed);
        }

        public static int? Seed
        {
            get { return RandomSingleton.seed; }
            set { RandomSingleton.seed = value; }
        }

        public static ThreadSafeRandom Random
        {
            get
            {
                if (randomSingleton == null)
                {
                    lock (syncRoot)
                    {
                        if (randomSingleton == null)
                        {
                            randomSingleton = (seed != null)
                                            ? new RandomSingleton((int)seed)
                                            : new RandomSingleton();
                        }
                    }
                }

                return randomSingleton.systemRandom;
            }
        }
    }

    public sealed class ThreadSafeRandom : System.Random
    {
        private object _lock = new object();

        public ThreadSafeRandom()
            : base()
        {
        }

        public ThreadSafeRandom(int seed)
            : base(seed)
        {
        }

        public override int Next()
        {
            lock (_lock) return base.Next();
        }

        public override int Next(int maxValue)
        {
            lock (_lock) return base.Next(maxValue);
        }

        public override int Next(int minValue, int maxValue)
        {
            lock (_lock) return base.Next(minValue, maxValue);
        }

        public override void NextBytes(byte[] buffer)
        {
            lock (_lock) base.NextBytes(buffer);
        }

        public override double NextDouble()
        {
            lock (_lock) return base.NextDouble();
        }
    }
}