using System;
using System.Windows.Forms;

namespace Infovision.MRI.UI
{
    public partial class NeighbourDialog : DialogForm
    {
        public NeighbourDialog()
        {
            InitializeComponent();
        }

        public BindingSource FormBindingSource
        {
            get { return this.imageNeighbourBindingSource; }
            set { this.imageNeighbourBindingSource = value; }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {
        }

        private void NeighbourDialog_Load(object sender, EventArgs e)
        {
        }

        private void label2_Click(object sender, EventArgs e)
        {
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void dlgLabelObject_SelectedIndexChanged(object sender, EventArgs e)
        {
            DAL.ImageNeighbour imageNeighbour = this.FormBindingSource.Current as DAL.ImageNeighbour;
            if (imageNeighbour == null)
            {
                throw new InvalidOperationException("Unexpected error");
            }

            ComboBox comboBox = (ComboBox)sender;

            imageNeighbour.Labels = imageNeighbour.GetSelectedLabel((long)comboBox.SelectedValue);
        }

        private void dlgMaskObject_SelectedIndexChanged(object sender, EventArgs e)
        {
            DAL.ImageNeighbour imageNeighbour = this.FormBindingSource.Current as DAL.ImageNeighbour;
            if (imageNeighbour == null)
            {
                throw new InvalidOperationException("Unexpected error");
            }

            ComboBox comboBox = (ComboBox)sender;

            imageNeighbour.Mask = imageNeighbour.GetSelectedMask((long)comboBox.SelectedValue);
        }
    }
}