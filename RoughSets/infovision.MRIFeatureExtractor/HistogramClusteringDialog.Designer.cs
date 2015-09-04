namespace Infovision.MRI.UI
{
    partial class HistogramClusteringDialog
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
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.dlgApproximationDegree = new System.Windows.Forms.NumericUpDown();
            this.histogramClusteringBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.dlgMinClusterDistance = new System.Windows.Forms.NumericUpDown();
            this.dlgBucketCountWeight = new System.Windows.Forms.NumericUpDown();
            this.dlgHistogramBucketSize = new System.Windows.Forms.NumericUpDown();
            this.dlgMaxNumberOfClusters = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.dlgReadFromFile = new System.Windows.Forms.CheckBox();
            this.btnOpenFileName = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.dlgFileNameLoad = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.dlgSaveToFile = new System.Windows.Forms.CheckBox();
            this.dlgFileNameSave = new System.Windows.Forms.TextBox();
            this.btnSaveFileName = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dlgApproximationDegree)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.histogramClusteringBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dlgMinClusterDistance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dlgBucketCountWeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dlgHistogramBucketSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dlgMaxNumberOfClusters)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.dlgApproximationDegree);
            this.groupBox1.Controls.Add(this.dlgMinClusterDistance);
            this.groupBox1.Controls.Add(this.dlgBucketCountWeight);
            this.groupBox1.Controls.Add(this.dlgHistogramBucketSize);
            this.groupBox1.Controls.Add(this.dlgMaxNumberOfClusters);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 91);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(529, 105);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Parameters";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(273, 75);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(109, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Approximation degree";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(273, 48);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(101, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Min cluster distance";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(273, 21);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(105, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Bucket count weight";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(111, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Histogram bucket size";
            // 
            // dlgApproximationDegree
            // 
            this.dlgApproximationDegree.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.histogramClusteringBindingSource, "ApproximationDegree", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dlgApproximationDegree.DecimalPlaces = 2;
            this.dlgApproximationDegree.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.dlgApproximationDegree.Location = new System.Drawing.Point(406, 73);
            this.dlgApproximationDegree.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.dlgApproximationDegree.Name = "dlgApproximationDegree";
            this.dlgApproximationDegree.Size = new System.Drawing.Size(89, 20);
            this.dlgApproximationDegree.TabIndex = 5;
            this.dlgApproximationDegree.ValueChanged += new System.EventHandler(this.numericUpDown5_ValueChanged);
            // 
            // histogramClusteringBindingSource
            // 
            this.histogramClusteringBindingSource.DataSource = typeof(Infovision.MRI.DAL.HistogramClustering);
            // 
            // dlgMinClusterDistance
            // 
            this.dlgMinClusterDistance.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.histogramClusteringBindingSource, "MinimumClusterDistance", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dlgMinClusterDistance.DecimalPlaces = 2;
            this.dlgMinClusterDistance.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.dlgMinClusterDistance.Location = new System.Drawing.Point(406, 46);
            this.dlgMinClusterDistance.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.dlgMinClusterDistance.Name = "dlgMinClusterDistance";
            this.dlgMinClusterDistance.Size = new System.Drawing.Size(89, 20);
            this.dlgMinClusterDistance.TabIndex = 4;
            this.dlgMinClusterDistance.ValueChanged += new System.EventHandler(this.numericUpDown4_ValueChanged);
            // 
            // dlgBucketCountWeight
            // 
            this.dlgBucketCountWeight.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.histogramClusteringBindingSource, "BucketCountWeight", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dlgBucketCountWeight.DecimalPlaces = 2;
            this.dlgBucketCountWeight.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.dlgBucketCountWeight.Location = new System.Drawing.Point(406, 19);
            this.dlgBucketCountWeight.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.dlgBucketCountWeight.Name = "dlgBucketCountWeight";
            this.dlgBucketCountWeight.Size = new System.Drawing.Size(89, 20);
            this.dlgBucketCountWeight.TabIndex = 3;
            this.dlgBucketCountWeight.ValueChanged += new System.EventHandler(this.numericUpDown3_ValueChanged);
            // 
            // dlgHistogramBucketSize
            // 
            this.dlgHistogramBucketSize.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.histogramClusteringBindingSource, "HistogramBucketSize", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dlgHistogramBucketSize.Location = new System.Drawing.Point(139, 46);
            this.dlgHistogramBucketSize.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this.dlgHistogramBucketSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.dlgHistogramBucketSize.Name = "dlgHistogramBucketSize";
            this.dlgHistogramBucketSize.Size = new System.Drawing.Size(89, 20);
            this.dlgHistogramBucketSize.TabIndex = 2;
            this.dlgHistogramBucketSize.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.dlgHistogramBucketSize.ValueChanged += new System.EventHandler(this.numericUpDown2_ValueChanged);
            // 
            // dlgMaxNumberOfClusters
            // 
            this.dlgMaxNumberOfClusters.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.histogramClusteringBindingSource, "MaxNumberOfRepresentatives", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dlgMaxNumberOfClusters.Location = new System.Drawing.Point(139, 19);
            this.dlgMaxNumberOfClusters.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this.dlgMaxNumberOfClusters.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.dlgMaxNumberOfClusters.Name = "dlgMaxNumberOfClusters";
            this.dlgMaxNumberOfClusters.Size = new System.Drawing.Size(89, 20);
            this.dlgMaxNumberOfClusters.TabIndex = 1;
            this.dlgMaxNumberOfClusters.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Max number of clusters";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(466, 287);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(385, 286);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "OK";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.dlgReadFromFile);
            this.groupBox3.Controls.Add(this.btnOpenFileName);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.dlgFileNameLoad);
            this.groupBox3.Location = new System.Drawing.Point(12, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(529, 73);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Restore";
            // 
            // dlgReadFromFile
            // 
            this.dlgReadFromFile.AutoSize = true;
            this.dlgReadFromFile.Checked = true;
            this.dlgReadFromFile.CheckState = System.Windows.Forms.CheckState.Checked;
            this.dlgReadFromFile.DataBindings.Add(new System.Windows.Forms.Binding("CheckState", this.histogramClusteringBindingSource, "LoadFile", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dlgReadFromFile.Location = new System.Drawing.Point(13, 19);
            this.dlgReadFromFile.Name = "dlgReadFromFile";
            this.dlgReadFromFile.Size = new System.Drawing.Size(91, 17);
            this.dlgReadFromFile.TabIndex = 11;
            this.dlgReadFromFile.Text = "Read from file";
            this.dlgReadFromFile.UseVisualStyleBackColor = true;
            this.dlgReadFromFile.CheckedChanged += new System.EventHandler(this.dlgReadFromFile_CheckedChanged);
            // 
            // btnOpenFileName
            // 
            this.btnOpenFileName.Location = new System.Drawing.Point(454, 37);
            this.btnOpenFileName.Name = "btnOpenFileName";
            this.btnOpenFileName.Size = new System.Drawing.Size(43, 23);
            this.btnOpenFileName.TabIndex = 10;
            this.btnOpenFileName.Text = "...";
            this.btnOpenFileName.UseVisualStyleBackColor = true;
            this.btnOpenFileName.Click += new System.EventHandler(this.btnOpenFileName_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 42);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(49, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "Filename";
            // 
            // dlgFileNameLoad
            // 
            this.dlgFileNameLoad.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.histogramClusteringBindingSource, "FileNameLoad", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dlgFileNameLoad.Location = new System.Drawing.Point(108, 39);
            this.dlgFileNameLoad.Name = "dlgFileNameLoad";
            this.dlgFileNameLoad.Size = new System.Drawing.Size(333, 20);
            this.dlgFileNameLoad.TabIndex = 9;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.dlgSaveToFile);
            this.groupBox4.Controls.Add(this.dlgFileNameSave);
            this.groupBox4.Controls.Add(this.btnSaveFileName);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Location = new System.Drawing.Point(12, 202);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(529, 79);
            this.groupBox4.TabIndex = 9;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Store";
            // 
            // dlgSaveToFile
            // 
            this.dlgSaveToFile.AutoSize = true;
            this.dlgSaveToFile.Checked = true;
            this.dlgSaveToFile.CheckState = System.Windows.Forms.CheckState.Checked;
            this.dlgSaveToFile.DataBindings.Add(new System.Windows.Forms.Binding("CheckState", this.histogramClusteringBindingSource, "SaveFile", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dlgSaveToFile.Location = new System.Drawing.Point(13, 22);
            this.dlgSaveToFile.Name = "dlgSaveToFile";
            this.dlgSaveToFile.Size = new System.Drawing.Size(79, 17);
            this.dlgSaveToFile.TabIndex = 11;
            this.dlgSaveToFile.Text = "Save to file";
            this.dlgSaveToFile.UseVisualStyleBackColor = true;
            // 
            // dlgFileNameSave
            // 
            this.dlgFileNameSave.AcceptsTab = true;
            this.dlgFileNameSave.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.histogramClusteringBindingSource, "FileNameSave", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dlgFileNameSave.Location = new System.Drawing.Point(108, 42);
            this.dlgFileNameSave.Name = "dlgFileNameSave";
            this.dlgFileNameSave.Size = new System.Drawing.Size(333, 20);
            this.dlgFileNameSave.TabIndex = 9;
            // 
            // btnSaveFileName
            // 
            this.btnSaveFileName.Location = new System.Drawing.Point(454, 40);
            this.btnSaveFileName.Name = "btnSaveFileName";
            this.btnSaveFileName.Size = new System.Drawing.Size(43, 23);
            this.btnSaveFileName.TabIndex = 10;
            this.btnSaveFileName.Text = "...";
            this.btnSaveFileName.UseVisualStyleBackColor = true;
            this.btnSaveFileName.Click += new System.EventHandler(this.btnSaveFileName_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 45);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(49, 13);
            this.label7.TabIndex = 8;
            this.label7.Text = "Filename";
            // 
            // HistogramClusteringDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(553, 319);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox1);
            this.Name = "HistogramClusteringDialog";
            this.Text = "Image histogram clustering";
            this.Load += new System.EventHandler(this.HistogramClusteringDialog_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dlgApproximationDegree)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.histogramClusteringBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dlgMinClusterDistance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dlgBucketCountWeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dlgHistogramBucketSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dlgMaxNumberOfClusters)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown dlgMaxNumberOfClusters;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown dlgApproximationDegree;
        private System.Windows.Forms.NumericUpDown dlgMinClusterDistance;
        private System.Windows.Forms.NumericUpDown dlgBucketCountWeight;
        private System.Windows.Forms.NumericUpDown dlgHistogramBucketSize;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.BindingSource histogramClusteringBindingSource;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox dlgReadFromFile;
        private System.Windows.Forms.Button btnOpenFileName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox dlgFileNameLoad;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox dlgSaveToFile;
        private System.Windows.Forms.TextBox dlgFileNameSave;
        private System.Windows.Forms.Button btnSaveFileName;
        private System.Windows.Forms.Label label7;
    }
}