namespace NRough.MRI.UI
{
    partial class NeighbourDialog
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
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.dlgMaskObject = new System.Windows.Forms.ComboBox();
            this.maskAvailableBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.imageNeighbourBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.dlgLabelObject = new System.Windows.Forms.ComboBox();
            this.labelsAvailableBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maskAvailableBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageNeighbourBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.labelsAvailableBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.dlgMaskObject);
            this.groupBox1.Controls.Add(this.dlgLabelObject);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(424, 85);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Related Objects";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Mask object";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Label object";
            // 
            // dlgMaskObject
            // 
            this.dlgMaskObject.DataSource = this.maskAvailableBindingSource;
            this.dlgMaskObject.DisplayMember = "DisplayName";
            this.dlgMaskObject.FormattingEnabled = true;
            this.dlgMaskObject.Location = new System.Drawing.Point(127, 48);
            this.dlgMaskObject.Name = "dlgMaskObject";
            this.dlgMaskObject.Size = new System.Drawing.Size(267, 21);
            this.dlgMaskObject.TabIndex = 1;
            this.dlgMaskObject.ValueMember = "Id";
            this.dlgMaskObject.SelectedIndexChanged += new System.EventHandler(this.dlgMaskObject_SelectedIndexChanged);
            // 
            // maskAvailableBindingSource
            // 
            this.maskAvailableBindingSource.DataMember = "MaskAvailable";
            this.maskAvailableBindingSource.DataSource = this.imageNeighbourBindingSource;
            // 
            // imageNeighbourBindingSource
            // 
            this.imageNeighbourBindingSource.DataSource = typeof(NRough.MRI.DAL.ImageNeighbour);
            // 
            // dlgLabelObject
            // 
            this.dlgLabelObject.DataSource = this.labelsAvailableBindingSource;
            this.dlgLabelObject.DisplayMember = "DisplayName";
            this.dlgLabelObject.FormattingEnabled = true;
            this.dlgLabelObject.Location = new System.Drawing.Point(127, 20);
            this.dlgLabelObject.Name = "dlgLabelObject";
            this.dlgLabelObject.Size = new System.Drawing.Size(267, 21);
            this.dlgLabelObject.TabIndex = 0;
            this.dlgLabelObject.ValueMember = "Id";
            this.dlgLabelObject.SelectedIndexChanged += new System.EventHandler(this.dlgLabelObject_SelectedIndexChanged);
            // 
            // labelsAvailableBindingSource
            // 
            this.labelsAvailableBindingSource.DataMember = "LabelsAvailable";
            this.labelsAvailableBindingSource.DataSource = this.imageNeighbourBindingSource;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(281, 104);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(362, 104);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // NeighbourDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(456, 137);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.groupBox1);
            this.Name = "NeighbourDialog";
            this.Text = "Neighbour";
            this.Load += new System.EventHandler(this.NeighbourDialog_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maskAvailableBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageNeighbourBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.labelsAvailableBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox dlgMaskObject;
        private System.Windows.Forms.ComboBox dlgLabelObject;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.BindingSource maskAvailableBindingSource;
        private System.Windows.Forms.BindingSource imageNeighbourBindingSource;
        private System.Windows.Forms.BindingSource labelsAvailableBindingSource;
    }
}