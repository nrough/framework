namespace NRough.MRI
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
        Int64 = 4,
        UInt8 = 5,
        UInt16 = 6,
        UInt32 = 7,
        UInt64 = 8,
        Float = 9,
        Double = 10
    }

    public enum ITKImageType
    {
        Unknown = 0,
        ITKRawImage = 1,
        ITKStandardImage = 2
    }
}