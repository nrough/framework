using NRough.MachineLearning.Experimenter.Parms;
using NUnit.Framework;

namespace NRough.Tests.MachineLearning.Experimenter
{
    [TestFixture]
    internal class ParmExclusionFixture
    {
        [Test]
        public void CreateExclusionList()
        {
            //Console.WriteLine("*** CreateExclusionList ****");

            IParameter parmA = new ParameterValueCollection<int>("A", new int[] { 1, 2, 3 });
            IParameter parmB = new ParameterValueCollection<int>("B", new int[] { 1, 2, 3 });
            IParameter parmC = new ParameterValueCollection<int>("C", new int[] { 1, 2, 3 });

            IParameter parmAx1 = new ParameterValueCollection<int>("Ax1", new int[] { 1 });
            IParameter parmBx1 = new ParameterValueCollection<int>("Bx1", new int[] { 1 });
            IParameter parmCx1 = new ParameterValueCollection<int>("Cx1", new int[] { 1 });

            IParameter parmAx2 = new ParameterValueCollection<int>("Ax2", new int[] { 1 });
            IParameter parmCx2 = new ParameterValueCollection<int>("Cx2", new int[] { 3 });

            IParameter parmAx3 = new ParameterValueCollection<int>("Ax3", new int[] { 2 });

            ParameterCollection parmList = new ParameterCollection(new IParameter[] { parmA, parmB, parmC });

            //Add exclusions
            parmList.AddExclusion(new string[] { "A", "B", "C" }, new IParameter[] { parmAx1, parmBx1, parmCx1 });
            parmList.AddExclusion(new string[] { "A", "C" }, new IParameter[] { parmAx2, parmCx2 });
            parmList.AddExclusion(new string[] { "A" }, new IParameter[] { parmAx3 });

            int i = 0;
            foreach (object[] parms in parmList.Values())
            {
                //Console.WriteLine("{0} {1} {2}", parms[0], parms[1], parms[2]);
                i++;
            }

            Assert.AreEqual(14, i);
        }

        [Test]
        public void CreateExclusionListOnRanges()
        {
            //Console.WriteLine("*** CreateExclusionListOnRanges ****");

            IParameter parmA = new ParameterNumericRange<int>("A", 0, 9, 1);
            IParameter parmB = new ParameterNumericRange<double>("B", 0, 0.9, 0.1);

            IParameter parmAx1 = new ParameterNumericRange<int>("Ax1", 3, 5, 1);
            IParameter parmBx2 = new ParameterNumericRange<double>("Bx1", 0.3, 0.5, 0.1);

            ParameterCollection parmList = new ParameterCollection(new IParameter[] { parmA, parmB });

            //Add exclusions
            parmList.AddExclusion(new string[] { "A" }, new IParameter[] { parmAx1 });
            parmList.AddExclusion(new string[] { "B" }, new IParameter[] { parmBx2 });

            int i = 0;
            foreach (object[] parms in parmList.Values())
            {
                //Console.WriteLine("{0} {1}", parms[0], parms[1]);
                i++;
            }

            Assert.AreEqual(49, i);
        }
    }
}