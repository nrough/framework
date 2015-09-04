using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Clustering.Hierarchical
{
    internal class DendrogramLinkChartData
    {
        public DendrogramLinkChartData(int nodeId)
        {
            this.NodeId = nodeId;
        }
        
        public int NodeId { get; private set; }                

        public int LeftNodeX { get; set; }
        public int LeftNodeY { get; set; }

        public int RightNodeX { get; set; }
        public int RightNodeY { get; set; }
        
        public int ParentNodeY { get; set; }

        public int ParentNodeX
        {
            get
            {
                return (this.LeftNodeX + this.RightNodeX) / 2;
            }
        }        

        public void Draw(Graphics g, Pen pen, bool showLabel, Font font)
        {
            Point a = new Point(this.LeftNodeX, this.LeftNodeY);
            Point b = new Point(this.LeftNodeX, this.ParentNodeY);
            Point c = new Point(this.RightNodeX, this.ParentNodeY);
            Point d = new Point(this.RightNodeX, this.RightNodeY);

            g.DrawLines(pen, new Point[] { a, b, c, d });

            SolidBrush brush = new SolidBrush(pen.Color);
            if (showLabel)
            {
                g.DrawString(this.NodeId.ToString(), font, brush, new PointF(this.ParentNodeX - ((font.Size * this.NodeId.ToString().Length) / 2), this.ParentNodeY + 4));
            }
            brush.Dispose();
        }
        

        public override string ToString()
        {
            return String.Format("{0}: ({1},{2}) ({3},{4}) ({5},{6}) ({7},{8})", 
                                 NodeId, 
                                 LeftNodeX, 
                                 LeftNodeY,
                                 LeftNodeX,
                                 ParentNodeY,
                                 RightNodeX,
                                 ParentNodeY,
                                 RightNodeX,
                                 RightNodeY);
        }

        public override int GetHashCode()
        {
            return this.NodeId.GetHashCode();
        }
        
    }
}
