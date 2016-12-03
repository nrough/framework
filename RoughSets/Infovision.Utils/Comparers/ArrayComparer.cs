using System;
using System.Collections;
using System.Collections.Generic;

namespace Infovision.Core
{
    public class ArrayComparer<T> : EqualityComparer<T[]>, IComparer<T[]>, IComparer
        where T : IComparable, IEquatable<T>
    {
        private static volatile ArrayComparer<T> arrayComparer = null;
        private static readonly object syncRoot = new object();

        public static ArrayComparer<T> Instance
        {
            get
            {
                if (arrayComparer == null)
                {
                    lock (syncRoot)
                    {
                        if (arrayComparer == null)
                        {                            
                            arrayComparer = new ArrayComparer<T>();
                        }
                    }
                }

                return arrayComparer;
            }            
        }



        public int Compare(object x, object y)
        {
            T[] a1 = x as T[];            
            T[] a2 = y as T[];

            return Compare(a1, a2);
        }

        public int Compare(T[] x, T[] y)
        {
            if (x == null && y == null)
                return 0;

            if (x == null)
                return -1;

            if (y == null)
                return 1;

            int checkLength = x.Length.CompareTo(y.Length);
            if (checkLength != 0)
                return checkLength;

            for (int i = 0; i < x.Length; i++)
            {
                int checkElement = x[i].CompareTo(y[i]);
                if (checkElement != 0)
                    return checkElement;
            }

            return 0;
        }

        public override bool Equals(T[] x, T[] y)
        {
            if (x == null && y == null)
                return true;

            if (x == null)
                return false;

            if (y == null)
                return false;

            if (x.Length != y.Length)
                return false;

            for (int i = 0; i < x.Length; i++)
                if (!x[i].Equals(y[i]))
                    return false;
          
            return true;
        }

        public override int GetHashCode(T[] obj)
        {
            return HashHelper.ArrayHashMedium(obj);
        }        
    }


}
