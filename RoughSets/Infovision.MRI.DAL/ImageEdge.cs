using System;

namespace NRough.MRI.DAL
{
    public class ImageEdge : MiningObjectViewModel
    {
        private int background;
        private int foreground;
        private double noise;
        private IImage image;

        public int Background
        {
            get { return this.background; }
            set { SetField(ref this.background, value, () => Background); }
        }

        public int Foreground
        {
            get { return this.foreground; }
            set { SetField(ref this.foreground, value, () => Foreground); }
        }

        public double Noise
        {
            get { return this.noise; }
            set { SetField(ref this.noise, value, () => Noise); }
        }

        public IImage Image
        {
            get { return image; }
            set { this.image = value; }
        }

        public override Type GetMiningObjectType()
        {
            return typeof(MiningObjectImageEdge);
        }
    }
}