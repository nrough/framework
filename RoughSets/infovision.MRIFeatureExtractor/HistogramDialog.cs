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
    public partial class HistogramDialog : DialogForm
    {
        public HistogramDialog()
        {
            InitializeComponent();
        }

        public BindingSource HistogramParametersBindingSource
        {
            get { return this.histogramParametersBindingSource; }
            private set { this.histogramParametersBindingSource = value; }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

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
