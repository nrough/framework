namespace Infovision.MRI.UnitTests
{
    public class PerformanceTestRunner
    {
        private static void Main(string[] args)
        {
            FeatureExtractorTest testClass = new FeatureExtractorTest();
            testClass.FeatureGroupExtractor();
        }
    }
}