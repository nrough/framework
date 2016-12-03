using System;
using System.Drawing;

namespace Infovision.MachineLearning.Clustering.Hierarchical
{
    internal class DendrogramChartNode
    {
        public int NodeId { get; private set; }
        public int LeftNodeX { get; set; }
        public int LeftNodeY { get; set; }
        public int RightNodeX { get; set; }
        public int RightNodeY { get; set; }
        public int ParentNodeY { get; set; }
        public Color LeftColor { get; set; }
        public Color RightColor { get; set; }
        public double Height { get; set; }
        public bool IsLeaf { get; set; }

        public int ParentNodeX
        {
            get
            {
                return (this.LeftNodeX + this.RightNodeX) / 2;
            }
        }

        public DendrogramChartNode(int nodeId)
        {
            this.NodeId = nodeId;
            this.LeftColor = Color.Black;
            this.RightColor = Color.Black;
        }

        public void Draw(Graphics g, Pen pen, Brush brush, bool showLabel, Font font)
        {
            Point a = new Point(this.LeftNodeX, this.LeftNodeY);
            Point b = new Point(this.LeftNodeX, this.ParentNodeY);
            Point x = new Point(this.ParentNodeX, this.ParentNodeY);
            Point c = new Point(this.RightNodeX, this.ParentNodeY);
            Point d = new Point(this.RightNodeX, this.RightNodeY);

            Color origColor = pen.Color;

            pen.Color = this.LeftColor;
            g.DrawLines(pen, new Point[] { a, b });

            pen.Color = this.LeftColor == this.RightColor ? this.LeftColor : origColor;
            g.DrawLines(pen, new Point[] { b, x });

            pen.Color = this.LeftColor == this.RightColor ? this.RightColor : origColor;
            g.DrawLines(pen, new Point[] { x, c });

            pen.Color = this.RightColor;
            g.DrawLines(pen, new Point[] { c, d });

            pen.Color = origColor;
            if (showLabel)
            {
                /*
                if (!this.IsLeaf)
                {
                    g.DrawString(String.Format("{0} ({1:0.000})", this.NodeId.ToString(), this.Height),
                                 font,
                                 brush,
                                 new PointF(this.ParentNodeX - (font.Size * (this.NodeId.ToString().Length + 7) / 2), this.ParentNodeY + 4));
                }
                else
                {
                */
                g.DrawString(String.Format("{0}", this.NodeId.ToString()),
                             font,
                             brush,
                             new PointF(this.ParentNodeX - ((font.Size * this.NodeId.ToString().Length) / 2), this.ParentNodeY + 4));
                //}
            }
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