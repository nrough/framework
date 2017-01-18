namespace Raccoon.MRI.UI
{
    partial class ImageForm
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
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.mainPanel = new System.Windows.Forms.Panel();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.filterGroup = new System.Windows.Forms.GroupBox();
            this.contrastLbl = new System.Windows.Forms.Label();
            this.brightnessLbl = new System.Windows.Forms.Label();
            this.contrastTrackbar = new System.Windows.Forms.TrackBar();
            this.brightnessTrackbar = new System.Windows.Forms.TrackBar();
            this.sliceGroup = new System.Windows.Forms.GroupBox();
            this.sliceLbl = new System.Windows.Forms.Label();
            this.sliceTrackbar = new System.Windows.Forms.TrackBar();
            this.statusStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.filterGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.contrastTrackbar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.brightnessTrackbar)).BeginInit();
            this.sliceGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sliceTrackbar)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 386);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(726, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            this.statusStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.statusStrip1_ItemClicked);
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.toolStripButton2});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(726, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            this.toolStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.toolStrip1_ItemClicked);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = global::Raccoon.MRI.UI.Properties.Resources.arrow_back_16xLG;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "toolStripButton1";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton2.Image = global::Raccoon.MRI.UI.Properties.Resources.arrow_Forward_16xLG;
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton2.Text = "toolStripButton2";
            this.toolStripButton2.Click += new System.EventHandler(this.toolStripButton2_Click);
            // 
            // mainPanel
            // 
            this.mainPanel.Cursor = System.Windows.Forms.Cursors.Cross;
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.Location = new System.Drawing.Point(0, 0);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new System.Drawing.Size(436, 361);
            this.mainPanel.TabIndex = 2;
            this.mainPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.mainPanel_Paint);
            this.mainPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.mainPanel_MouseMove);
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(0, 25);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 361);
            this.splitter1.TabIndex = 3;
            this.splitter1.TabStop = false;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 25);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.mainPanel);
            this.splitContainer1.Panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer1_Panel1_Paint);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.filterGroup);
            this.splitContainer1.Panel2.Controls.Add(this.sliceGroup);
            this.splitContainer1.Panel2.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer1_Panel2_Paint);
            this.splitContainer1.Size = new System.Drawing.Size(723, 361);
            this.splitContainer1.SplitterDistance = 436;
            this.splitContainer1.TabIndex = 4;
            // 
            // filterGroup
            // 
            this.filterGroup.Controls.Add(this.contrastLbl);
            this.filterGroup.Controls.Add(this.brightnessLbl);
            this.filterGroup.Controls.Add(this.contrastTrackbar);
            this.filterGroup.Controls.Add(this.brightnessTrackbar);
            this.filterGroup.Location = new System.Drawing.Point(4, 110);
            this.filterGroup.Name = "filterGroup";
            this.filterGroup.Size = new System.Drawing.Size(276, 248);
            this.filterGroup.TabIndex = 1;
            this.filterGroup.TabStop = false;
            this.filterGroup.Text = "groupBox2";
            this.filterGroup.Enter += new System.EventHandler(this.filterGroup_Enter);
            // 
            // contrastLbl
            // 
            this.contrastLbl.AutoSize = true;
            this.contrastLbl.Location = new System.Drawing.Point(127, 99);
            this.contrastLbl.Name = "contrastLbl";
            this.contrastLbl.Size = new System.Drawing.Size(46, 13);
            this.contrastLbl.TabIndex = 3;
            this.contrastLbl.Text = "Contrast";
            // 
            // brightnessLbl
            // 
            this.brightnessLbl.AutoSize = true;
            this.brightnessLbl.Location = new System.Drawing.Point(124, 20);
            this.brightnessLbl.Name = "brightnessLbl";
            this.brightnessLbl.Size = new System.Drawing.Size(56, 13);
            this.brightnessLbl.TabIndex = 2;
            this.brightnessLbl.Text = "Brightness";
            // 
            // contrastTrackbar
            // 
            this.contrastTrackbar.Location = new System.Drawing.Point(8, 115);
            this.contrastTrackbar.Maximum = 100;
            this.contrastTrackbar.Minimum = -100;
            this.contrastTrackbar.Name = "contrastTrackbar";
            this.contrastTrackbar.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.contrastTrackbar.Size = new System.Drawing.Size(259, 45);
            this.contrastTrackbar.TabIndex = 1;
            this.contrastTrackbar.TickFrequency = 5;
            this.contrastTrackbar.Scroll += new System.EventHandler(this.contrastTrackbar_Scroll);
            this.contrastTrackbar.ValueChanged += new System.EventHandler(this.contrastTrackbar_ValueChanged);
            // 
            // brightnessTrackbar
            // 
            this.brightnessTrackbar.LargeChange = 50;
            this.brightnessTrackbar.Location = new System.Drawing.Point(8, 44);
            this.brightnessTrackbar.Maximum = 250;
            this.brightnessTrackbar.Minimum = -250;
            this.brightnessTrackbar.Name = "brightnessTrackbar";
            this.brightnessTrackbar.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.brightnessTrackbar.Size = new System.Drawing.Size(259, 45);
            this.brightnessTrackbar.SmallChange = 10;
            this.brightnessTrackbar.TabIndex = 0;
            this.brightnessTrackbar.TickFrequency = 10;
            this.brightnessTrackbar.Scroll += new System.EventHandler(this.brightnessTrackbar_Scroll);
            this.brightnessTrackbar.ValueChanged += new System.EventHandler(this.brightnessTrackbar_ValueChanged);
            // 
            // sliceGroup
            // 
            this.sliceGroup.Controls.Add(this.sliceLbl);
            this.sliceGroup.Controls.Add(this.sliceTrackbar);
            this.sliceGroup.Location = new System.Drawing.Point(3, 3);
            this.sliceGroup.Name = "sliceGroup";
            this.sliceGroup.Size = new System.Drawing.Size(277, 100);
            this.sliceGroup.TabIndex = 0;
            this.sliceGroup.TabStop = false;
            this.sliceGroup.Text = "Image slice";
            this.sliceGroup.Enter += new System.EventHandler(this.groupBox1_Enter_1);
            // 
            // sliceLbl
            // 
            this.sliceLbl.AutoSize = true;
            this.sliceLbl.Location = new System.Drawing.Point(122, 33);
            this.sliceLbl.Name = "sliceLbl";
            this.sliceLbl.Size = new System.Drawing.Size(30, 13);
            this.sliceLbl.TabIndex = 1;
            this.sliceLbl.Text = "Slice";
            // 
            // sliceTrackbar
            // 
            this.sliceTrackbar.LargeChange = 10;
            this.sliceTrackbar.Location = new System.Drawing.Point(9, 49);
            this.sliceTrackbar.Maximum = 100;
            this.sliceTrackbar.Name = "sliceTrackbar";
            this.sliceTrackbar.Size = new System.Drawing.Size(261, 45);
            this.sliceTrackbar.TabIndex = 0;
            this.sliceTrackbar.TickFrequency = 2;
            // 
            // ImageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(726, 408);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "ImageForm";
            this.Text = "ImageForm";
            this.Load += new System.EventHandler(this.ImageForm_Load);
            this.Resize += new System.EventHandler(this.ImageForm_Resize);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.filterGroup.ResumeLayout(false);
            this.filterGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.contrastTrackbar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.brightnessTrackbar)).EndInit();
            this.sliceGroup.ResumeLayout(false);
            this.sliceGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sliceTrackbar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox sliceGroup;
        private System.Windows.Forms.GroupBox filterGroup;
        private System.Windows.Forms.Label sliceLbl;
        private System.Windows.Forms.TrackBar sliceTrackbar;
        private System.Windows.Forms.Label contrastLbl;
        private System.Windows.Forms.Label brightnessLbl;
        private System.Windows.Forms.TrackBar contrastTrackbar;
        private System.Windows.Forms.TrackBar brightnessTrackbar;

    }
}