using System;
using System.Drawing;

namespace Infovision.MRI
{
    public interface IImage
    {
        uint Width { get; set; }
        uint Height { get; set; }
        uint Depth { get; set; }

        Type PixelType { get; set; }

        IImage Extract(int z);
        
        T GetPixel<T>(uint[] position) where T : IComparable, IConvertible;
        void SetPixel<T>(uint[] position, T value) where T : IComparable, IConvertible;
        T[] GetData<T>() where T : IComparable, IConvertible;

        Bitmap GetBitmap();
        Bitmap GetBitmap(uint z);
    }
}
