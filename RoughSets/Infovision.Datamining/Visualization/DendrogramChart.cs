using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Datamining.Clustering.Hierarchical;

namespace Infovision.Datamining.Visualization
{
    public class DendrogramChart
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public HierarchicalClusteringBase HCluster { get; set; }

        public DendrogramChart()
        {
        }

        public DendrogramChart(HierarchicalClusteringBase clustering, int width, int height)
            : this()
        {
            this.HCluster = clustering;
            this.Width = width;
            this.Height = height;
        }
        
        public Bitmap GetAsBitmap()                
        {
            int topMargin = 1;
            int bottomMargin = 1;
            int leftMargin = 1;
            int rightMargin = 1;

            int dendrogramWidth = this.Width - (leftMargin + rightMargin);
            int dendrogramHeight = this.Height - (topMargin + bottomMargin);

            Color background = Color.White;
            Color foreground = Color.Black;

            int xMajorScalePx = (int)System.Math.Floor((double)dendrogramWidth / this.HCluster.NumberOfInstances);

            //TODO scale if MaxHeight < 1
            double maxHeight = this.HCluster.Root.Height;
            double heightScale = 1;
            if (maxHeight < 1)
            {
                while (maxHeight < 1)
                {
                    maxHeight *= 10;
                    heightScale *= 10;
                }
            }

            if (dendrogramHeight < maxHeight)
            {

            }

            int yMajorScalePx = (int)System.Math.Floor((double)(dendrogramHeight - 40) / (maxHeight + 1));

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
                while (yAxisTickBegin.Y >= yAxisStart.Y)
                {
                    g.DrawLine(yAxisPen, yAxisTickBegin, yAxisTickEnd);

                    yAxisTickBegin.Y -= yMajorScalePx;
                    yAxisTickEnd.Y = yAxisTickBegin.Y;

                    g.DrawLine(dahedGridPen, dashedBegin, dashedEnd);

                    dashedBegin.Y -= yMajorScalePx;
                    dashedEnd.Y = dashedBegin.Y;
                }
                yAxisPen.Dispose();
                dahedGridPen.Dispose();


                //TODO Use tree PostOrderTraversal instead of LeafOrder + DendrogramLink collection!
                Dictionary<int, DendrogramLinkChartData> dendrogramChartData = new Dictionary<int, DendrogramLinkChartData>(this.HCluster.NumberOfInstances - 1);
                int xAxisOffset = 10;
                int nodePointX = yAxisEnd.X + xAxisOffset;

                DendrogramLinkChartData linkChartData = null;
                DendrogramLinkChartData linkChartData1 = null;
                DendrogramLinkChartData linkChartData2 = null;

                Action<DendrogramNode> buildChartPoints = delegate(DendrogramNode d)
                {
                    if (d.IsLeaf)
                    {
                        linkChartData = new DendrogramLinkChartData(d.Id);

                        linkChartData.LeftNodeX = nodePointX;
                        linkChartData.LeftNodeY = yAxisEnd.Y;
                        linkChartData.RightNodeX = nodePointX;
                        linkChartData.RightNodeY = yAxisEnd.Y;
                        linkChartData.ParentNodeY = yAxisEnd.Y;

                        dendrogramChartData.Add(d.Id, linkChartData);

                        nodePointX += xMajorScalePx;
                    }
                    else
                    {
                        linkChartData1 = dendrogramChartData[d.LeftNode.Id];
                        linkChartData2 = dendrogramChartData[d.RightNode.Id];

                        linkChartData = new DendrogramLinkChartData(d.Id);
                        linkChartData.LeftNodeX = linkChartData1.ParentNodeX;
                        linkChartData.LeftNodeY = linkChartData1.ParentNodeY;
                        linkChartData.RightNodeX = linkChartData2.ParentNodeX;
                        linkChartData.RightNodeY = linkChartData2.ParentNodeY;

                        int nodeHeight = (int)(d.Height * heightScale * yMajorScalePx);
                        linkChartData.ParentNodeY = yAxisEnd.Y - nodeHeight;

                        dendrogramChartData.Add(d.Id, linkChartData);
                    }
                };

                HierarchicalClusteringBase.TraversePostOrder(this.HCluster.Root, buildChartPoints);                                                

                Font font = new Font("Tahoma", 9);
                Pen dendrogramPen = new Pen(foreground, 2);
                foreach (KeyValuePair<int, DendrogramLinkChartData> kvp in dendrogramChartData)
                {
                    kvp.Value.Draw(g, dendrogramPen, true, font);
                }
                dendrogramPen.Dispose();
                font.Dispose();


                g.Flush();
                Bitmap result = new Bitmap(bitmap);
            }

            return bitmap;
        }
    }
}
