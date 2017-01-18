namespace Raccoon.MRI.UI
{
    partial class HistogramDialog
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
            this.sliceFromDlg = new System.Windows.Forms.NumericUpDown();
            this.histogramParametersBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.sliceToDlg = new System.Windows.Forms.NumericUpDown();
            this.bucketSizeDlg = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.bucketSizeLbl = new System.Windows.Forms.Label();
            this.sliceRangeGrp = new System.Windows.Forms.GroupBox();
            this.histogramParmGrp = new System.Windows.Forms.GroupBox();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.okBtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.sliceFromDlg)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.histogramParametersBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sliceToDlg)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bucketSizeDlg)).BeginInit();
            this.sliceRangeGrp.SuspendLayout();
            this.histogramParmGrp.SuspendLayout();
            this.SuspendLayout();
            // 
            // sliceFromDlg
            // 
            this.sliceFromDlg.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.histogramParametersBindingSource, "SliceFrom", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.sliceFromDlg.Location = new System.Drawing.Point(91, 19);
            this.sliceFromDlg.Name = "sliceFromDlg";
            this.sliceFromDlg.Size = new System.Drawing.Size(120, 20);
            this.sliceFromDlg.TabIndex = 0;
            // 
            // histogramParametersBindingSource
            // 
            this.histogramParametersBindingSource.DataSource = typeof(Raccoon.MRI.DAL.Histogram);
            // 
            // sliceToDlg
            // 
            this.sliceToDlg.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.histogramParametersBindingSource, "SliceTo", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.sliceToDlg.Location = new System.Drawing.Point(91, 46);
            this.sliceToDlg.Name = "sliceToDlg";
            this.sliceToDlg.Size = new System.Drawing.Size(120, 20);
            this.sliceToDlg.TabIndex = 1;
            // 
            // bucketSizeDlg
            // 
            this.bucketSizeDlg.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.histogramParametersBindingSource, "BucketSize", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.bucketSizeDlg.Location = new System.Drawing.Point(91, 28);
            this.bucketSizeDlg.Name = "bucketSizeDlg";
            this.bucketSizeDlg.Size = new System.Drawing.Size(120, 20);
            this.bucketSizeDlg.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "From";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(20, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "To";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // bucketSizeLbl
            // 
            this.bucketSizeLbl.AutoSize = true;
            this.bucketSizeLbl.Location = new System.Drawing.Point(18, 30);
            this.bucketSizeLbl.Name = "bucketSizeLbl";
            this.bucketSizeLbl.Size = new System.Drawing.Size(62, 13);
            this.bucketSizeLbl.TabIndex = 6;
            this.bucketSizeLbl.Text = "Bucket size";
            // 
            // sliceRangeGrp
            // 
            this.sliceRangeGrp.Controls.Add(this.sliceFromDlg);
            this.sliceRangeGrp.Controls.Add(this.sliceToDlg);
            this.sliceRangeGrp.Controls.Add(this.label2);
            this.sliceRangeGrp.Controls.Add(this.label1);
            this.sliceRangeGrp.Location = new System.Drawing.Point(12, 12);
            this.sliceRangeGrp.Name = "sliceRangeGrp";
            this.sliceRangeGrp.Size = new System.Drawing.Size(241, 83);
            this.sliceRangeGrp.TabIndex = 7;
            this.sliceRangeGrp.TabStop = false;
            this.sliceRangeGrp.Text = "Slice range";
            // 
            // histogramParmGrp
            // 
            this.histogramParmGrp.Controls.Add(this.bucketSizeDlg);
            this.histogramParmGrp.Controls.Add(this.bucketSizeLbl);
            this.histogramParmGrp.Location = new System.Drawing.Point(12, 101);
            this.histogramParmGrp.Name = "histogramParmGrp";
            this.histogramParmGrp.Size = new System.Drawing.Size(241, 64);
            this.histogramParmGrp.TabIndex = 8;
            this.histogramParmGrp.TabStop = false;
            this.histogramParmGrp.Text = "Histogram parameters";
            // 
            // cancelBtn
            // 
            this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelBtn.Location = new System.Drawing.Point(182, 180);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(75, 23);
            this.cancelBtn.TabIndex = 9;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
            // 
            // okBtn
            // 
            this.okBtn.Location = new System.Drawing.Point(101, 179);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(75, 23);
            this.okBtn.TabIndex = 10;
            this.okBtn.Text = "OK";
            this.okBtn.UseVisualStyleBackColor = true;
            this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
            // 
            // HistogramDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelBtn;
            this.ClientSize = new System.Drawing.Size(269, 215);
            this.Controls.Add(this.okBtn);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.histogramParmGrp);
            this.Controls.Add(this.sliceRangeGrp);
            this.Name = "HistogramDialog";
            this.Text = "Image histogram";
            ((System.ComponentModel.ISupportInitialize)(this.sliceFromDlg)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.histogramParametersBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sliceToDlg)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bucketSizeDlg)).EndInit();
            this.sliceRangeGrp.ResumeLayout(false);
            this.sliceRangeGrp.PerformLayout();
            this.histogramParmGrp.ResumeLayout(false);
            this.histogramParmGrp.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NumericUpDown sliceFromDlg;
        private System.Windows.Forms.NumericUpDown sliceToDlg;
        private System.Windows.Forms.NumericUpDown bucketSizeDlg;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label bucketSizeLbl;
        private System.Windows.Forms.GroupBox sliceRangeGrp;
        private System.Windows.Forms.GroupBox histogramParmGrp;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.Button okBtn;
        private System.Windows.Forms.BindingSource histogramParametersBindingSource;
    }
}