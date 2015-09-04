using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Infovision.MRI.UI
{
    public class DialogForm : Form
    {
    
        public DialogForm()
            : base()
        {
            this.TopMost = true;
            this.StartPosition = FormStartPosition.CenterParent;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // DialogForm
            // 
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Name = "DialogForm";
            this.ResumeLayout(false);

        }
    }
}
