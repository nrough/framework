using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Infovision.MRI.UI
{
    public partial class ImageEdgeDialog : DialogForm
    {
        public ImageEdgeDialog()
        {
            InitializeComponent();
        }

        public BindingSource ImageEdgeBindingSource
        {
            get { return this.imageEdgeBindingSource; }
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
