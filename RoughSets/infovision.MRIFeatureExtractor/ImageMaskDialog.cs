﻿using System;
using System.Windows.Forms;
using Raccoon.MRI.DAL;

namespace Raccoon.MRI.UI
{
    public partial class ImageMaskDialog : DialogForm
    {
        public ImageMaskDialog()
        {
            InitializeComponent();
        }

        public BindingSource ImageMaskItemsBindingSource
        {
            get { return this.imageMaskItemsBindingSource; }
        }

        public ImageMaskItems ImageMaskItems
        {
            get
            {
                ImageMaskItems maskItems = (ImageMaskItems)this.ImageMaskItemsBindingSource.List;
                return maskItems;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void label2_Click(object sender, EventArgs e)
        {
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void label3_Click(object sender, EventArgs e)
        {
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void bindingSource1_CurrentChanged(object sender, EventArgs e)
        {
        }

        private void addBtn_Click(object sender, EventArgs e)
        {
            try
            {
                this.ImageMaskItems.AddMaskItem((int)this.labelValueDlg.Value,
                                                (int)this.radiusDlg.Value);
            }
            catch
            {
            }
        }

        private void ImageMaskDialog_Load(object sender, EventArgs e)
        {
        }

        private void itemsBindingSource_CurrentChanged(object sender, EventArgs e)
        {
        }

        private void removeBtn_Click(object sender, EventArgs e)
        {
            if (this.imageMaskItemsDlg.SelectedItem != null)
            {
                this.ImageMaskItems.RemoveMaskItem(this.imageMaskItemsDlg.SelectedItem);
            }
        }
    }
}