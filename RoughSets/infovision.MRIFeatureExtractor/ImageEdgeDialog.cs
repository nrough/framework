using System;
using System.Windows.Forms;

namespace Raccoon.MRI.UI
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