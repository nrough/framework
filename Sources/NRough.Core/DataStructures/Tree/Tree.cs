using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Core.DataStructures.Tree
{
    public interface ITree
    {
        ITreeNode Root { get; }
    }

    public class Tree
    {
        public ITreeNode Root { get; set; }
    }
}
