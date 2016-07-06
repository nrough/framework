using System;
using System.Windows.Forms;

namespace Infovision.MRI.UI
{
    public partial class ImageExtractDialog : DialogForm
    {
        public ImageExtractDialog()
        {
            InitializeComponent();
        }

        public BindingSource ImageExtractBiningSource
        {
            get { return this.imageExtractBindingSource; }
        }

        private void ImageExtractDialog_Load(object sender, EventArgs e)
        {
            this.sliceDlg.Value = Properties.Settings.Default.ImageExtractSliceId;
            this.viewImageDlg.Checked = Properties.Settings.Default.ImageExtractViewImage;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void ImageExtractDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.OK)
            {
                Properties.Settings.Default.ImageExtractSliceId = (int)this.sliceDlg.Value;
                Properties.Settings.Default.ImageExtractViewImage = this.viewImageDlg.Checked;
                Properties.Settings.Default.Save();
            }
        }
    }
}