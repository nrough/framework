using System.Windows.Forms;

namespace Raccoon.MRI.UI
{
    partial class MainForm
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.projectToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.projectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem10 = new System.Windows.Forms.ToolStripSeparator();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.projectToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem11 = new System.Windows.Forms.ToolStripSeparator();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rAWFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rAWImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem9 = new System.Windows.Forms.ToolStripSeparator();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearUserDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripSeparator();
            this.showImageToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.showHistogramToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
            this.histogramClusteringToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.sOMClusteringToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripSeparator();
            this.windowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.dlgStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.dlgMiningObjectTree = new System.Windows.Forms.TreeView();
            this.panel2 = new System.Windows.Forms.Panel();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.treeContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showHistogramToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.histogramClusteringToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sOMClusteringToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.imageMaskToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.laplacianFilterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.neighbourToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.propertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.treeContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.windowToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.MdiWindowListItem = this.windowToolStripMenuItem;
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.menuStrip1.Size = new System.Drawing.Size(976, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.menuStrip1_ItemClicked);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.toolStripMenuItem10,
            this.saveToolStripMenuItem,
            this.toolStripMenuItem11,
            this.importToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.toolStripMenuItem9,
            this.closeToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.projectToolStripMenuItem1});
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.newToolStripMenuItem.Text = "New";
            // 
            // projectToolStripMenuItem1
            // 
            this.projectToolStripMenuItem1.Name = "projectToolStripMenuItem1";
            this.projectToolStripMenuItem1.Size = new System.Drawing.Size(120, 22);
            this.projectToolStripMenuItem1.Text = "Project...";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.projectToolStripMenuItem,
            this.imageToolStripMenuItem});
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // projectToolStripMenuItem
            // 
            this.projectToolStripMenuItem.Name = "projectToolStripMenuItem";
            this.projectToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.projectToolStripMenuItem.Text = "Project...";
            this.projectToolStripMenuItem.Click += new System.EventHandler(this.projectToolStripMenuItem_Click);
            // 
            // imageToolStripMenuItem
            // 
            this.imageToolStripMenuItem.Name = "imageToolStripMenuItem";
            this.imageToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.imageToolStripMenuItem.Text = "Image...";
            this.imageToolStripMenuItem.Click += new System.EventHandler(this.imageToolStripMenuItem_Click);
            // 
            // toolStripMenuItem10
            // 
            this.toolStripMenuItem10.Name = "toolStripMenuItem10";
            this.toolStripMenuItem10.Size = new System.Drawing.Size(149, 6);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.projectToolStripMenuItem2});
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.saveToolStripMenuItem.Text = "Save";
            // 
            // projectToolStripMenuItem2
            // 
            this.projectToolStripMenuItem2.Name = "projectToolStripMenuItem2";
            this.projectToolStripMenuItem2.Size = new System.Drawing.Size(120, 22);
            this.projectToolStripMenuItem2.Text = "Project...";
            this.projectToolStripMenuItem2.Click += new System.EventHandler(this.projectToolStripMenuItem2_Click);
            // 
            // toolStripMenuItem11
            // 
            this.toolStripMenuItem11.Name = "toolStripMenuItem11";
            this.toolStripMenuItem11.Size = new System.Drawing.Size(149, 6);
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rAWFileToolStripMenuItem});
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.importToolStripMenuItem.Text = "Import";
            // 
            // rAWFileToolStripMenuItem
            // 
            this.rAWFileToolStripMenuItem.Name = "rAWFileToolStripMenuItem";
            this.rAWFileToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.rAWFileToolStripMenuItem.Text = "RAW Image...";
            this.rAWFileToolStripMenuItem.Click += new System.EventHandler(this.rAWFileToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rAWImageToolStripMenuItem});
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exportToolStripMenuItem.Text = "Export";
            // 
            // rAWImageToolStripMenuItem
            // 
            this.rAWImageToolStripMenuItem.Name = "rAWImageToolStripMenuItem";
            this.rAWImageToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.rAWImageToolStripMenuItem.Text = "RAW Image...";
            // 
            // toolStripMenuItem9
            // 
            this.toolStripMenuItem9.Name = "toolStripMenuItem9";
            this.toolStripMenuItem9.Size = new System.Drawing.Size(149, 6);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearUserDataToolStripMenuItem,
            this.toolStripMenuItem8,
            this.showImageToolStripMenuItem1,
            this.showHistogramToolStripMenuItem1,
            this.toolStripMenuItem6,
            this.histogramClusteringToolStripMenuItem1,
            this.sOMClusteringToolStripMenuItem1,
            this.toolStripMenuItem7});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // clearUserDataToolStripMenuItem
            // 
            this.clearUserDataToolStripMenuItem.Name = "clearUserDataToolStripMenuItem";
            this.clearUserDataToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.clearUserDataToolStripMenuItem.Text = "Clear user label";
            this.clearUserDataToolStripMenuItem.Click += new System.EventHandler(this.clearUserDataToolStripMenuItem_Click);
            // 
            // toolStripMenuItem8
            // 
            this.toolStripMenuItem8.Name = "toolStripMenuItem8";
            this.toolStripMenuItem8.Size = new System.Drawing.Size(191, 6);
            // 
            // showImageToolStripMenuItem1
            // 
            this.showImageToolStripMenuItem1.Name = "showImageToolStripMenuItem1";
            this.showImageToolStripMenuItem1.Size = new System.Drawing.Size(194, 22);
            this.showImageToolStripMenuItem1.Text = "Show image";
            // 
            // showHistogramToolStripMenuItem1
            // 
            this.showHistogramToolStripMenuItem1.Name = "showHistogramToolStripMenuItem1";
            this.showHistogramToolStripMenuItem1.Size = new System.Drawing.Size(194, 22);
            this.showHistogramToolStripMenuItem1.Text = "Show histogram...";
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(191, 6);
            // 
            // histogramClusteringToolStripMenuItem1
            // 
            this.histogramClusteringToolStripMenuItem1.Name = "histogramClusteringToolStripMenuItem1";
            this.histogramClusteringToolStripMenuItem1.Size = new System.Drawing.Size(194, 22);
            this.histogramClusteringToolStripMenuItem1.Text = "Histogram clustering...";
            // 
            // sOMClusteringToolStripMenuItem1
            // 
            this.sOMClusteringToolStripMenuItem1.Name = "sOMClusteringToolStripMenuItem1";
            this.sOMClusteringToolStripMenuItem1.Size = new System.Drawing.Size(194, 22);
            this.sOMClusteringToolStripMenuItem1.Text = "SOM clustering...";
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            this.toolStripMenuItem7.Size = new System.Drawing.Size(191, 6);
            // 
            // windowToolStripMenuItem
            // 
            this.windowToolStripMenuItem.Name = "windowToolStripMenuItem";
            this.windowToolStripMenuItem.Size = new System.Drawing.Size(63, 20);
            this.windowToolStripMenuItem.Text = "Window";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.aboutToolStripMenuItem.Text = "About...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dlgStatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 501);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(976, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // dlgStatusLabel
            // 
            this.dlgStatusLabel.Name = "dlgStatusLabel";
            this.dlgStatusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // dlgMiningObjectTree
            // 
            this.dlgMiningObjectTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dlgMiningObjectTree.Location = new System.Drawing.Point(0, 0);
            this.dlgMiningObjectTree.Name = "dlgMiningObjectTree";
            this.dlgMiningObjectTree.Size = new System.Drawing.Size(200, 477);
            this.dlgMiningObjectTree.TabIndex = 6;
            this.dlgMiningObjectTree.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.dlgImageTree_AfterLabelEdit);
            this.dlgMiningObjectTree.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.dlgImageTree_BeforeExpand);
            this.dlgMiningObjectTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.dlgImageTree_AfterSelect);
            this.dlgMiningObjectTree.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dlgImageTree_KeyDown);
            this.dlgMiningObjectTree.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dlgImageTree_MouseUp);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.dlgMiningObjectTree);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 24);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(200, 477);
            this.panel2.TabIndex = 9;
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(200, 24);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 477);
            this.splitter1.TabIndex = 10;
            this.splitter1.TabStop = false;
            // 
            // treeContextMenu
            // 
            this.treeContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showImageToolStripMenuItem,
            this.showHistogramToolStripMenuItem,
            this.extractImageToolStripMenuItem,
            this.toolStripMenuItem1,
            this.histogramClusteringToolStripMenuItem,
            this.sOMClusteringToolStripMenuItem,
            this.toolStripMenuItem2,
            this.imageMaskToolStripMenuItem,
            this.toolStripMenuItem3,
            this.laplacianFilterToolStripMenuItem,
            this.toolStripMenuItem4,
            this.neighbourToolStripMenuItem,
            this.toolStripMenuItem5,
            this.propertiesToolStripMenuItem,
            this.removeToolStripMenuItem});
            this.treeContextMenu.Name = "treeContextMenu";
            this.treeContextMenu.Size = new System.Drawing.Size(197, 254);
            this.treeContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.treeContextMenu_Opening);
            // 
            // showImageToolStripMenuItem
            // 
            this.showImageToolStripMenuItem.Name = "showImageToolStripMenuItem";
            this.showImageToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.showImageToolStripMenuItem.Text = "View";
            this.showImageToolStripMenuItem.Click += new System.EventHandler(this.showImageToolStripMenuItem_Click);
            // 
            // showHistogramToolStripMenuItem
            // 
            this.showHistogramToolStripMenuItem.Name = "showHistogramToolStripMenuItem";
            this.showHistogramToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.showHistogramToolStripMenuItem.Text = "Show histogram...";
            this.showHistogramToolStripMenuItem.Click += new System.EventHandler(this.showHistogramToolStripMenuItem_Click);
            // 
            // extractImageToolStripMenuItem
            // 
            this.extractImageToolStripMenuItem.Name = "extractImageToolStripMenuItem";
            this.extractImageToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.extractImageToolStripMenuItem.Text = "Extract image...";
            this.extractImageToolStripMenuItem.Click += new System.EventHandler(this.extractImageToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(193, 6);
            // 
            // histogramClusteringToolStripMenuItem
            // 
            this.histogramClusteringToolStripMenuItem.Name = "histogramClusteringToolStripMenuItem";
            this.histogramClusteringToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.histogramClusteringToolStripMenuItem.Text = "Histogram Clustering...";
            this.histogramClusteringToolStripMenuItem.Click += new System.EventHandler(this.histogramClusteringToolStripMenuItem_Click);
            // 
            // sOMClusteringToolStripMenuItem
            // 
            this.sOMClusteringToolStripMenuItem.Name = "sOMClusteringToolStripMenuItem";
            this.sOMClusteringToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.sOMClusteringToolStripMenuItem.Text = "SOM Clustering...";
            this.sOMClusteringToolStripMenuItem.Click += new System.EventHandler(this.sOMClusteringToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(193, 6);
            // 
            // imageMaskToolStripMenuItem
            // 
            this.imageMaskToolStripMenuItem.Name = "imageMaskToolStripMenuItem";
            this.imageMaskToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.imageMaskToolStripMenuItem.Text = "Image mask...";
            this.imageMaskToolStripMenuItem.Click += new System.EventHandler(this.imageMaskToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(193, 6);
            // 
            // laplacianFilterToolStripMenuItem
            // 
            this.laplacianFilterToolStripMenuItem.Name = "laplacianFilterToolStripMenuItem";
            this.laplacianFilterToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.laplacianFilterToolStripMenuItem.Text = "Laplacian filter...";
            this.laplacianFilterToolStripMenuItem.Click += new System.EventHandler(this.edgeFilterToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(193, 6);
            // 
            // neighbourToolStripMenuItem
            // 
            this.neighbourToolStripMenuItem.Name = "neighbourToolStripMenuItem";
            this.neighbourToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.neighbourToolStripMenuItem.Text = "Neighbour..";
            this.neighbourToolStripMenuItem.Click += new System.EventHandler(this.neighbourToolStripMenuItem_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(193, 6);
            // 
            // propertiesToolStripMenuItem
            // 
            this.propertiesToolStripMenuItem.Name = "propertiesToolStripMenuItem";
            this.propertiesToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.propertiesToolStripMenuItem.Text = "Properties";
            this.propertiesToolStripMenuItem.Click += new System.EventHandler(this.propertiesToolStripMenuItem_Click);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(976, 523);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.statusStrip1);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Raccoon MRI";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.treeContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rAWFileToolStripMenuItem;
        private System.Windows.Forms.Timer timer1;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel dlgStatusLabel;
        private TreeView dlgMiningObjectTree;
        private ToolStripMenuItem windowToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private Panel panel2;
        private Splitter splitter1;
        private ToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripMenuItem clearUserDataToolStripMenuItem;
        private ContextMenuStrip treeContextMenu;
        private ToolStripMenuItem showImageToolStripMenuItem;
        private ToolStripMenuItem showHistogramToolStripMenuItem;
        private ToolStripMenuItem histogramClusteringToolStripMenuItem;
        private ToolStripMenuItem sOMClusteringToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripSeparator toolStripMenuItem2;
        private ToolStripMenuItem imageMaskToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem3;
        private ToolStripMenuItem laplacianFilterToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem4;
        private ToolStripMenuItem neighbourToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem5;
        private ToolStripMenuItem propertiesToolStripMenuItem;
        private ToolStripMenuItem showImageToolStripMenuItem1;
        private ToolStripMenuItem showHistogramToolStripMenuItem1;
        private ToolStripSeparator toolStripMenuItem6;
        private ToolStripMenuItem histogramClusteringToolStripMenuItem1;
        private ToolStripMenuItem sOMClusteringToolStripMenuItem1;
        private ToolStripSeparator toolStripMenuItem7;
        private ToolStripSeparator toolStripMenuItem8;
        private ToolStripMenuItem extractImageToolStripMenuItem;
        private ToolStripMenuItem newToolStripMenuItem;
        private ToolStripMenuItem projectToolStripMenuItem1;
        private ToolStripMenuItem projectToolStripMenuItem;
        private ToolStripMenuItem imageToolStripMenuItem;
        private ToolStripMenuItem exportToolStripMenuItem;
        private ToolStripMenuItem closeToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem10;
        private ToolStripMenuItem rAWImageToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem9;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripMenuItem projectToolStripMenuItem2;
        private ToolStripSeparator toolStripMenuItem11;
        private ToolStripMenuItem removeToolStripMenuItem;
    }
}

