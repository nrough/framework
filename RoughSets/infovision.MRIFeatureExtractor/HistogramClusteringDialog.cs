using System;
using System.Windows.Forms;

namespace NRough.MRI.UI
{
    public partial class HistogramClusteringDialog : DialogForm
    {
        public HistogramClusteringDialog()
        {
            InitializeComponent();
        }

        public BindingSource HistogramClusteringBindingSource
        {
            get { return this.histogramClusteringBindingSource; }
            private set { this.histogramClusteringBindingSource = value; }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
        }

        private void label4_Click(object sender, EventArgs e)
        {
        }

        private void label3_Click(object sender, EventArgs e)
        {
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
        }

        private void label5_Click(object sender, EventArgs e)
        {
        }

        private void btnOpenFileName_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            fileDialog.RestoreDirectory = false;

            fileDialog.Filter = "Histogram cluster label (*.bin)|*.bin";
            fileDialog.Filter += "|" + "All files (*.*)|*.*";

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                dlgFileNameLoad.Text = fileDialog.FileName;
            }
        }

        private void btnSaveFileName_Click(object sender, EventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();

            fileDialog.RestoreDirectory = false;

            fileDialog.Filter = "Histogram cluster label (*.bin)|*.bin";

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                dlgFileNameSave.Text = fileDialog.FileName;
            }
        }

        private void dlgReadFromFile_CheckedChanged(object sender, EventArgs e)
        {
            EnableFields();
        }

        private void EnableFields()
        {
            dlgFileNameLoad.Enabled = dlgReadFromFile.Checked;
            btnOpenFileName.Enabled = dlgReadFromFile.Checked;

            dlgMaxNumberOfClusters.Enabled = !dlgReadFromFile.Checked;
            dlgHistogramBucketSize.Enabled = !dlgReadFromFile.Checked;
            dlgBucketCountWeight.Enabled = !dlgReadFromFile.Checked;
            dlgMinClusterDistance.Enabled = !dlgReadFromFile.Checked;
            dlgApproximationDegree.Enabled = !dlgReadFromFile.Checked;

            dlgFileNameSave.Enabled = dlgSaveToFile.Checked;
            btnSaveFileName.Enabled = dlgSaveToFile.Checked;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
        }

        private void HistogramClusteringDialog_Load(object sender, EventArgs e)
        {
            EnableFields();
        }
    }
}