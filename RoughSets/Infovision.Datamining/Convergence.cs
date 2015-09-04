using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining
{
    public interface IConvergence
    {
        double NewValue { get; set; }
        double OldValue { get; set; }
        bool HasConverged { get; set; }
    }
    
    public class Convergence : IConvergence
    {
        private double newValue;
        private double oldValue;        
        
        public double NewValue 
        {
            get
            {
                return this.newValue;
            }
            set
            {
                this.oldValue = this.newValue;
                this.newValue = value;
            }
        }

        public double OldValue
        {
            get
            {
                return this.oldValue;
            }
            set
            {
                this.oldValue = value;
            }
        }

        public bool HasConverged { get; set; }
        
    }
}
