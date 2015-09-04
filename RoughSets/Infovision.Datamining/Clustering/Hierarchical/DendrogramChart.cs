using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Datamining.Clustering.Hierarchical;

namespace Infovision.Datamining.Clustering.Hierarchical
{
    public class DendrogramChart
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public HierarchicalClusteringBase HCluster { get; set; }
        public List<Color> Colors { get; set; }

        public DendrogramChart()
        {
            this.Colors = new List<Color>();
            this.Colors.Add(Color.Black);
            this.Width = 800;
            this.Height = 600;
        }

        public DendrogramChart(HierarchicalClusteringBase clustering, int width, int height)
            : this()
        {
            this.Colors = new List<Color>();
            this.HCluster = clustering;
            this.Width = width;
            this.Height = height;
        }
        
        public Bitmap GetAsBitmap()                
        {
            int topMargin = 1;
            int bottomMargin = 1;
            int leftMargin = 25;
            int rightMargin = 1;
            int dendrogramWidth = this.Width - (leftMargin + rightMargin);
            int dendrogramHeight = this.Height - (topMargin + bottomMargin);
            Color background = Color.White;
            Color foreground = Color.Black;
            int xMajorScalePx = (int)System.Math.Floor((double)dendrogramWidth / (double)this.HCluster.NumberOfInstances);            
            double maxHeight = this.HCluster.Root.Height;
            double heightScale = 1;
            
            if (maxHeight > 0.0 && maxHeight < 1.0)
            {
                //TODO scale if 0 < maxHeight < 1
                while (maxHeight < 1)
                {
                    maxHeight *= 10;
                    heightScale *= 10;
                }
            }
            else if (maxHeight == 0.0)
            {
                maxHeight = 1;                
            }

            if (dendrogramHeight < maxHeight)
            {
                //TODO scale
            }

            int yMajorScalePx = (int)System.Math.Floor((double)(dendrogramHeight - 40) / (maxHeight + 1));
            
            //TODO Scale
            int yScalePx = yMajorScalePx;
            
            if (yMajorScalePx <= 1)
                yMajorScalePx = 100;

            
            

            Bitmap bitmap = new Bitmap(this.Width, this.Height);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(background);

                //draw Y axis                
                Pen yAxisPen = new Pen(foreground, 1);
                System.Drawing.Point yAxisStart = new System.Drawing.Point(leftMargin + 10, topMargin + 20);
                System.Drawing.Point yAxisEnd = new System.Drawing.Point(leftMargin + 10, (this.Height - (bottomMargin + 20)));
                g.DrawLine(yAxisPen, yAxisStart, yAxisEnd);

                //draw Y major scale and dashed grid lines
                System.Drawing.Point yAxisTickBegin = new System.Drawing.Point(yAxisEnd.X - 1, yAxisEnd.Y);
                System.Drawing.Point yAxisTickEnd = new System.Drawing.Point(yAxisEnd.X + 1, yAxisTickBegin.Y);
                System.Drawing.Point dashedBegin = new System.Drawing.Point(yAxisEnd.X, yAxisEnd.Y);
                System.Drawing.Point dashedEnd = new System.Drawing.Point(this.Width - (leftMargin + 20), dashedBegin.Y);

                Pen dahedGridPen = new Pen(Color.Black, 1);
                dahedGridPen.DashStyle = DashStyle.Dot;
                
                Font yAxisFont = new Font("Tahoma", 9);
                Brush yAxisBrush = new SolidBrush(foreground);
                
                while (yAxisTickBegin.Y >= yAxisStart.Y)
                {
                    //g.DrawString(String.Format("{0:0.000}", yAxisTickBegin.Y - yAxisStart.Y),
                    //         yAxisFont,
                    //         yAxisBrush,
                    //         new PointF(yAxisTickBegin.X - 24, yAxisTickBegin.Y));
                    
                    g.DrawLine(yAxisPen, yAxisTickBegin, yAxisTickEnd);

                    yAxisTickBegin.Y -= yMajorScalePx;
                    yAxisTickEnd.Y = yAxisTickBegin.Y;

                    g.DrawLine(dahedGridPen, dashedBegin, dashedEnd);

                    dashedBegin.Y -= yMajorScalePx;
                    dashedEnd.Y = dashedBegin.Y;
                }
                yAxisBrush.Dispose();
                yAxisFont.Dispose();

                yAxisPen.Dispose();
                dahedGridPen.Dispose();

                
                
                Dictionary<int, DendrogramChartNode> dendrogramChartData = new Dictionary<int, DendrogramChartNode>(this.HCluster.NumberOfInstances - 1);
                int xAxisOffset = 10;
                int nodePointX = yAxisEnd.X + xAxisOffset;

                DendrogramChartNode linkChartData = null;
                DendrogramChartNode linkChartData1 = null;
                DendrogramChartNode linkChartData2 = null;

                Dictionary<int, int> node2cluster = null;
                Dictionary<int, int> cluster2color = null;
                //HashSet<int> uncoloredNodes = null;
                if (this.Colors.Count > 1)
                {
                    List<DendrogramNode> subTrees = this.HCluster.GetCutOffNodes(this.Colors.Count);
                    node2cluster = new Dictionary<int, int>(this.HCluster.NumberOfInstances);
                    //uncoloredNodes = new HashSet<int>();              
                    foreach (DendrogramNode node in subTrees)
                    {
                        HierarchicalClusteringBase.TraversePreOrder(node, d => node2cluster[d.Id] = node.Id);
                        //if (node.Parent != null)
                        //    HierarchicalClusteringBase.TraverseParent(node.Parent.Parent, d => uncoloredNodes.Add(d.Id));
                    }                    
                    cluster2color = new Dictionary<int, int>(this.Colors.Count);
                    int c = 0;
                    foreach (KeyValuePair<int, int> kvp in node2cluster)
                    {
                        if (!cluster2color.ContainsKey(kvp.Value))
                            cluster2color.Add(kvp.Value, c++);                        
                    }
                }

                Action<DendrogramNode> buildChartPoints = delegate(DendrogramNode d)
                {
                    if (d.IsLeaf)
                    {
                        linkChartData = new DendrogramChartNode(d.Id);

                        linkChartData.LeftNodeX = nodePointX;
                        linkChartData.LeftNodeY = yAxisEnd.Y;
                        linkChartData.RightNodeX = nodePointX;
                        linkChartData.RightNodeY = yAxisEnd.Y;
                        linkChartData.ParentNodeY = yAxisEnd.Y;
                        linkChartData.Height = d.Height;
                        linkChartData.IsLeaf = d.IsLeaf;

                        linkChartData.LeftColor = this.Colors.Count > 1 
                                            ? this.Colors[cluster2color[node2cluster[d.Id]]] 
                                            : foreground;
                        linkChartData.RightColor = linkChartData.LeftColor;

                                                
                        dendrogramChartData.Add(d.Id, linkChartData);

                        nodePointX += xMajorScalePx;
                    }
                    else
                    {
                        linkChartData1 = dendrogramChartData[d.LeftNode.Id];
                        linkChartData2 = dendrogramChartData[d.RightNode.Id];

                        linkChartData = new DendrogramChartNode(d.Id);
                        linkChartData.LeftNodeX = linkChartData1.ParentNodeX;
                        linkChartData.LeftNodeY = linkChartData1.ParentNodeY;
                        linkChartData.RightNodeX = linkChartData2.ParentNodeX;
                        linkChartData.RightNodeY = linkChartData2.ParentNodeY;
                        linkChartData.Height = d.Height;
                        linkChartData.IsLeaf = d.IsLeaf;

                        int nodeHeight = (int)(d.Height * heightScale * yScalePx);
                        linkChartData.ParentNodeY = yAxisEnd.Y - nodeHeight;
                        linkChartData.LeftColor = linkChartData1.LeftColor;
                        linkChartData.RightColor = linkChartData2.RightColor;

                        //if (uncoloredNodes != null && uncoloredNodes.Contains(d.Id))
                        //{
                        //    linkChartData.LeftColor = foreground;
                        //    linkChartData.RightColor = foreground;
                        //}

                        dendrogramChartData.Add(d.Id, linkChartData);
                    }
                };

                if (this.HCluster.NumberOfInstances > 1)
                {
                    HierarchicalClusteringBase.TraversePostOrder(this.HCluster.Root, buildChartPoints);
                }
                else
                {
                    int id = (this.HCluster.Root.LeftNode != null)
                           ? this.HCluster.Root.LeftNode.Id
                           : this.HCluster.Root.Id;

                    linkChartData = new DendrogramChartNode(id);

                    linkChartData.LeftNodeX = nodePointX;
                    linkChartData.LeftNodeY = yAxisEnd.Y;
                    linkChartData.RightNodeX = nodePointX;
                    linkChartData.RightNodeY = yAxisEnd.Y;
                    linkChartData.ParentNodeY = yAxisEnd.Y;

                    dendrogramChartData.Add(id, linkChartData);
                }

                Font font = new Font("Tahoma", 9);
                Pen dendrogramPen = new Pen(foreground, 2);
                Brush brush = new SolidBrush(foreground);
                
                foreach (KeyValuePair<int, DendrogramChartNode> kvp in dendrogramChartData)                                    
                    kvp.Value.Draw(g, dendrogramPen, brush, true, font);
                                
                brush.Dispose();
                dendrogramPen.Dispose();
                font.Dispose();

                g.Flush();
                Bitmap result = new Bitmap(bitmap);
            }

            return bitmap;
        }
    }
}
