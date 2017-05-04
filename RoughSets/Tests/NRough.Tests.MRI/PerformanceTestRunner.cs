using System;

namespace NRough.Tests.MRI
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