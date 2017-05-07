using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Doc
{
    [AttributeUsage(AttributeTargets.All, Inherited = true)]    
    public class AssemblyTreeVisibleAttribute : Attribute
    {
        public bool IsVisible { get; set; }

        public AssemblyTreeVisibleAttribute()
        {
            IsVisible = true;
        }

        public AssemblyTreeVisibleAttribute(bool isVisible)
        {
            IsVisible = isVisible;
        }
    }
}
