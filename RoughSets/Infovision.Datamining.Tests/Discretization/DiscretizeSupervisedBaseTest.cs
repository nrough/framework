using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Tests.Discretization
{
    public class DiscretizeSupervisedBaseTest : DiscretizeBaseTest
    {        
        protected long[] labels = {
                        1, 1, 1, 2, 2, 2, 2, 2,
                        1, 1, 1, 1, 1, 1, 1, 1,
                        2, 2, 2, 2, 2, 3, 3, 3 };        
    }
}
