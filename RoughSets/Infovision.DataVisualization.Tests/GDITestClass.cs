using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Drawing;

namespace Infovision.DataVisualization.Tests
{
    [TestFixture]
    public class GDITestClass
    {
        [Test]
        public void MethodTest()
        {
            int width = 640;
            int height = 380;
            string outputFileName = "F:\\test.bmp";

            Random random = new Random();
            
            int x1 = width % random.Next(width - 1);
            int y1 = height % random.Next(height - 1);
            int x2 = width % random.Next(width - 1);
            int y2 = height % random.Next(height - 1);

            Bitmap bitmap = new Bitmap(width, height);
            
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);

                //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                Pen pen = new Pen(Color.Black, 2);                
                Point p1 = new Point(x1, y1);
                Point p2 = new Point(x2, y2);                                
                g.DrawLine(pen, p1, p2);
                g.Flush();

                pen.Dispose();                

                bitmap.Save(outputFileName, System.Drawing.Imaging.ImageFormat.Bmp);
            }            
        }
    }
}
