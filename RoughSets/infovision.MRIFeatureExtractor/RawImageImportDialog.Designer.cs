namespace Raccoon.MRI.UI
{
    partial class RawImageImportDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.dlgEndianess = new System.Windows.Forms.ComboBox();
            this.imageBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.label4 = new System.Windows.Forms.Label();
            this.dlgPixelType = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dlgSliceTo = new System.Windows.Forms.NumericUpDown();
            this.dlgSliceFrom = new System.Windows.Forms.NumericUpDown();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dlgDepth = new System.Windows.Forms.NumericUpDown();
            this.dlgHeight = new System.Windows.Forms.NumericUpDown();
            this.dlgWidth = new System.Windows.Forms.NumericUpDown();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.dlgHeader = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.dlgFileSelectBtn = new System.Windows.Forms.Button();
            this.dlgOpenBtn = new System.Windows.Forms.Button();
            this.dlgCancelBtn = new System.Windows.Forms.Button();
            this.formErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.dlgFileName = new System.Windows.Forms.TextBox();
            this.viewImageDlg = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.imageBindingSource)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dlgSliceTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dlgSliceFrom)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dlgDepth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dlgHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dlgWidth)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dlgHeader)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.formErrorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Width";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Height";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 78);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(36, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Depth";
            // 
            // dlgEndianess
            // 
            this.dlgEndianess.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.imageBindingSource, "Endianness", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dlgEndianess.FormattingEnabled = true;
            this.dlgEndianess.Location = new System.Drawing.Point(81, 19);
            this.dlgEndianess.Name = "dlgEndianess";
            this.dlgEndianess.Size = new System.Drawing.Size(121, 21);
            this.dlgEndianess.TabIndex = 5;
            // 
            // imageBindingSource
            // 
            this.imageBindingSource.DataSource = typeof(Raccoon.MRI.DAL.ImageRead);
            this.imageBindingSource.CurrentChanged += new System.EventHandler(this.imageMetaDataBindingSource_CurrentChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Endianess";
            // 
            // dlgPixelType
            // 
            this.dlgPixelType.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.imageBindingSource, "PixelType", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dlgPixelType.FormattingEnabled = true;
            this.dlgPixelType.Location = new System.Drawing.Point(81, 47);
            this.dlgPixelType.Name = "dlgPixelType";
            this.dlgPixelType.Size = new System.Drawing.Size(121, 21);
            this.dlgPixelType.TabIndex = 6;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 51);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(52, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Pixel type";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 46);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(20, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "To";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 20);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(30, 13);
            this.label8.TabIndex = 17;
            this.label8.Text = "From";
            this.label8.Click += new System.EventHandler(this.label8_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.dlgSliceTo);
            this.groupBox1.Controls.Add(this.dlgSliceFrom);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Location = new System.Drawing.Point(7, 151);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(212, 75);
            this.groupBox1.TabIndex = 18;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Slice range";
            // 
            // dlgSliceTo
            // 
            this.dlgSliceTo.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.imageBindingSource, "SliceTo", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dlgSliceTo.Location = new System.Drawing.Point(59, 45);
            this.dlgSliceTo.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.dlgSliceTo.Name = "dlgSliceTo";
            this.dlgSliceTo.Size = new System.Drawing.Size(120, 20);
            this.dlgSliceTo.TabIndex = 9;
            // 
            // dlgSliceFrom
            // 
            this.dlgSliceFrom.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.imageBindingSource, "SliceFrom", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dlgSliceFrom.Location = new System.Drawing.Point(59, 18);
            this.dlgSliceFrom.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.dlgSliceFrom.Name = "dlgSliceFrom";
            this.dlgSliceFrom.Size = new System.Drawing.Size(120, 20);
            this.dlgSliceFrom.TabIndex = 8;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.dlgDepth);
            this.groupBox2.Controls.Add(this.dlgHeight);
            this.groupBox2.Controls.Add(this.dlgWidth);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(7, 38);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(212, 107);
            this.groupBox2.TabIndex = 20;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Image dimensions";
            // 
            // dlgDepth
            // 
            this.dlgDepth.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.imageBindingSource, "Depth", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dlgDepth.Location = new System.Drawing.Point(59, 76);
            this.dlgDepth.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.dlgDepth.Name = "dlgDepth";
            this.dlgDepth.Size = new System.Drawing.Size(120, 20);
            this.dlgDepth.TabIndex = 4;
            this.dlgDepth.Enter += new System.EventHandler(this.dlgDepth_Enter);
            // 
            // dlgHeight
            // 
            this.dlgHeight.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.imageBindingSource, "Height", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dlgHeight.Location = new System.Drawing.Point(59, 48);
            this.dlgHeight.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.dlgHeight.Name = "dlgHeight";
            this.dlgHeight.Size = new System.Drawing.Size(120, 20);
            this.dlgHeight.TabIndex = 3;
            this.dlgHeight.Enter += new System.EventHandler(this.dlgHeight_Enter);
            // 
            // dlgWidth
            // 
            this.dlgWidth.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.imageBindingSource, "Width", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dlgWidth.Location = new System.Drawing.Point(59, 21);
            this.dlgWidth.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.dlgWidth.Name = "dlgWidth";
            this.dlgWidth.Size = new System.Drawing.Size(120, 20);
            this.dlgWidth.TabIndex = 2;
            this.dlgWidth.Enter += new System.EventHandler(this.dlgWidth_Enter);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.dlgHeader);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.dlgEndianess);
            this.groupBox3.Controls.Add(this.dlgPixelType);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Location = new System.Drawing.Point(227, 39);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(235, 107);
            this.groupBox3.TabIndex = 21;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "File format";
            // 
            // dlgHeader
            // 
            this.dlgHeader.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.imageBindingSource, "Header", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dlgHeader.Location = new System.Drawing.Point(81, 75);
            this.dlgHeader.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.dlgHeader.Name = "dlgHeader";
            this.dlgHeader.Size = new System.Drawing.Size(120, 20);
            this.dlgHeader.TabIndex = 7;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 78);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(42, 13);
            this.label9.TabIndex = 14;
            this.label9.Text = "Header";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(13, 15);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(49, 13);
            this.label6.TabIndex = 23;
            this.label6.Text = "Filename";
            // 
            // dlgFileSelectBtn
            // 
            this.dlgFileSelectBtn.Location = new System.Drawing.Point(435, 10);
            this.dlgFileSelectBtn.Name = "dlgFileSelectBtn";
            this.dlgFileSelectBtn.Size = new System.Drawing.Size(27, 23);
            this.dlgFileSelectBtn.TabIndex = 0;
            this.dlgFileSelectBtn.Text = "...";
            this.dlgFileSelectBtn.UseVisualStyleBackColor = true;
            this.dlgFileSelectBtn.Click += new System.EventHandler(this.button1_Click);
            // 
            // dlgOpenBtn
            // 
            this.dlgOpenBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.dlgOpenBtn.Location = new System.Drawing.Point(305, 203);
            this.dlgOpenBtn.Name = "dlgOpenBtn";
            this.dlgOpenBtn.Size = new System.Drawing.Size(75, 23);
            this.dlgOpenBtn.TabIndex = 10;
            this.dlgOpenBtn.Text = "Open";
            this.dlgOpenBtn.UseVisualStyleBackColor = true;
            this.dlgOpenBtn.Click += new System.EventHandler(this.dlgOpenBtn_Click);
            // 
            // dlgCancelBtn
            // 
            this.dlgCancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.dlgCancelBtn.Location = new System.Drawing.Point(387, 203);
            this.dlgCancelBtn.Name = "dlgCancelBtn";
            this.dlgCancelBtn.Size = new System.Drawing.Size(75, 23);
            this.dlgCancelBtn.TabIndex = 11;
            this.dlgCancelBtn.Text = "Cancel";
            this.dlgCancelBtn.UseVisualStyleBackColor = true;
            this.dlgCancelBtn.Click += new System.EventHandler(this.dialogCancelBtn_Click);
            // 
            // formErrorProvider
            // 
            this.formErrorProvider.ContainerControl = this;
            // 
            // dlgFileName
            // 
            this.dlgFileName.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.imageBindingSource, "FileName", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dlgFileName.Location = new System.Drawing.Point(68, 12);
            this.dlgFileName.Name = "dlgFileName";
            this.dlgFileName.Size = new System.Drawing.Size(361, 20);
            this.dlgFileName.TabIndex = 1;
            this.dlgFileName.TextChanged += new System.EventHandler(this.dlgFileName_TextChanged);
            // 
            // viewImageDlg
            // 
            this.viewImageDlg.AutoSize = true;
            this.viewImageDlg.DataBindings.Add(new System.Windows.Forms.Binding("CheckState", this.imageBindingSource, "ViewImage", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.viewImageDlg.Location = new System.Drawing.Point(236, 170);
            this.viewImageDlg.Name = "viewImageDlg";
            this.viewImageDlg.Size = new System.Drawing.Size(80, 17);
            this.viewImageDlg.TabIndex = 24;
            this.viewImageDlg.Text = "View image";
            this.viewImageDlg.UseVisualStyleBackColor = true;
            // 
            // RawImageImportDialog
            // 
            this.AcceptButton = this.dlgOpenBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.dlgCancelBtn;
            this.ClientSize = new System.Drawing.Size(468, 230);
            this.Controls.Add(this.viewImageDlg);
            this.Controls.Add(this.dlgFileName);
            this.Controls.Add(this.dlgCancelBtn);
            this.Controls.Add(this.dlgOpenBtn);
            this.Controls.Add(this.dlgFileSelectBtn);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox3);
            this.Name = "RawImageImportDialog";
            this.Text = "Image import";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RawImageImportForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.RawImageImportForm_FormClosed);
            this.Load += new System.EventHandler(this.RawImageImportForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.imageBindingSource)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dlgSliceTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dlgSliceFrom)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dlgDepth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dlgHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dlgWidth)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dlgHeader)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.formErrorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox dlgEndianess;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox dlgPixelType;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button dlgFileSelectBtn;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button dlgOpenBtn;
        private System.Windows.Forms.Button dlgCancelBtn;
        private System.Windows.Forms.BindingSource imageBindingSource;
        private System.Windows.Forms.ErrorProvider formErrorProvider;
        private System.Windows.Forms.NumericUpDown dlgWidth;
        private System.Windows.Forms.NumericUpDown dlgSliceTo;
        private System.Windows.Forms.NumericUpDown dlgSliceFrom;
        private System.Windows.Forms.NumericUpDown dlgDepth;
        private System.Windows.Forms.NumericUpDown dlgHeight;
        private System.Windows.Forms.NumericUpDown dlgHeader;
        private System.Windows.Forms.TextBox dlgFileName;
        private System.Windows.Forms.CheckBox viewImageDlg;
    }
}