using System;
using System.Windows.Forms;
using NRough.MRI.DAL;

namespace NRough.MRI.UI
{
    public partial class SOMClusteringDialog : DialogForm
    {
        public SOMClusteringDialog()
        {
            InitializeComponent();
        }

        public BindingSource SOMClusteringBindingSource
        {
            get { return this.sOMClusteringBindingSource; }
            set { this.sOMClusteringBindingSource = value; }
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

        private void groupBox1_Enter(object sender, EventArgs e)
        {
        }

        private void SOMClusteringDialog_Load(object sender, EventArgs e)
        {
            DAL.SOMClustering somClustering = this.SOMClusteringBindingSource.Current as DAL.SOMClustering;
            if (somClustering == null)
            {
                throw new InvalidOperationException("Unexpected error");
            }

            this.dlgSelectedInputs.DataSource = somClustering.SelectedObjects;
            this.dlgSelectedInputs.DisplayMember = "Name";

            this.dlgAvailableInputs.DataSource = somClustering.AvailableObjects;
            this.dlgAvailableInputs.DisplayMember = "Name";

            EnableFields();
        }

        private void addBtn_Click(object sender, EventArgs e)
        {
            if (this.dlgAvailableInputs.SelectedItem != null)
            {
                MiningObjectDisplay selectedItem = this.dlgAvailableInputs.SelectedItem as MiningObjectDisplay;
                if (selectedItem != null)
                {
                    DAL.SOMClustering somClustering = this.SOMClusteringBindingSource.Current as DAL.SOMClustering;
                    if (somClustering == null)
                    {
                        throw new InvalidOperationException("Unexpected error");
                    }

                    somClustering.AddSelectedObject(selectedItem);
                    somClustering.RemoveAvailableObject(selectedItem);
                }
            }
        }

        private void removeBtn_Click(object sender, EventArgs e)
        {
            if (this.dlgSelectedInputs.SelectedItem != null)
            {
                MiningObjectDisplay selectedItem = this.dlgSelectedInputs.SelectedItem as MiningObjectDisplay;
                if (selectedItem != null)
                {
                    DAL.SOMClustering somClustering = this.SOMClusteringBindingSource.Current as DAL.SOMClustering;
                    if (somClustering == null)
                    {
                        throw new InvalidOperationException("Unexpected error");
                    }

                    somClustering.AddAvailableObject(selectedItem);
                    somClustering.RemoveSelectedObject(selectedItem);
                }
            }
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            fileDialog.RestoreDirectory = false;

            fileDialog.Filter = "Trained network label (*.bin)|*.bin";
            fileDialog.Filter += "|" + "All files (*.*)|*.*";

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                dlgFileNameLoad.Text = fileDialog.FileName;
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();

            fileDialog.RestoreDirectory = false;

            fileDialog.Filter = "Trained network label (*.bin)|*.bin";

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                dlgFileNameSave.Text = fileDialog.FileName;
            }
        }

        private void dlgReadFromFile_CheckedChanged(object sender, EventArgs e)
        {
            EnableFields();
        }

        private void dlgSaveToFile_CheckedChanged(object sender, EventArgs e)
        {
            EnableFields();
        }

        private void EnableFields()
        {
            dlgFileNameLoad.Enabled = dlgReadFromFile.Checked;
            btnOpenFileName.Enabled = dlgReadFromFile.Checked;

            dlgLearningRate.Enabled = !dlgReadFromFile.Checked;
            dlgNumberOfClusters.Enabled = !dlgReadFromFile.Checked;
            dlgNumberOfIterations.Enabled = !dlgReadFromFile.Checked;
            dlgRadius.Enabled = !dlgReadFromFile.Checked;
            dlgInputs.Enabled = !dlgReadFromFile.Checked;

            dlgSelectedInputs.Enabled = !dlgReadFromFile.Checked;
            dlgAvailableInputs.Enabled = !dlgReadFromFile.Checked;

            btnAdd.Enabled = !dlgReadFromFile.Checked;
            btnRemove.Enabled = !dlgReadFromFile.Checked;

            dlgFileNameSave.Enabled = dlgSaveToFile.Checked;
            btnSaveFileName.Enabled = dlgSaveToFile.Checked;
        }
    }
}