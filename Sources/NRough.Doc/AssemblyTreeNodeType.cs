using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Doc
{
    public enum AssemblyTreeNodeType
    {
        None = 0,
        Assembly = 1,
        Class = 2,
        Interface = 3,
        Enum = 4,
        Delegate = 5,
        Folder = 6,
        Namespace = 7
    }
}
