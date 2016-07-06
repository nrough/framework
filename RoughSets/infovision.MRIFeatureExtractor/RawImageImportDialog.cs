using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Infovision.MRI.UI
{
    public partial class RawImageImportDialog : DialogForm
    {
        internal RawImageImportDialog()
        {
            InitializeComponent();

            dlgEndianess.DataSource = Enum.GetValues(typeof(Endianness));
            dlgPixelType.DataSource = Enum.GetValues(typeof(PixelType));
        }

        public BindingSource ImageBindingSource
        {
            get { return imageBindingSource; }
            private set { this.imageBindingSource = value; }
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void RawImageImportForm_Load(object sender, EventArgs e)
        {
            this.dlgWidth.Value = Properties.Settings.Default.RawImageWidth;
            this.dlgHeight.Value = Properties.Settings.Default.RawImageHeight;
            this.dlgDepth.Value = Properties.Settings.Default.RawImageDepth;

            var endiannessValue = Properties.Settings.Default.RawImageEndianness;
            Endianness localEndianness = (Endianness)Enum.Parse(typeof(Endianness), endiannessValue);
            this.dlgEndianess.SelectedItem = localEndianness;

            var pixelTypeValue = Properties.Settings.Default.RawImagePixelType;
            PixelType localPixelType = (PixelType)Enum.Parse(typeof(PixelType), pixelTypeValue);
            this.dlgPixelType.SelectedItem = localPixelType;

            this.dlgHeader.Value = Properties.Settings.Default.RawImageHeader;
            this.dlgSliceFrom.Value = Properties.Settings.Default.RawImageFromSlice;
            this.dlgSliceTo.Value = Properties.Settings.Default.RawImageToSlice;
            this.dlgFileName.Text = Properties.Settings.Default.RawImageFileName;

            this.viewImageDlg.Checked = Properties.Settings.Default.RawImageViewImage;
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            fileDialog.RestoreDirectory = false;

            fileDialog.Filter = "RAW Image (*.raw;*.rawb;*.raws)|*.raw;*.rawb;*.raws";
            fileDialog.Filter += "|" + "All files (*.*)|*.*";

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                dlgFileName.Text = fileDialog.FileName;
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {
        }

        private void maskedTextBox1_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
        }

        private void label8_Click(object sender, EventArgs e)
        {
        }

        private void dialogCancelBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void dlgOpenBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void imageMetaDataBindingSource_CurrentChanged(object sender, EventArgs e)
        {
        }

        private void dlgWidth_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
        }

        private void RawImageImportForm_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void RawImageImportForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.OK)
            {
                Properties.Settings.Default.RawImageWidth = (int)this.dlgWidth.Value;
                Properties.Settings.Default.RawImageHeight = (int)this.dlgHeight.Value;
                Properties.Settings.Default.RawImageDepth = (int)this.dlgDepth.Value;

                Endianness localEndianness = (Endianness)this.dlgEndianess.SelectedItem;
                Properties.Settings.Default.RawImageEndianness = localEndianness.ToString();

                PixelType localPixelType = (PixelType)this.dlgPixelType.SelectedItem;
                Properties.Settings.Default.RawImagePixelType = localPixelType.ToString();

                Properties.Settings.Default.RawImageHeader = (int)this.dlgHeader.Value;
                Properties.Settings.Default.RawImageFromSlice = (int)this.dlgSliceFrom.Value;
                Properties.Settings.Default.RawImageToSlice = (int)this.dlgSliceTo.Value;
                Properties.Settings.Default.RawImageFileName = this.dlgFileName.Text;

                Properties.Settings.Default.RawImageViewImage = this.viewImageDlg.Checked;

                Properties.Settings.Default.Save();
            }
        }

        private void dlgWidth_Enter(object sender, EventArgs e)
        {
            dlgWidth.Select(0, 10);
        }

        private void dlgHeight_Enter(object sender, EventArgs e)
        {
            dlgHeight.Select(0, 10);
        }

        private void dlgDepth_Enter(object sender, EventArgs e)
        {
            dlgDepth.Select(0, 10);
        }

        private void dlgFileName_TextChanged(object sender, EventArgs e)
        {
        }
    }
}