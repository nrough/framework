namespace Infovision.MRI.UI
{
    partial class MiningObjectPropertyForm
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
            this.dlgPropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.SuspendLayout();
            // 
            // dlgPropertyGrid
            // 
            this.dlgPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dlgPropertyGrid.Location = new System.Drawing.Point(0, 0);
            this.dlgPropertyGrid.Name = "dlgPropertyGrid";
            this.dlgPropertyGrid.SelectedObject = this;
            this.dlgPropertyGrid.Size = new System.Drawing.Size(555, 513);
            this.dlgPropertyGrid.TabIndex = 0;
            this.dlgPropertyGrid.Click += new System.EventHandler(this.propertyGrid1_Click);
            // 
            // MiningObjectPropertyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(555, 513);
            this.Controls.Add(this.dlgPropertyGrid);
            this.Name = "MiningObjectPropertyForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Mining object properties";
            this.Load += new System.EventHandler(this.MiningObjectPropertyForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PropertyGrid dlgPropertyGrid;
    }
}