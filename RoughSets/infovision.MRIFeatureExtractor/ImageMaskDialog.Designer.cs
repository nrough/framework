namespace NRough.MRI.UI
{
    partial class ImageMaskDialog
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
            this.imageMaskItemsDlg = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.addBtn = new System.Windows.Forms.Button();
            this.removeBtn = new System.Windows.Forms.Button();
            this.labelValueDlg = new System.Windows.Forms.NumericUpDown();
            this.radiusDlg = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.okBtn = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.imageMaskItemsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.labelValueDlg)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radiusDlg)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imageMaskItemsBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // imageMaskItemsDlg
            // 
            this.imageMaskItemsDlg.DataSource = this.imageMaskItemsBindingSource;
            this.imageMaskItemsDlg.DisplayMember = "DisplayValue";
            this.imageMaskItemsDlg.FormattingEnabled = true;
            this.imageMaskItemsDlg.Location = new System.Drawing.Point(6, 32);
            this.imageMaskItemsDlg.Name = "imageMaskItemsDlg";
            this.imageMaskItemsDlg.Size = new System.Drawing.Size(158, 108);
            this.imageMaskItemsDlg.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Label key";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Radius";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // addBtn
            // 
            this.addBtn.Location = new System.Drawing.Point(170, 33);
            this.addBtn.Name = "addBtn";
            this.addBtn.Size = new System.Drawing.Size(75, 23);
            this.addBtn.TabIndex = 5;
            this.addBtn.Text = "Add";
            this.addBtn.UseVisualStyleBackColor = true;
            this.addBtn.Click += new System.EventHandler(this.addBtn_Click);
            // 
            // removeBtn
            // 
            this.removeBtn.Location = new System.Drawing.Point(170, 62);
            this.removeBtn.Name = "removeBtn";
            this.removeBtn.Size = new System.Drawing.Size(75, 23);
            this.removeBtn.TabIndex = 6;
            this.removeBtn.Text = "Remove";
            this.removeBtn.UseVisualStyleBackColor = true;
            this.removeBtn.Click += new System.EventHandler(this.removeBtn_Click);
            // 
            // labelValueDlg
            // 
            this.labelValueDlg.Location = new System.Drawing.Point(74, 19);
            this.labelValueDlg.Name = "labelValueDlg";
            this.labelValueDlg.Size = new System.Drawing.Size(120, 20);
            this.labelValueDlg.TabIndex = 7;
            this.labelValueDlg.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // radiusDlg
            // 
            this.radiusDlg.Location = new System.Drawing.Point(74, 46);
            this.radiusDlg.Name = "radiusDlg";
            this.radiusDlg.Size = new System.Drawing.Size(120, 20);
            this.radiusDlg.TabIndex = 8;
            this.radiusDlg.ValueChanged += new System.EventHandler(this.numericUpDown2_ValueChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.labelValueDlg);
            this.groupBox1.Controls.Add(this.radiusDlg);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(251, 19);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(225, 82);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Parameters";
            // 
            // cancelBtn
            // 
            this.cancelBtn.Location = new System.Drawing.Point(430, 172);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(75, 23);
            this.cancelBtn.TabIndex = 10;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.button3_Click);
            // 
            // okBtn
            // 
            this.okBtn.Location = new System.Drawing.Point(349, 172);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(75, 23);
            this.okBtn.TabIndex = 11;
            this.okBtn.Text = "OK";
            this.okBtn.UseVisualStyleBackColor = true;
            this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.imageMaskItemsDlg);
            this.groupBox2.Controls.Add(this.addBtn);
            this.groupBox2.Controls.Add(this.groupBox1);
            this.groupBox2.Controls.Add(this.removeBtn);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(493, 154);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Mask regions";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // imageMaskItemsBindingSource
            // 
            this.imageMaskItemsBindingSource.DataSource = typeof(NRough.MRI.DAL.ImageMaskItems);
            // 
            // ImageMaskDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(520, 208);
            this.Controls.Add(this.okBtn);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.groupBox2);
            this.Name = "ImageMaskDialog";
            this.Text = "Image mask";
            this.Load += new System.EventHandler(this.ImageMaskDialog_Load);
            ((System.ComponentModel.ISupportInitialize)(this.labelValueDlg)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radiusDlg)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imageMaskItemsBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox imageMaskItemsDlg;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button addBtn;
        private System.Windows.Forms.Button removeBtn;
        private System.Windows.Forms.NumericUpDown labelValueDlg;
        private System.Windows.Forms.NumericUpDown radiusDlg;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.Button okBtn;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.BindingSource imageMaskItemsBindingSource;
    }
}