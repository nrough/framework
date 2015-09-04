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
        
        //TODO optimize object size
        /*
        public int TopY { get; set; }
        public int MiddleY { get; set; }
        public int BottomY { get; set; }
        public int MiddleY { get; set; }
        public int LeftX { get; set; }
        public int RightX { get; set; } 
        */

        public int LeftBottomX { get; set; }
        public int LeftBottomY { get; set; }
        public int RightBottomX { get; set; }
        public int RightBottomY { get; set; }
        public int LeftTopX { get; set; }
        public int LeftTopY { get; set; }
        public int RightTopX { get; set; }
        public int RightTopY { get; set; }

        public int ParentNodeX
        {
            get
            {
                return (this.LeftTopX + this.RightTopX) / 2;
            }
        }

        public int ParentNodeY
        {
            get
            {
                return this.LeftTopY;
            }
        }

        public void Draw(Graphics g, Pen pen)
        {
            Point a = new Point(this.LeftBottomX, this.LeftBottomY);
            Point b = new Point(this.LeftTopX, this.LeftTopY);
            Point c = new Point(this.RightTopX, this.RightTopY);
            Point d = new Point(this.RightBottomX, this.RightBottomY);

            g.DrawLines(pen, new Point[] { a, b, c, d });
        }
        

        public override string ToString()
        {
            return String.Format("{0}: ({1},{2}) ({3},{4}) ({5},{6}) ({7},{8})", 
                                 NodeId, 
                                 LeftBottomX, 
                                 LeftBottomY, 
                                 LeftTopX, 
                                 LeftTopY, 
                                 RightBottomX, 
                                 RightBottomY, 
                                 RightTopX, 
                                 RightTopY);
        }

        public override int GetHashCode()
        {
            return this.NodeId.GetHashCode();
        }
        
    }
}
