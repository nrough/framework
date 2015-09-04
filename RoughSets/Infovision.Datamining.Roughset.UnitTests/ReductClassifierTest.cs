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
            string trainFileName = @"Data\playgolf.train";
            dataStoreTrain = DataStore.Load(trainFileName, FileFormat.Rses1);
        }


    }
}
