using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace NRough.MRI.DAL
{
    public abstract class ImageFilterBase
    {
        abstract protected void Prepare(int[] pValues);

        abstract protected byte Adjust(byte iValue);

        public void Adjust(Bitmap pBitmap, params int[] pValues)
        {
            Prepare(pValues);

            BitmapData pBitmapData = pBitmap.LockBits(
                new Rectangle(0, 0, pBitmap.Width, pBitmap.Height),
                ImageLockMode.ReadWrite,
                pBitmap.PixelFormat);

            byte[] pData = new byte[pBitmapData.Stride * pBitmapData.Height];
            Marshal.Copy(pBitmapData.Scan0, pData, 0, pData.Length);

            int iOffset = pBitmapData.Stride - pBitmapData.Width * 3;
            int iIndex = 0;
            for (int i = 0; i < pBitmapData.Height; i++)
            {
                for (int j = 0; j < pBitmapData.Width; j++)
                {
                    for (int k = iIndex; k < iIndex + 3; k++)
                    {
                        pData[k] = Adjust(pData[k]);
                    }
                    iIndex += 3;
                }
                iIndex += iOffset;
            }

            Marshal.Copy(pData, 0, pBitmapData.Scan0, pData.Length);
            pBitmap.UnlockBits(pBitmapData);
        }

        protected byte Fix(int iValue)
        {
            if (iValue < 0) iValue = 0;
            if (iValue > 255) iValue = 255;
            return (byte)iValue;
        }
    }

    public class Brightness : ImageFilterBase
    {
        private int m_iBrightness;

        protected override void Prepare(int[] pValues)
        {
            m_iBrightness = pValues[0];
        }

        protected override byte Adjust(byte iValue)
        {
            return Fix(iValue + m_iBrightness);
        }
    }

    public class Contrast : ImageFilterBase
    {
        private double m_fContrast;

        protected override void Prepare(int[] pValues)
        {
            m_fContrast = Math.Pow((100.0 + pValues[0]) / 100.0, 2);
        }

        protected override byte Adjust(byte iValue)
        {
            return Fix((int)(((iValue / 255.0 - 0.5) * m_fContrast + 0.5) * 255.0));
        }
    }

    public class BrightnessContrast : ImageFilterBase
    {
        private int m_iBrightness;
        private double m_fContrast;

        protected override void Prepare(int[] pValues)
        {
            m_iBrightness = pValues[0];
            m_fContrast = Math.Pow((100.0 + pValues[1]) / 100.0, 2);
        }

        protected override byte Adjust(byte iValue)
        {
            return Fix((int)(((Fix(iValue + m_iBrightness) / 255.0 - 0.5) * m_fContrast + 0.5) * 255.0));
        }
    }
}