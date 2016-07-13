using System;

namespace Infovision.Utils
{
    [Serializable]
    public class Pair<T1, T2>
    {
        private Pair()
        {
        }

        public Pair(T1 item1, T2 item2)
            : this()
        {
            this.Item1 = item1;
            this.Item2 = item2;
        }

        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
    };
}