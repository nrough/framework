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
            System.Drawing.Imaging.PixelFormat pixelFormat = System.Drawing.Imaging.PixelFormat.Format32bppArgb;

            Bitmap bitmap = new Bitmap(width, height, pixelFormat);
            Graphics g = Graphics.FromImage(bitmap);            
        }
    }
}
