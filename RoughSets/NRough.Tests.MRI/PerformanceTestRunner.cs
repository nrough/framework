using System;

namespace NRough.MRI.UnitTests
{
    public class PerformanceTestRunner
    {
        [STAThread]
        public static void Main(string[] args)
        {
            FeatureExtractorTest testClass = new FeatureExtractorTest();
            testClass.FeatureGroupExtractor();
        }
    }
}