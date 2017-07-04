//
// NRough Framework
// http://www.nrough.net
//
// Copyright © Sebastian Widz, 2017
// authors at nrough.net

//This file is part of NRough.

//Foobar is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 3 of the License, or
//(at your option) any later version.
//
//Foobar is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Foobar; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//
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