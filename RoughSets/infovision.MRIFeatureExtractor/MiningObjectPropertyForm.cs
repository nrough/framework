﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Infovision.MRI.DAL;

namespace Infovision.MRI.UI
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