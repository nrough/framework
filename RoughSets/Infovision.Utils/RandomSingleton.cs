using System;

namespace Infovision.Utils
{
    public class RandomSingleton
    {
        private static RandomSingleton randomSingleton = null;
        private static object syncRoot = new object();

        private Random systemRandom;

        private RandomSingleton()
        {
            this.systemRandom = new Random();
        }

        private RandomSingleton(int seed)
        {
            this.systemRandom = new Random(seed);
        }

        public static int Seed
        {
            get;
            set;
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
                            randomSingleton = Seed > 0 ? new RandomSingleton(Seed) : new RandomSingleton();
                        }
                    }
                }

                return randomSingleton.SystemRandom;
            }
        }
    }
}
