using System;

namespace NRough.Core.Random
{
    public static class RandomExtensions
    {
        public static int[] RandomVectorNoRepetition(int n, int min, int max)
        {
            int size = max - min + 1;
            if (n > size)
                throw new ArgumentException("Argument n is too big for given range.", "n");
            int[] tmp = new int[size];
            for (int i = 0; i < size; i++)
                tmp[i] = i + min;
            for (int i = 0; i < size; i++)
            {
                int k = RandomSingleton.Random.Next(0, size);
                int t = tmp[i];
                tmp[i] = tmp[k];
                tmp[k] = t;
            }

            int[] result = new int[n];
            Array.Copy(tmp, result, n);
            return result;
        }
    }
}