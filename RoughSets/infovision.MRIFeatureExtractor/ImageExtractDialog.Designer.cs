namespace Infovision.MRI.UI
{
    partial class ImageExtractDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.sliceLabelDlg = new System.Windows.Forms.Label();
            this.sliceDlg = new System.Windows.Forms.NumericUpDown();
            this.imageExtractBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.viewImageDlg = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sliceDlg)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageExtractBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.sliceLabelDlg);
            this.groupBox1.Controls.Add(this.sliceDlg);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(242, 57);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Parameters";
            // 
            // sliceLabelDlg
            // 
            this.sliceLabelDlg.AutoSize = true;
            this.sliceLabelDlg.Location = new System.Drawing.Point(6, 22);
            this.sliceLabelDlg.Name = "sliceLabelDlg";
            this.sliceLabelDlg.Size = new System.Drawing.Size(68, 13);
            this.sliceLabelDlg.TabIndex = 1;
            this.sliceLabelDlg.Text = "Slice number";
            // 
            // sliceDlg
            // 
            this.sliceDlg.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.imageExtractBindingSource, "Slice", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.sliceDlg.Location = new System.Drawing.Point(80, 20);
            this.sliceDlg.Name = "sliceDlg";
            this.sliceDlg.Size = new System.Drawing.Size(120, 20);
            this.sliceDlg.TabIndex = 0;
            // 
            // imageExtractBindingSource
            // 
            this.imageExtractBindingSource.DataSource = typeof(Infovision.MRI.DAL.ImageExtract);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(180, 115);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(99, 115);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 2;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // viewImageDlg
            // 
            this.viewImageDlg.AutoSize = true;
            this.viewImageDlg.DataBindings.Add(new System.Windows.Forms.Binding("CheckState", this.imageExtractBindingSource, "ViewImage", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.viewImageDlg.Location = new System.Drawing.Point(22, 77);
            this.viewImageDlg.Name = "viewImageDlg";
            this.viewImageDlg.Size = new System.Drawing.Size(80, 17);
            this.viewImageDlg.TabIndex = 3;
            this.viewImageDlg.Text = "View image";
            this.viewImageDlg.UseVisualStyleBackColor = true;
            // 
            // ImageExtractDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(271, 150);
            this.Controls.Add(this.viewImageDlg);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.groupBox1);
            this.Name = "ImageExtractDialog";
            this.Text = "Extract image";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ImageExtractDialog_FormClosing);
            this.Load += new System.EventHandler(this.ImageExtractDialog_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sliceDlg)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageExtractBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label sliceLabelDlg;
        private System.Windows.Forms.NumericUpDown sliceDlg;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.BindingSource imageExtractBindingSource;
        private System.Windows.Forms.CheckBox viewImageDlg;
    }
}