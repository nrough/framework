
namespace Infovision.MRI
{
    public enum Endianness
    {
        Unknown = 0,
        BigEndian = 1,
        LittleEndian = 2
    }

    public enum PixelType
    {
        Unknown = 0,
        Int8 = 1,
        Int16 = 2,
        Int32 = 3,
        UInt8 = 4,
        UInt16 = 5,
        Uint = 6,
        Float = 7,
        Double = 8
    }

    public enum ITKImageType
    {
        Unknown = 0,
        ITKRawImage = 1,
        ITKStandardImage = 2
    }


}
