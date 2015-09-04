using System;

namespace Infovision.Utils
{
    public class RandomSingleton
    {
        private static RandomSingleton randomSingleton = null;
        private static object syncRoot = new object();
        private static int? seed;
        private Random systemRandom;

        private RandomSingleton()
        {
            this.systemRandom = new Random();
        }

        private RandomSingleton(int seed)
        {
            this.systemRandom = new Random((int)seed);
        }

        public static int? Seed
        {
            get { return RandomSingleton.seed; }
            set { RandomSingleton.seed = value; }
        }

        public Random SystemRandom
        {
            get { return this.systemRandom; }
        }

        public static Random Random
        {
            get
            {
                if (randomSingleton == null)
                {
                    lock (syncRoot)
                    {
                        if (randomSingleton == null)
                        {
                            randomSingleton = Seed != null ? new RandomSingleton((int) Seed) : new RandomSingleton();
                        }
                    }
                }

                return randomSingleton.SystemRandom;
            }
        }
    }
}
