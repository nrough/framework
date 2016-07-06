using System;

namespace Infovision.Utils
{
    [Serializable]
    public class Pair<TFirst, TSecond>
    {
        public Pair()
        {
        }

        public Pair(TFirst first, TSecond second)
        {
            this.First = first;
            this.Second = second;
        }

        public TFirst First { get; set; }
        public TSecond Second { get; set; }
    };
}