using System;
using System.Drawing;
using System.Windows.Forms;
using Infovision.MRI.DAL;

namespace Infovision.MRI.UI
{
    public partial class ImageForm : Form
    {
        private Infovision.MRI.DAL.ImageRead image;

        private Bitmap bmp;
        private Bitmap newBmp;

        private bool imageStatus = false;
        private int slice = -1;

        public ImageForm()
        {
            InitializeComponent();

            mainPanel.Paint += new PaintEventHandler(RePaint);
        }

        public Infovision.MRI.DAL.ImageRead Image
        {
            get { return this.image; }
            set
            {
                this.image = value;
                this.slice = 0;
                this.imageStatus = false;

                /*
                this.bmp = this.Image.Depth > 0
                         ? this.Image[slice].Clone(new Rectangle(0, 0, this.Image.Width, this.Image.Height), this.Image[slice].PixelFormat)
                         : this.Image.Bitmap.Clone(new Rectangle(0, 0, this.Image.Width, this.Image.Height), this.Image.Bitmap.PixelFormat);
                */

                this.bmp = this.Image.Depth > 0
                         ? this.Image[slice]
                         : this.Image.Bitmap;

                this.imageStatus = this.Image != null ? true : false;
            }
        }

        private void RePaint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (this.imageStatus)
            {
                this.DrawImage(this.newBmp, g);
                //g.DrawImage(this.newBmp, e.ClipRectangle, e.ClipRectangle, GraphicsUnit.Pixel);
            }
            base.OnPaint(e);
        }

        private void ShowImage(Bitmap image)
        {
            this.newBmp = image;
            //this.newBmp = image.Clone(new Rectangle(0, 0, image.Width, image.Height), image.PixelFormat);

            using (Graphics g = mainPanel.CreateGraphics())
            {
                this.DrawImage(this.newBmp, g);
                this.imageStatus = true;
            }
        }

        private void DrawImage(System.Drawing.Image image, Graphics g)
        {
            g.FillRectangle(Brushes.White, 0, 0, mainPanel.Width, mainPanel.Height);

            Rectangle r = new Rectangle((int)Math.Max(Math.Floor(((double)(mainPanel.Width - image.Width) / (double)2)), 0),
                                        (int)Math.Max(Math.Floor(((double)(mainPanel.Height - image.Height) / (double)2)), 0),
                                        image.Width,
                                        image.Height);
            g.DrawImage(image, r);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (this.imageStatus)
            {
                if (slice >= 1 && slice <= this.Image.Depth - 1)
                {
                    slice--;

                    ShowImage(this.Image.Depth > 0 ? this.Image[slice] : this.Image.Bitmap);
                    /*
                    this.bmp = this.Image.Depth > 0
                             ? this.Image[slice].Clone(new Rectangle(0, 0, this.Image.Width, this.Image.Height), this.Image[slice].PixelFormat)
                             : this.Image.Bitmap.Clone(new Rectangle(0, 0, this.Image.Width, this.Image.Height), this.Image.Bitmap.PixelFormat);

                    ShowImage(bmp);
                    */
                }

                statusLabel.Text = "z: " + slice.ToString();
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (this.imageStatus)
            {
                if (slice >= 0 && slice < this.Image.Depth - 1)
                {
                    slice++;

                    ShowImage(this.Image.Depth > 0 ? this.Image[slice] : this.Image.Bitmap);

                    /*
                    this.bmp = this.Image.Depth > 0
                             ? this.Image[slice].Clone(new Rectangle(0,0, this.Image.Width, this.Image.Height), this.Image[slice].PixelFormat)
                             : this.Image.Bitmap.Clone(new Rectangle(0, 0, this.Image.Width, this.Image.Height), this.Image.Bitmap.PixelFormat);

                    ShowImage(bmp);
                    */
                }

                statusLabel.Text = "z: " + slice.ToString();
            }
        }

        private void ImageForm_Load(object sender, EventArgs e)
        {
            ShowImage(bmp);
        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
        }

        private void ImageForm_Resize(object sender, EventArgs e)
        {
            if (this.imageStatus)
            {
                using (Graphics g = mainPanel.CreateGraphics())
                {
                    this.DrawImage(this.newBmp, g);
                }
            }
        }

        private void mainPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsCursorOverImageArea(e.Location.X, e.Location.Y))
            {
                int relativeX = this.CursorRelativePositionX(e.Location.X);
                int relativeY = this.CursorRelativePositionY(e.Location.Y);

                statusLabel.Text = "x: " + relativeX.ToString()
                                 + " y: " + relativeY.ToString()
                                 + " z: " + slice.ToString()
                                 + " v: " + this.Image.GetPixelValue(relativeX, relativeY, slice);
            }
            else
            {
                statusLabel.Text = "z: " + slice.ToString();
            }
        }

        private int CursorRelativePositionX(int x)
        {
            return x - (int)Math.Floor((double)(mainPanel.Width - this.Image.Width) / (double)2);
        }

        private int CursorRelativePositionY(int y)
        {
            return y - (int)Math.Floor((double)(mainPanel.Height - this.Image.Height) / (double)2);
        }

        private bool IsCursorOverImageArea(int x, int y)
        {
            if (x >= (int)Math.Floor((double)(mainPanel.Width - this.Image.Width) / (double)2)
                && x < (int)Math.Floor((double)(mainPanel.Width + this.Image.Width) / (double)2)
                && y >= (int)Math.Floor((double)(mainPanel.Height - this.Image.Height) / (double)2)
                && y < (int)Math.Floor((double)(mainPanel.Height + this.Image.Height) / (double)2))
            {
                return true;
            }

            return false;
        }

        private void mainPanel_Paint(object sender, PaintEventArgs e)
        {
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {
        }

        private void groupBox1_Enter_1(object sender, EventArgs e)
        {
        }

        private void filterGroup_Enter(object sender, EventArgs e)
        {
        }

        private void brightnessTrackbar_Scroll(object sender, EventArgs e)
        {
        }

        private void contrastTrackbar_Scroll(object sender, EventArgs e)
        {
        }

        private void brightnessTrackbar_ValueChanged(object sender, EventArgs e)
        {
            //Bitmap currentBitmap = this.Image.Depth > 0 ? this.Image[slice] : this.Image.Bitmap;
            //Bitmap pBitmap = System.Drawing.Image.currentBitmap.Clone(new Rectangle(0, 0, this.newBmp.Width, this.newBmp.Height), this.newBmp.PixelFormat);
            Bitmap pBitmap = this.Image.Depth > 0 ? this.Image[slice] : this.Image.Bitmap;
            new BrightnessContrast().Adjust(pBitmap, brightnessTrackbar.Value, contrastTrackbar.Value);
            ShowImage(pBitmap);
        }

        private void contrastTrackbar_ValueChanged(object sender, EventArgs e)
        {
            //Bitmap currentBitmap = this.Image.Depth > 0 ? this.Image[slice] : this.Image.Bitmap;
            //Bitmap pBitmap = currentBitmap.Clone(new Rectangle(0, 0, this.newBmp.Width, this.newBmp.Height), this.newBmp.PixelFormat);
            Bitmap pBitmap = this.Image.Depth > 0 ? this.Image[slice] : this.Image.Bitmap;
            new BrightnessContrast().Adjust(pBitmap, brightnessTrackbar.Value, contrastTrackbar.Value);
            ShowImage(pBitmap);
        }
    }
}