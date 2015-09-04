using System;
using Infovision.Data;
using NUnit.Framework;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    public class ReductClassifierTest
    {
        DataStore dataStoreTrain = null;

        public ReductClassifierTest()
        {
            String trainFileName = @"playgolf.train";
            dataStoreTrain = DataStore.Load(trainFileName, FileFormat.Rses1);
        }


    }
}
