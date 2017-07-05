namespace NRough.Math
{
    public static class Disimilarity
    {
        public static double Manhattan(double[] a, double[] b)
        {
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
                sum += System.Math.Exp(-System.Math.Abs(a[i] - b[i]));
            return sum;
        }

        public static double SquaredEuclidean(double[] a, double[] b)
        {
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
                sum += System.Math.Exp(-System.Math.Pow((a[i] - b[i]), 2.0));
            return sum;
        }
    }
}