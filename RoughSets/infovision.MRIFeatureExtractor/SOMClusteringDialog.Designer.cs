namespace Infovision.MRI.UI
{
    partial class SOMClusteringDialog
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
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.dlgRadius = new System.Windows.Forms.NumericUpDown();
            this.sOMClusteringBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.dlgLearningRate = new System.Windows.Forms.NumericUpDown();
            this.dlgNumberOfIterations = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.dlgInputs = new System.Windows.Forms.NumericUpDown();
            this.dlgNumberOfClusters = new System.Windows.Forms.NumericUpDown();
            this.dlgSelectedInputs = new System.Windows.Forms.ListBox();
            this.dlgAvailableInputs = new System.Windows.Forms.ListBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.dlgFileNameLoad = new System.Windows.Forms.TextBox();
            this.btnOpenFileName = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.dlgReadFromFile = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.dlgSaveToFile = new System.Windows.Forms.CheckBox();
            this.dlgFileNameSave = new System.Windows.Forms.TextBox();
            this.btnSaveFileName = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dlgRadius)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sOMClusteringBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dlgLearningRate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dlgNumberOfIterations)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dlgInputs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dlgNumberOfClusters)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(384, 426);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(77, 23);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(465, 426);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(77, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.dlgRadius);
            this.groupBox1.Controls.Add(this.dlgLearningRate);
            this.groupBox1.Controls.Add(this.dlgNumberOfIterations);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.dlgInputs);
            this.groupBox1.Controls.Add(this.dlgNumberOfClusters);
            this.groupBox1.Location = new System.Drawing.Point(13, 92);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(529, 108);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Parameters";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(264, 75);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Radius";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(264, 48);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Learning rate";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(264, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Number of iterations";
            // 
            // dlgRadius
            // 
            this.dlgRadius.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.sOMClusteringBindingSource, "Radius", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dlgRadius.Location = new System.Drawing.Point(371, 73);
            this.dlgRadius.Name = "dlgRadius";
            this.dlgRadius.Size = new System.Drawing.Size(120, 20);
            this.dlgRadius.TabIndex = 4;
            // 
            // sOMClusteringBindingSource
            // 
            this.sOMClusteringBindingSource.DataSource = typeof(Infovision.MRI.DAL.SOMClustering);
            // 
            // dlgLearningRate
            // 
            this.dlgLearningRate.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.sOMClusteringBindingSource, "LearningRate", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dlgLearningRate.Location = new System.Drawing.Point(371, 46);
            this.dlgLearningRate.Name = "dlgLearningRate";
            this.dlgLearningRate.Size = new System.Drawing.Size(120, 20);
            this.dlgLearningRate.TabIndex = 3;
            // 
            // dlgNumberOfIterations
            // 
            this.dlgNumberOfIterations.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.sOMClusteringBindingSource, "NumberOfIterations", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dlgNumberOfIterations.Location = new System.Drawing.Point(371, 19);
            this.dlgNumberOfIterations.Name = "dlgNumberOfIterations";
            this.dlgNumberOfIterations.Size = new System.Drawing.Size(120, 20);
            this.dlgNumberOfIterations.TabIndex = 2;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(87, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Number of inputs";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Number of clusters";
            // 
            // dlgInputs
            // 
            this.dlgInputs.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.sOMClusteringBindingSource, "Inputs", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dlgInputs.Location = new System.Drawing.Point(108, 46);
            this.dlgInputs.Name = "dlgInputs";
            this.dlgInputs.Size = new System.Drawing.Size(120, 20);
            this.dlgInputs.TabIndex = 0;
            // 
            // dlgNumberOfClusters
            // 
            this.dlgNumberOfClusters.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.sOMClusteringBindingSource, "NumberOfClusters", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dlgNumberOfClusters.Location = new System.Drawing.Point(108, 19);
            this.dlgNumberOfClusters.Name = "dlgNumberOfClusters";
            this.dlgNumberOfClusters.Size = new System.Drawing.Size(120, 20);
            this.dlgNumberOfClusters.TabIndex = 0;
            // 
            // dlgSelectedInputs
            // 
            this.dlgSelectedInputs.DataSource = this.sOMClusteringBindingSource;
            this.dlgSelectedInputs.FormattingEnabled = true;
            this.dlgSelectedInputs.Location = new System.Drawing.Point(10, 19);
            this.dlgSelectedInputs.Name = "dlgSelectedInputs";
            this.dlgSelectedInputs.Size = new System.Drawing.Size(219, 95);
            this.dlgSelectedInputs.TabIndex = 3;
            // 
            // dlgAvailableInputs
            // 
            this.dlgAvailableInputs.FormattingEnabled = true;
            this.dlgAvailableInputs.Location = new System.Drawing.Point(302, 19);
            this.dlgAvailableInputs.Name = "dlgAvailableInputs";
            this.dlgAvailableInputs.Size = new System.Drawing.Size(219, 95);
            this.dlgAvailableInputs.TabIndex = 4;
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(235, 41);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(61, 23);
            this.btnAdd.TabIndex = 5;
            this.btnAdd.Text = "<--";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.addBtn_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.Location = new System.Drawing.Point(235, 70);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(61, 23);
            this.btnRemove.TabIndex = 5;
            this.btnRemove.Text = "-->";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.removeBtn_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnRemove);
            this.groupBox2.Controls.Add(this.dlgAvailableInputs);
            this.groupBox2.Controls.Add(this.btnAdd);
            this.groupBox2.Controls.Add(this.dlgSelectedInputs);
            this.groupBox2.Location = new System.Drawing.Point(13, 206);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(529, 128);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Select input images";
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
            this.dlgFileNameLoad.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.sOMClusteringBindingSource, "FileNameLoad", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dlgFileNameLoad.Location = new System.Drawing.Point(108, 39);
            this.dlgFileNameLoad.Name = "dlgFileNameLoad";
            this.dlgFileNameLoad.Size = new System.Drawing.Size(333, 20);
            this.dlgFileNameLoad.TabIndex = 9;
            // 
            // btnOpenFileName
            // 
            this.btnOpenFileName.Location = new System.Drawing.Point(454, 37);
            this.btnOpenFileName.Name = "btnOpenFileName";
            this.btnOpenFileName.Size = new System.Drawing.Size(43, 23);
            this.btnOpenFileName.TabIndex = 10;
            this.btnOpenFileName.Text = "...";
            this.btnOpenFileName.UseVisualStyleBackColor = true;
            this.btnOpenFileName.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.dlgReadFromFile);
            this.groupBox3.Controls.Add(this.btnOpenFileName);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.dlgFileNameLoad);
            this.groupBox3.Location = new System.Drawing.Point(13, 13);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(529, 73);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Restore";
            this.groupBox3.Enter += new System.EventHandler(this.groupBox3_Enter);
            // 
            // dlgReadFromFile
            // 
            this.dlgReadFromFile.AutoSize = true;
            this.dlgReadFromFile.Checked = true;
            this.dlgReadFromFile.CheckState = System.Windows.Forms.CheckState.Checked;
            this.dlgReadFromFile.DataBindings.Add(new System.Windows.Forms.Binding("CheckState", this.sOMClusteringBindingSource, "LoadFile", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dlgReadFromFile.Location = new System.Drawing.Point(13, 19);
            this.dlgReadFromFile.Name = "dlgReadFromFile";
            this.dlgReadFromFile.Size = new System.Drawing.Size(91, 17);
            this.dlgReadFromFile.TabIndex = 11;
            this.dlgReadFromFile.Text = "Read from file";
            this.dlgReadFromFile.UseVisualStyleBackColor = true;
            this.dlgReadFromFile.CheckedChanged += new System.EventHandler(this.dlgReadFromFile_CheckedChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.dlgSaveToFile);
            this.groupBox4.Controls.Add(this.dlgFileNameSave);
            this.groupBox4.Controls.Add(this.btnSaveFileName);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Location = new System.Drawing.Point(13, 341);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(529, 79);
            this.groupBox4.TabIndex = 8;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Store";
            // 
            // dlgSaveToFile
            // 
            this.dlgSaveToFile.AutoSize = true;
            this.dlgSaveToFile.Checked = true;
            this.dlgSaveToFile.CheckState = System.Windows.Forms.CheckState.Checked;
            this.dlgSaveToFile.DataBindings.Add(new System.Windows.Forms.Binding("CheckState", this.sOMClusteringBindingSource, "SaveFile", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dlgSaveToFile.Location = new System.Drawing.Point(13, 22);
            this.dlgSaveToFile.Name = "dlgSaveToFile";
            this.dlgSaveToFile.Size = new System.Drawing.Size(79, 17);
            this.dlgSaveToFile.TabIndex = 11;
            this.dlgSaveToFile.Text = "Save to file";
            this.dlgSaveToFile.UseVisualStyleBackColor = true;
            this.dlgSaveToFile.CheckedChanged += new System.EventHandler(this.dlgSaveToFile_CheckedChanged);
            // 
            // dlgFileNameSave
            // 
            this.dlgFileNameSave.AcceptsTab = true;
            this.dlgFileNameSave.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.sOMClusteringBindingSource, "FileNameSave", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
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
            this.btnSaveFileName.Click += new System.EventHandler(this.button2_Click_1);
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
            // SOMClusteringDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(552, 458);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Name = "SOMClusteringDialog";
            this.Text = "SOM image clustering";
            this.Load += new System.EventHandler(this.SOMClusteringDialog_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dlgRadius)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sOMClusteringBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dlgLearningRate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dlgNumberOfIterations)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dlgInputs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dlgNumberOfClusters)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown dlgRadius;
        private System.Windows.Forms.NumericUpDown dlgLearningRate;
        private System.Windows.Forms.NumericUpDown dlgNumberOfIterations;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown dlgNumberOfClusters;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.BindingSource sOMClusteringBindingSource;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown dlgInputs;
        private System.Windows.Forms.ListBox dlgSelectedInputs;
        private System.Windows.Forms.ListBox dlgAvailableInputs;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnOpenFileName;
        private System.Windows.Forms.TextBox dlgFileNameLoad;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox dlgReadFromFile;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox dlgSaveToFile;
        private System.Windows.Forms.TextBox dlgFileNameSave;
        private System.Windows.Forms.Button btnSaveFileName;
        private System.Windows.Forms.Label label7;
    }
}