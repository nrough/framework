using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using NUnit.Framework;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    public class DecisionTreeTest
    {
        [Test]
        public void ID3LearnTest()
        {
            DataStore data = DataStore.Load(@"Data\playgolf.train", FileFormat.Rses1);
            DecisionTreeID3 tree = new DecisionTreeID3();
            tree.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
            DecisionTreeNode node = tree.Root;
        }

        [Test]
        public void C45LearnTest()
        {
            DataStore data = DataStore.Load(@"Data\playgolf.train", FileFormat.Rses1);
            
            DecisionTreeC45 treeC45 = new DecisionTreeC45();
            treeC45.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
            DecisionTreeNode nodeC45 = treeC45.Root;

            DecisionTreeID3 treeID3 = new DecisionTreeID3();
            treeID3.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
            DecisionTreeNode nodeID3 = treeID3.Root;
        }
    }
}
