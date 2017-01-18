using System;
using System.Windows.Forms;
using Raccoon.MRI.DAL;

namespace Raccoon.MRI.UI
{
    public partial class MiningObjectPropertyForm : Form
    {
        private IMiningObject miningObject;

        public MiningObjectPropertyForm()
        {
            InitializeComponent();
        }

        public IMiningObject MiningObject
        {
            get
            {
                return this.miningObject;
            }

            set
            {
                this.miningObject = value;
            }
        }

        private void propertyGrid1_Click(object sender, EventArgs e)
        {
        }

        private void MiningObjectPropertyForm_Load(object sender, EventArgs e)
        {
            if (this.MiningObject != null)
            {
                dlgPropertyGrid.SelectedObject = this.MiningObject;
            }
        }
    }
}