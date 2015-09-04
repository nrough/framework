namespace Infovision.MRI.UI
{
    partial class ImageEdgeDialog
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
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.noiseDlg = new System.Windows.Forms.NumericUpDown();
            this.imageEdgeBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.foregroundDlg = new System.Windows.Forms.NumericUpDown();
            this.backgroundDlg = new System.Windows.Forms.NumericUpDown();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.okBtn = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.noiseDlg)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageEdgeBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.foregroundDlg)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.backgroundDlg)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.noiseDlg);
            this.groupBox1.Controls.Add(this.foregroundDlg);
            this.groupBox1.Controls.Add(this.backgroundDlg);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(259, 113);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Parameters";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 76);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Noise level";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Foreground value";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Background value";
            // 
            // noiseDlg
            // 
            this.noiseDlg.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.imageEdgeBindingSource, "Noise", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.noiseDlg.DecimalPlaces = 2;
            this.noiseDlg.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.noiseDlg.Location = new System.Drawing.Point(120, 74);
            this.noiseDlg.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.noiseDlg.Name = "noiseDlg";
            this.noiseDlg.Size = new System.Drawing.Size(120, 20);
            this.noiseDlg.TabIndex = 2;
            // 
            // imageEdgeBindingSource
            // 
            this.imageEdgeBindingSource.DataSource = typeof(Infovision.MRI.DAL.ImageEdge);
            // 
            // foregroundDlg
            // 
            this.foregroundDlg.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.imageEdgeBindingSource, "Foreground", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.foregroundDlg.Location = new System.Drawing.Point(120, 47);
            this.foregroundDlg.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.foregroundDlg.Name = "foregroundDlg";
            this.foregroundDlg.Size = new System.Drawing.Size(120, 20);
            this.foregroundDlg.TabIndex = 1;
            // 
            // backgroundDlg
            // 
            this.backgroundDlg.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.imageEdgeBindingSource, "Background", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.backgroundDlg.Location = new System.Drawing.Point(120, 20);
            this.backgroundDlg.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.backgroundDlg.Name = "backgroundDlg";
            this.backgroundDlg.Size = new System.Drawing.Size(120, 20);
            this.backgroundDlg.TabIndex = 0;
            // 
            // cancelBtn
            // 
            this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelBtn.Location = new System.Drawing.Point(197, 139);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(75, 23);
            this.cancelBtn.TabIndex = 1;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
            // 
            // okBtn
            // 
            this.okBtn.Location = new System.Drawing.Point(116, 139);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(75, 23);
            this.okBtn.TabIndex = 2;
            this.okBtn.Text = "OK";
            this.okBtn.UseVisualStyleBackColor = true;
            this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
            // 
            // ImageEdgeDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelBtn;
            this.ClientSize = new System.Drawing.Size(282, 174);
            this.Controls.Add(this.okBtn);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.groupBox1);
            this.Name = "ImageEdgeDialog";
            this.Text = "Edge detection";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.noiseDlg)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageEdgeBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.foregroundDlg)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.backgroundDlg)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown noiseDlg;
        private System.Windows.Forms.NumericUpDown foregroundDlg;
        private System.Windows.Forms.NumericUpDown backgroundDlg;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.Button okBtn;
        private System.Windows.Forms.BindingSource imageEdgeBindingSource;
    }
}