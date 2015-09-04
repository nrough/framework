using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Infovision.MRI.DAL;
using System.Windows.Forms.DataVisualization.Charting;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace Infovision.MRI.UI
{
    public partial class MainForm : Form
    {
        private MRIApplication appl;
        private TreeNode rootNode;

        public MainForm()
        {
            InitializeComponent();
            appl = MRIApplication.Default;

            if (appl.ActiveProject == null)
            {
                appl.CreateProject("New project");
            }

            BuildMiningObjectTree();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
        }

        private void AddMiningObjectTreeRoot(string name)
        {
            this.AddObjectTree(this.appl.GetDummyMiningObject(name));
        }

        private void BuildMiningObjectTree()
        {
            this.dlgMiningObjectTree.Nodes.Clear();
            AddMiningObjectTreeRoot(this.appl.ActiveProject.Name);
            foreach (MiningObject o in this.appl.GetMiningObjects())
            {
                this.AddObjectTree(o);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void rAWFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IMiningObject rawImage = appl.OpenRAWImage();
            if (rawImage != null)
            {
                this.AddObjectTree(rawImage);
            }
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void propertyGrid1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripContainer1_LeftToolStripPanel_Click(object sender, EventArgs e)
        {

        }

        private void toolStripContainer1_ContentPanel_Load(object sender, EventArgs e)
        {

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void clearUserDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reset();
        }

        private void dlgImageTree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode nodeSelected;
            nodeSelected = e.Node;

            /*
            if (nodeSelected.Nodes[0].Text == "*")
            {
                // This is a dummy node.
                nodeSelected.Nodes.Clear();

                foreach (IMiningObject m in miningProject.GetMiningObjects())
                {
                    TreeNode nodeChild = nodeSelected.Nodes.Add(m.Name);

                    nodeChild.Tag = m.Id;
                    nodeChild.ImageIndex = 1;
                    nodeChild.SelectedImageIndex = 1;
                }
            }
            */
        }

        private void dlgImageTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            dlgStatusLabel.Text = appl.ActiveProject.GetMiningObject((long)e.Node.Tag).Name;
        }

        private void LoadObjectTree()
        {
            foreach (IMiningObject m in this.appl.GetMiningObjects())
            {
                AddObjectTree(m);
            }
        }

        private void AddObjectTree(IMiningObject miningObject)
        {
            if (miningObject.TypeId == MiningObjectType.Types.Dummy)
            {
                rootNode = dlgMiningObjectTree.Nodes.Add(miningObject.Name);
                rootNode.ImageIndex = 0;
                rootNode.Tag = miningObject.Id;
            }
            else
            {
                TreeNode nodeParent;
                nodeParent = rootNode.Nodes.Add(miningObject.Name);
                nodeParent.ImageIndex = 0;
                nodeParent.Tag = miningObject.Id;

                rootNode.Expand();
            }

            // Add a "dummy" child node.
            //nodeParent.Nodes.Add("*");
        }

        private void RemoveObjectTree(TreeNode treeNode)
        {
            this.dlgMiningObjectTree.Nodes.Remove(treeNode);
        }

        private TreeNode oldSelectNode;
        private void dlgImageTree_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Point p = new Point(e.X, e.Y);
                TreeNode node = dlgMiningObjectTree.GetNodeAt(p);

                if (node != null)
                {
                    this.oldSelectNode = dlgMiningObjectTree.SelectedNode;
                    dlgMiningObjectTree.SelectedNode = node;

                    // Find the appropriate ContextMenu depending on the selected node.
                    switch (this.appl.ActiveProject.GetMiningObject((long)node.Tag).TypeId)
                    {
                        case MiningObjectType.Types.ImageITK:
                        case MiningObjectType.Types.ImageRAW:
                            treeContextMenu.Show(dlgMiningObjectTree, p);
                            break;

                        case MiningObjectType.Types.Dummy:
                            //do nothing
                            break;

                        //TODO Add more context menues or implement mechanism for hiding unactive menu items
                        default:
                            treeContextMenu.Show(dlgMiningObjectTree, p);
                            break;
                    }

                    dlgMiningObjectTree.SelectedNode = this.oldSelectNode;
                    this.oldSelectNode = null;
                }
            }
        }

        private void showHistogramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IMiningObject histogram = this.appl.CreateImageHistogram((long)dlgMiningObjectTree.SelectedNode.Tag);
            if (histogram != null)
            {
                this.AddObjectTree(histogram);
                histogram.View();
            }
        }

        private void showImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.appl.View((long)dlgMiningObjectTree.SelectedNode.Tag);
        }

        private void sOMClusteringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IMiningObject somClustering = this.appl.CreateSOMCluster((long)dlgMiningObjectTree.SelectedNode.Tag);
            if (somClustering != null)
            {
                this.AddObjectTree(somClustering);
            }
        }

        private void histogramClusteringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IMiningObject histCluster = this.appl.CreateHistCluster((long)dlgMiningObjectTree.SelectedNode.Tag);
            if (histCluster != null)
            {
                this.AddObjectTree(histCluster);
            }
        }

        private void imageMaskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IMiningObject mask = this.appl.CreateImageMask((long)dlgMiningObjectTree.SelectedNode.Tag);
            if (mask != null)
            {
                this.AddObjectTree(mask);
                mask.View();
            }
        }

        private void edgeFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IMiningObject edge = this.appl.CreateImageEdge((long)dlgMiningObjectTree.SelectedNode.Tag);
            if (edge != null)
            {
                this.AddObjectTree(edge);
                edge.View();
            }
        }

        private void treeContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void extractImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IMiningObject extractedImage = this.appl.ExtractImage((long)dlgMiningObjectTree.SelectedNode.Tag);
            if (extractedImage != null)
            {
                this.AddObjectTree(extractedImage);
            }
        }

        private void dlgImageTree_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2
                && dlgMiningObjectTree.SelectedNode != null)
            {
                IMiningObject selectedObject = this.appl.GetMiningObject((long)dlgMiningObjectTree.SelectedNode.Tag);
                if (selectedObject.Id != 0)
                {
                    dlgMiningObjectTree.LabelEdit = true;
                    dlgMiningObjectTree.SelectedNode.BeginEdit();
                }
            }
        }

        private void dlgImageTree_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label != null)
            {
                if (e.Label.Length > 0)
                {
                    if (e.Label.IndexOfAny(new char[] { '@', '.', ',', '!' }) == -1)
                    {
                        // Stop editing without canceling the label change.
                        e.Node.EndEdit(false);
                        dlgMiningObjectTree.LabelEdit = false;

                        this.appl.GetMiningObject((long)e.Node.Tag).Name = e.Label;
                        dlgStatusLabel.Text = e.Label;
                    }
                    else
                    {
                        /* Cancel the label edit action, inform the user, and 
                           place the node in edit mode again. */
                        e.CancelEdit = true;
                        MessageBox.Show("Invalid tree node label.\n" +
                           "The invalid characters are: '@','.', ',', '!'",
                           "Node Label Edit");
                        e.Node.BeginEdit();
                    }
                }
                else
                {
                    /* Cancel the label edit action, inform the user, and 
                       place the node in edit mode again. */
                    e.CancelEdit = true;
                    MessageBox.Show("Invalid tree node label.\nThe label cannot be blank",
                       "Node Label Edit");
                    e.Node.BeginEdit();
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.Show();
        }

        private void projectToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.RestoreDirectory = false;
            fileDialog.Filter = "Mining Project XML 1.0 (*.xml)|*.xml";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                this.appl.SaveProject(fileDialog.FileName);
            }
        }

        private void imageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IMiningObject image = this.appl.OpenImage();
            if (image != null)
            {
                this.AddObjectTree(image);
                image.View();
            }
        }

        private void projectToolStripMenuItem_Click(object sender, EventArgs e)
        {

            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.RestoreDirectory = false;
            fileDialog.Filter = "Mining Project XML 1.0 (*.xml)|*.xml";

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                this.appl.LoadProject(fileDialog.FileName);
                BuildMiningObjectTree();
            }
        }

        private void neighbourToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IMiningObject neighbour = this.appl.CreateNeighbour();
            if (neighbour != null)
            {
                this.AddObjectTree(neighbour);
            }
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.appl.RemoveMiningObject((long)dlgMiningObjectTree.SelectedNode.Tag))
            {
                this.RemoveObjectTree(dlgMiningObjectTree.SelectedNode);
            }
        }

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IMiningObject selectedObject = this.appl.GetMiningObject((long)dlgMiningObjectTree.SelectedNode.Tag);
            if (selectedObject != null)
            {
                MiningObjectPropertyForm propertiesForm = new MiningObjectPropertyForm();
                propertiesForm.MiningObject = selectedObject;
                propertiesForm.Show();
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.appl.ActiveProject.IsDirty)
            {
                this.appl.ActiveProject.Save();
            }
        }

    }
}