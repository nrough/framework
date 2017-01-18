using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

//using System.Windows.Media;
//using System.Windows.Media.Imaging;
using itk.simple;

namespace Raccoon.MRI
{
    public class SimpleITKHelper
    {
        /// <summary>
        /// Get maximum pixel key based on itk.simple PixelEnumValueId
        /// </summary>
        /// <param name="pixelID"></param>
        /// <returns></returns>
        public static double MaxPixelValue(int pixelID)
        {
            if (pixelID == PixelIDValueEnum.sitkUInt8.swigValue)
            {
                return (double)Byte.MaxValue;
            }
            else if (pixelID == PixelIDValueEnum.sitkUInt16.swigValue)
            {
                return (double)ushort.MaxValue;
            }
            else if (pixelID == PixelIDValueEnum.sitkUInt32.swigValue)
            {
                return (double)uint.MaxValue;
            }
            else if (pixelID == PixelIDValueEnum.sitkInt8.swigValue)
            {
                return (double)char.MaxValue;
            }
            else if (pixelID == PixelIDValueEnum.sitkInt16.swigValue)
            {
                return (double)short.MaxValue;
            }
            else if (pixelID == PixelIDValueEnum.sitkInt32.swigValue)
            {
                return (double)int.MaxValue;
            }
            else if (pixelID == PixelIDValueEnum.sitkFloat32.swigValue)
            {
                return (double)float.MaxValue;
            }
            else if (pixelID == PixelIDValueEnum.sitkFloat64.swigValue)
            {
                return (double)double.MaxValue;
            }

            return 0;
        }

        /// <summary>
        /// Get maximum pixel key based on pixel <c>Type</c>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static double MaxPixelValue(Type type)
        {
            double maxPixelValue = 0;

            var @switch = new Dictionary<Type, Action> {
                { typeof(sbyte), () => maxPixelValue = (double)sbyte.MaxValue },
                { typeof(short), () => maxPixelValue = (double)short.MaxValue },
                { typeof(int), () => maxPixelValue = (double)int.MaxValue },
                { typeof(byte), () => maxPixelValue = (double)byte.MaxValue },
                { typeof(ushort), () => maxPixelValue = (double)ushort.MaxValue },
                { typeof(uint), () => maxPixelValue = (double)uint.MaxValue },
                { typeof(float), () => maxPixelValue = (double)float.MaxValue },
                { typeof(double), () => maxPixelValue = (double)double.MaxValue }
            };

            @switch[type]();

            return maxPixelValue;
        }

        public static double GetPixelAsDouble(itk.simple.Image image, VectorUInt32 position)
        {
            return Convert.ToDouble(SimpleITKHelper.GetPixel(image, position));
        }

        public static byte[] GetPixelAsBytes(itk.simple.Image image, VectorUInt32 position)
        {
            if (image.GetPixelIDValue() == PixelIDValueEnum.sitkUInt8.swigValue)
            {
                return new byte[] { image.GetPixelAsUInt8(position) };
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkInt16.swigValue)
            {
                return BitConverter.GetBytes((short)image.GetPixelAsInt16(position));
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkUInt16.swigValue)
            {
                return BitConverter.GetBytes((ushort)image.GetPixelAsUInt16(position));
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkUInt32.swigValue)
            {
                return BitConverter.GetBytes((uint)image.GetPixelAsUInt32(position));
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkInt8.swigValue)
            {
                return BitConverter.GetBytes((sbyte)image.GetPixelAsInt8(position));
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkInt32.swigValue)
            {
                return BitConverter.GetBytes((int)image.GetPixelAsInt32(position));
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkFloat32.swigValue)
            {
                return BitConverter.GetBytes((float)image.GetPixelAsFloat(position));
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkFloat64.swigValue)
            {
                return BitConverter.GetBytes((double)image.GetPixelAsDouble(position));
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkInt64.swigValue)
            {
                return BitConverter.GetBytes((int)image.GetPixelAsInt64(position));
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkUInt64.swigValue)
            {
                return BitConverter.GetBytes((uint)image.GetPixelAsUInt64(position));
            }

            StringBuilder message = new StringBuilder();
            message.AppendFormat("Unhandled Pixel ID Value = {0}", image.GetPixelIDValue());

            throw new System.InvalidOperationException(message.ToString());
        }

        public static byte[] GetImageAsBytes(itk.simple.Image image)
        {
            int width = (int)image.GetWidth();
            int height = (int)image.GetHeight();
            int depth = image.GetDepth() > 0 ? (int)image.GetDepth() : 1;
            int numberOfPixels = width * height * depth;
            int bytesPerPixel = SimpleITKHelper.PixelSize(image.GetPixelIDValue());
            int bufferLength = numberOfPixels * bytesPerPixel;
            byte[] result = new byte[bufferLength];

            IntPtr bufferPtr;
            if (image.GetPixelIDValue() == PixelIDValueEnum.sitkUInt8.swigValue)
            {
                bufferPtr = image.GetBufferAsUInt8();
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkInt16.swigValue)
            {
                bufferPtr = image.GetBufferAsInt16();
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkUInt16.swigValue)
            {
                bufferPtr = image.GetBufferAsUInt16();
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkUInt32.swigValue)
            {
                bufferPtr = image.GetBufferAsUInt32();
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkInt8.swigValue)
            {
                bufferPtr = image.GetBufferAsInt8();
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkInt32.swigValue)
            {
                bufferPtr = image.GetBufferAsInt32();
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkFloat32.swigValue)
            {
                bufferPtr = image.GetBufferAsFloat();
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkFloat64.swigValue)
            {
                bufferPtr = image.GetBufferAsDouble();
            }
            else
            {
                StringBuilder message = new StringBuilder();
                message.AppendFormat("Unhandled Pixel ID Value = {0}", image.GetPixelIDValue());

                throw new System.NotImplementedException(message.ToString());
            }

            Marshal.Copy(bufferPtr, result, 0, bufferLength);

            return result;
        }

        public static int PixelSize(int pixelIDValue)
        {
            if (pixelIDValue == PixelIDValueEnum.sitkUInt8.swigValue)
            {
                return sizeof(byte);
            }
            else if (pixelIDValue == PixelIDValueEnum.sitkInt16.swigValue)
            {
                return sizeof(short);
            }
            else if (pixelIDValue == PixelIDValueEnum.sitkUInt16.swigValue)
            {
                return sizeof(ushort);
            }
            else if (pixelIDValue == PixelIDValueEnum.sitkUInt32.swigValue)
            {
                return sizeof(uint);
            }
            else if (pixelIDValue == PixelIDValueEnum.sitkInt8.swigValue)
            {
                return sizeof(sbyte);
            }
            else if (pixelIDValue == PixelIDValueEnum.sitkInt32.swigValue)
            {
                return sizeof(int);
            }
            else if (pixelIDValue == PixelIDValueEnum.sitkFloat32.swigValue)
            {
                return sizeof(float);
            }
            else if (pixelIDValue == PixelIDValueEnum.sitkFloat64.swigValue)
            {
                return sizeof(double);
            }

            StringBuilder message = new StringBuilder();
            message.AppendFormat("Unhandled Pixel ID Value = {0}", pixelIDValue);
            throw new System.NotImplementedException(message.ToString());
        }

        public static object GetPixel(itk.simple.Image image, VectorUInt32 position)
        {
            if (image.GetPixelIDValue() == PixelIDValueEnum.sitkUInt8.swigValue)
            {
                return image.GetPixelAsUInt8(position);
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkInt16.swigValue)
            {
                return image.GetPixelAsInt16(position);
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkUInt16.swigValue)
            {
                return image.GetPixelAsUInt16(position);
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkUInt32.swigValue)
            {
                return image.GetPixelAsUInt32(position);
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkUInt64.swigValue)
            {
                return image.GetPixelAsUInt64(position);
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkInt8.swigValue)
            {
                return image.GetPixelAsInt8(position);
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkInt32.swigValue)
            {
                return image.GetPixelAsInt32(position);
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkInt64.swigValue)
            {
                return image.GetPixelAsInt64(position);
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkFloat32.swigValue)
            {
                return image.GetPixelAsFloat(position);
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkFloat64.swigValue)
            {
                return image.GetPixelAsDouble(position);
            }

            StringBuilder message = new StringBuilder();
            message.AppendFormat("Unhandled Pixel ID Value = {0}", image.GetPixelIDValue());

            throw new System.NotImplementedException(message.ToString());
        }

        public static object GetPixel(itk.simple.Image image, uint[] position)
        {
            return SimpleITKHelper.GetPixel(image, new VectorUInt32(position));
        }

        public static AForge.Range GetRandRange(itk.simple.Image image)
        {
            AForge.Range range;

            if (image.GetPixelIDValue() == PixelIDValueEnum.sitkUInt8.swigValue)
            {
                range = new AForge.Range(0, Byte.MaxValue);
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkInt16.swigValue)
            {
                range = new AForge.Range(0, short.MaxValue);
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkUInt16.swigValue)
            {
                range = new AForge.Range(0, ushort.MaxValue);
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkUInt32.swigValue)
            {
                range = new AForge.Range(0, uint.MaxValue);
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkInt8.swigValue)
            {
                range = new AForge.Range(0, char.MaxValue);
            }
            else if (image.GetPixelIDValue() == PixelIDValueEnum.sitkInt32.swigValue)
            {
                range = new AForge.Range(0, int.MaxValue);
            }

            return new AForge.Range(0, 255);
        }

        public static AForge.Range GetRandRange(IImage image)
        {
            return new AForge.Range(0, (float)SimpleITKHelper.MaxPixelValue(image.PixelType));
        }

        public static int GetNumberOfHistogramBuckets(IImage image, int bucketSize)
        {
            if (bucketSize <= 0)
            {
                throw new ArgumentOutOfRangeException("bucketSize", "Parameter bucketSize should be grater than zero");
            }

            return (int)Math.Ceiling(SimpleITKHelper.MaxPixelValue(image.PixelType) / (double)bucketSize);
        }

        /*
        public static Bitmap ConvertToBitmap_OLD(itk.simple.Image image)
        {
            if (image.GetDepth() > 0)
            {
                throw new ArgumentException("image", "Only 2D images can be converted to bitmaps");
            }

            System.Windows.Media.PixelFormat pixelFormat = SimpleITKHelper.PixelIDValue2MediaPixelFormat(image.GetPixelIDValue());
            int width = (int)image.GetWidth();
            int height = (int)image.GetHeight();
            int bytesPerPixel = (int) Math.Ceiling((double) pixelFormat.BitsPerPixel / (double) 8);
            int stride = bytesPerPixel * width;

            byte[] pixelBytes = SimpleITKHelper.GetImageAsBytes(image);
            BitmapSource bmpSource = BitmapSource.Construct(width, height, 96, 96, pixelFormat, null, pixelBytes, stride);
            bmpSource.Freeze();

            Bitmap bmpResult = null;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Construct(bmpSource));
                enc.Save(outStream);
                bmpResult = new System.Drawing.Bitmap(outStream);
            }

            return bmpResult;
        }
        */

        public static Bitmap ConvertToBitmap(itk.simple.Image image)
        {
            if (image.GetDepth() > 0)
            {
                throw new ArgumentException("image", "Only 2D images can be converted to bitmaps");
            }

            System.Drawing.Imaging.PixelFormat pixelFormat = SimpleITKHelper.PixelIDValue2PixelFormat(image.GetPixelIDValue());

            int width = (int)image.GetWidth();
            int height = (int)image.GetHeight();
            byte[] pixelBytes = SimpleITKHelper.GetImageAsBytes(image);
            int bits = SimpleITKHelper.PixelSize(image.GetPixelIDValue()) * 8;

            Bitmap result = ImageHelper.Convert(pixelBytes, width, height, 12);

            return result;
        }

        public static Bitmap ConvertToBitmap(itk.simple.Image image, int sliceIdx)
        {
            itk.simple.Image slice = SimpleITKHelper.GetSlice(image, sliceIdx);
            return SimpleITKHelper.ConvertToBitmap(slice);
        }

        public static itk.simple.Image GetSlice(itk.simple.Image image, int sliceIdx)
        {
            if (image.GetDepth() < sliceIdx)
            {
                throw new ArgumentException("sliceIdx", "Slice index cannot exceed image depth");
            }

            itk.simple.Image slice = SimpleITK.Extract(image,
                                            new VectorUInt32(new uint[] { image.GetWidth(), image.GetHeight(), 0 }),
                                            new VectorInt32(new int[] { 0, 0, sliceIdx }),
                                            ExtractImageFilter.DirectionCollapseToStrategyType.DIRECTIONCOLLAPSETOSUBMATRIX);

            return slice;
        }

        /*
        public static System.Windows.Media.PixelFormat PixelIDValue2MediaPixelFormat(int pixelIDValueEnum)
        {
            if (pixelIDValueEnum == PixelIDValueEnum.sitkUInt8.swigValue)
            {
                return PixelFormats.Gray8;
            }
            else if (pixelIDValueEnum == PixelIDValueEnum.sitkInt16.swigValue)
            {
                return PixelFormats.Gray16;
            }
            else if (pixelIDValueEnum == PixelIDValueEnum.sitkUInt16.swigValue)
            {
                return PixelFormats.Gray16;
            }
            else if (pixelIDValueEnum == PixelIDValueEnum.sitkUInt32.swigValue)
            {
                return PixelFormats.Gray32Float;
            }
            else if (pixelIDValueEnum == PixelIDValueEnum.sitkUInt64.swigValue)
            {
                return PixelFormats.Gray8;
            }
            else if (pixelIDValueEnum == PixelIDValueEnum.sitkInt8.swigValue)
            {
                return PixelFormats.Gray8;
            }
            else if (pixelIDValueEnum == PixelIDValueEnum.sitkInt32.swigValue)
            {
                return PixelFormats.Gray32Float;
            }
            else if (pixelIDValueEnum == PixelIDValueEnum.sitkInt64.swigValue)
            {
                return PixelFormats.Gray32Float;
            }
            else if (pixelIDValueEnum == PixelIDValueEnum.sitkFloat32.swigValue)
            {
                return PixelFormats.Gray32Float;
            }
            else if (pixelIDValueEnum == PixelIDValueEnum.sitkFloat64.swigValue)
            {
                return PixelFormats.Gray32Float;
            }

            throw new NotImplementedException(String.Format("PixelIDValueEnum key {0} not implemented.", pixelIDValueEnum.ToString()));
        }
        */

        public static System.Drawing.Imaging.PixelFormat PixelIDValue2PixelFormat(int pixelIDValueEnum)
        {
            if (pixelIDValueEnum == PixelIDValueEnum.sitkUInt8.swigValue)
            {
                return System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
            }
            else if (pixelIDValueEnum == PixelIDValueEnum.sitkInt16.swigValue)
            {
                return System.Drawing.Imaging.PixelFormat.Format16bppGrayScale;
            }
            else if (pixelIDValueEnum == PixelIDValueEnum.sitkUInt16.swigValue)
            {
                return System.Drawing.Imaging.PixelFormat.Format16bppGrayScale;
            }
            else if (pixelIDValueEnum == PixelIDValueEnum.sitkUInt32.swigValue)
            {
                return System.Drawing.Imaging.PixelFormat.Format32bppArgb;
            }
            else if (pixelIDValueEnum == PixelIDValueEnum.sitkUInt64.swigValue)
            {
                return System.Drawing.Imaging.PixelFormat.Format64bppArgb;
            }
            else if (pixelIDValueEnum == PixelIDValueEnum.sitkInt8.swigValue)
            {
                return System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
            }
            else if (pixelIDValueEnum == PixelIDValueEnum.sitkInt32.swigValue)
            {
                return System.Drawing.Imaging.PixelFormat.Format32bppArgb;
            }
            else if (pixelIDValueEnum == PixelIDValueEnum.sitkInt64.swigValue)
            {
                return System.Drawing.Imaging.PixelFormat.Format64bppArgb;
            }
            else if (pixelIDValueEnum == PixelIDValueEnum.sitkFloat32.swigValue)
            {
                return System.Drawing.Imaging.PixelFormat.Format32bppArgb;
            }
            else if (pixelIDValueEnum == PixelIDValueEnum.sitkFloat64.swigValue)
            {
                return System.Drawing.Imaging.PixelFormat.Format64bppArgb;
            }

            throw new NotImplementedException(String.Format("PixelIDValueEnum key {0} not implemented.", pixelIDValueEnum.ToString()));
        }

        public static IntPtr GetIntPtr<T>(Array src, int srcOffset, int srcCount, int dstCount)
        {
            T[] data = new T[dstCount];
            Buffer.BlockCopy(src, srcOffset, data, 0, srcCount);
            GCHandle hObject = GCHandle.Alloc(data, GCHandleType.Pinned);
            return hObject.AddrOfPinnedObject();
        }

        public static void WriteImageRaw(itk.simple.Image image, string fileName)
        {
            int width = (int)image.GetWidth();
            int height = (int)image.GetHeight();
            int depth = (int)image.GetDepth();

            VectorDouble direction = image.GetDirection();
            VectorDouble spacing = image.GetSpacing();
            VectorDouble origin = image.GetOrigin();
            uint dimension = image.GetDimension();

            byte[] pixelBytes = SimpleITKHelper.GetImageAsBytes(image);
            int bits = SimpleITKHelper.PixelSize(image.GetPixelIDValue()) * 8;

            System.IO.File.WriteAllBytes(fileName, pixelBytes);
        }

        public static itk.simple.Image ReadImageRAW(string fileName,
                                                    uint width,
                                                    uint height,
                                                    uint depth,
                                                    PixelIDValueEnum pixelIDValue,
                                                    Endianness endianness = Endianness.LittleEndian,
                                                    uint header = 0)
        {
            byte[] imageData = System.IO.File.ReadAllBytes(fileName);

            int numberOfPixels = depth > 0
                ? (int)width * (int)height * (int)depth
                : (int)width * (int)height;

            int pixelSize = SimpleITKHelper.PixelSize(pixelIDValue.swigValue);

            if (endianness == Endianness.BigEndian && pixelSize > 1)
            {
                for (int i = (int)header; i < imageData.Length; i += pixelSize)
                {
                    Array.Reverse(imageData, i, pixelSize);
                }
            }

            IntPtr imageDataPtr;
            if (pixelIDValue == PixelIDValueEnum.sitkUInt8)
            {
                imageDataPtr = SimpleITKHelper.GetIntPtr<byte>(imageData, (int)header, imageData.Length, numberOfPixels);
            }
            else if (pixelIDValue == PixelIDValueEnum.sitkInt16)
            {
                imageDataPtr = SimpleITKHelper.GetIntPtr<short>(imageData, (int)header, imageData.Length, numberOfPixels);
            }
            else if (pixelIDValue == PixelIDValueEnum.sitkUInt16)
            {
                imageDataPtr = SimpleITKHelper.GetIntPtr<ushort>(imageData, (int)header, imageData.Length, numberOfPixels);
            }
            else if (pixelIDValue == PixelIDValueEnum.sitkUInt32)
            {
                imageDataPtr = SimpleITKHelper.GetIntPtr<uint>(imageData, (int)header, imageData.Length, numberOfPixels);
            }
            else if (pixelIDValue == PixelIDValueEnum.sitkInt8)
            {
                imageDataPtr = SimpleITKHelper.GetIntPtr<sbyte>(imageData, (int)header, imageData.Length, numberOfPixels);
            }
            else if (pixelIDValue == PixelIDValueEnum.sitkInt32)
            {
                imageDataPtr = SimpleITKHelper.GetIntPtr<int>(imageData, (int)header, imageData.Length, numberOfPixels);
            }
            else if (pixelIDValue == PixelIDValueEnum.sitkFloat32)
            {
                imageDataPtr = SimpleITKHelper.GetIntPtr<float>(imageData, (int)header, imageData.Length, numberOfPixels);
            }
            else if (pixelIDValue == PixelIDValueEnum.sitkFloat64)
            {
                imageDataPtr = SimpleITKHelper.GetIntPtr<double>(imageData, (int)header, imageData.Length, numberOfPixels);
            }
            else
            {
                throw new NotImplementedException();
            }

            ImportImageFilter importImageFilter = new ImportImageFilter();
            importImageFilter.SetSize(new VectorUInt32(new uint[] { width, height, depth }));

            VectorDouble direction = (depth > 0)

                ? new VectorDouble(new double[] {1, 0, 0,
                                                 0, 1, 0,
                                                 0, 0, 1})

                : new VectorDouble(new double[] {1, 0,
                                                 0, 1});

            importImageFilter.SetDirection(direction);

            VectorDouble origin = (depth) > 0
                ? new VectorDouble(new double[] { 0, 0, 0 })
                : new VectorDouble(new double[] { 0, 0 });

            importImageFilter.SetOrigin(origin);

            VectorDouble spacing = (depth > 0)
                ? new VectorDouble(new double[] { 1, 1, 1 })
                : new VectorDouble(new double[] { 1, 1 });

            importImageFilter.SetSpacing(spacing);

            if (pixelIDValue == PixelIDValueEnum.sitkUInt8)
            {
                importImageFilter.SetBufferAsUInt8(imageDataPtr);
            }
            else if (pixelIDValue == PixelIDValueEnum.sitkInt16)
            {
                importImageFilter.SetBufferAsInt16(imageDataPtr);
            }
            else if (pixelIDValue == PixelIDValueEnum.sitkUInt16)
            {
                importImageFilter.SetBufferAsUInt16(imageDataPtr);
            }
            else if (pixelIDValue == PixelIDValueEnum.sitkUInt32)
            {
                importImageFilter.SetBufferAsUInt32(imageDataPtr);
            }
            else if (pixelIDValue == PixelIDValueEnum.sitkInt8)
            {
                importImageFilter.SetBufferAsInt8(imageDataPtr);
            }
            else if (pixelIDValue == PixelIDValueEnum.sitkInt32)
            {
                importImageFilter.SetBufferAsInt32(imageDataPtr);
            }
            else if (pixelIDValue == PixelIDValueEnum.sitkFloat32)
            {
                importImageFilter.SetBufferAsFloat(imageDataPtr);
            }
            else if (pixelIDValue == PixelIDValueEnum.sitkFloat64)
            {
                importImageFilter.SetBufferAsDouble(imageDataPtr);
            }

            return importImageFilter.Execute();
        }

        public static itk.simple.Image ReadImage(string fileName)
        {
            ImageFileReader reader = new ImageFileReader();
            reader.SetFileName(fileName);
            return reader.Execute();
        }

        public static itk.simple.PixelIDValueEnum PixelType2ItkPixelIDValue(PixelType pixelType)
        {
            switch (pixelType)
            {
                case PixelType.Int8:
                    return itk.simple.PixelIDValueEnum.sitkInt8;

                case PixelType.Int16:
                    return itk.simple.PixelIDValueEnum.sitkInt16;

                case PixelType.Int32:
                    return itk.simple.PixelIDValueEnum.sitkInt32;

                case PixelType.Int64:
                    return itk.simple.PixelIDValueEnum.sitkInt64;

                case PixelType.UInt8:
                    return itk.simple.PixelIDValueEnum.sitkUInt8;

                case PixelType.UInt16:
                    return itk.simple.PixelIDValueEnum.sitkUInt16;

                case PixelType.UInt32:
                    return itk.simple.PixelIDValueEnum.sitkUInt32;

                case PixelType.UInt64:
                    return itk.simple.PixelIDValueEnum.sitkUInt64;

                case PixelType.Float:
                    return itk.simple.PixelIDValueEnum.sitkFloat32;

                case PixelType.Double:
                    return itk.simple.PixelIDValueEnum.sitkFloat64;

                default:
                    throw new NotImplementedException();
            }
        }

        public static Type PixelType2Type(PixelType pixelType)
        {
            switch (pixelType)
            {
                case PixelType.Int8:
                    return typeof(short);

                case PixelType.Int16:
                    return typeof(Int16);

                case PixelType.Int32:
                    return typeof(Int32);

                case PixelType.Int64:
                    return typeof(Int64);

                case PixelType.UInt8:
                    return typeof(ushort);

                case PixelType.UInt16:
                    return typeof(UInt16);

                case PixelType.UInt32:
                    return typeof(UInt32);

                case PixelType.UInt64:
                    return typeof(UInt64);

                case PixelType.Float:
                    return typeof(float);

                case PixelType.Double:
                    return typeof(double);

                default:
                    throw new NotImplementedException();
            }
        }

        public static PixelType ItkPixelIDValue2PixelType(int pixelIdValue)
        {
            if (pixelIdValue == itk.simple.PixelIDValueEnum.sitkInt8.swigValue) return PixelType.Int8;
            else if (pixelIdValue == itk.simple.PixelIDValueEnum.sitkInt16.swigValue) return PixelType.Int16;
            else if (pixelIdValue == itk.simple.PixelIDValueEnum.sitkInt32.swigValue) return PixelType.Int32;
            else if (pixelIdValue == itk.simple.PixelIDValueEnum.sitkUInt8.swigValue) return PixelType.UInt8;
            else if (pixelIdValue == itk.simple.PixelIDValueEnum.sitkUInt16.swigValue) return PixelType.UInt16;
            else if (pixelIdValue == itk.simple.PixelIDValueEnum.sitkUInt32.swigValue) return PixelType.UInt32;
            else if (pixelIdValue == itk.simple.PixelIDValueEnum.sitkFloat32.swigValue) return PixelType.Float;
            else if (pixelIdValue == itk.simple.PixelIDValueEnum.sitkFloat64.swigValue) return PixelType.Double;
            else throw new NotImplementedException();
        }

        public static Type PixelIdValue2Type(int pixelIdValue)
        {
            if (pixelIdValue == itk.simple.PixelIDValueEnum.sitkInt8.swigValue) return typeof(sbyte);
            else if (pixelIdValue == itk.simple.PixelIDValueEnum.sitkInt16.swigValue) return typeof(short);
            else if (pixelIdValue == itk.simple.PixelIDValueEnum.sitkInt32.swigValue) return typeof(int);
            else if (pixelIdValue == itk.simple.PixelIDValueEnum.sitkUInt8.swigValue) return typeof(byte);
            else if (pixelIdValue == itk.simple.PixelIDValueEnum.sitkUInt16.swigValue) return typeof(ushort);
            else if (pixelIdValue == itk.simple.PixelIDValueEnum.sitkUInt32.swigValue) return typeof(uint);
            else if (pixelIdValue == itk.simple.PixelIDValueEnum.sitkFloat32.swigValue) return typeof(float);
            else if (pixelIdValue == itk.simple.PixelIDValueEnum.sitkFloat64.swigValue) return typeof(double);
            else throw new NotImplementedException();
        }

        public static int Type2PixelIDValue(Type type)
        {
            return SimpleITKHelper.Type2PixelID(type).swigValue;
        }

        public static itk.simple.PixelIDValueEnum Type2PixelID(Type type)
        {
            itk.simple.PixelIDValueEnum pixelIdValue = itk.simple.PixelIDValueEnum.sitkUnknown;

            var @switch = new Dictionary<Type, Action> {
                { typeof(sbyte), () => pixelIdValue = itk.simple.PixelIDValueEnum.sitkInt8 },
                { typeof(short), () => pixelIdValue = itk.simple.PixelIDValueEnum.sitkInt16 },
                { typeof(int), () => pixelIdValue = itk.simple.PixelIDValueEnum.sitkInt32 },
                { typeof(long), () => pixelIdValue = itk.simple.PixelIDValueEnum.sitkInt64 },
                { typeof(byte), () => pixelIdValue = itk.simple.PixelIDValueEnum.sitkUInt8 },
                { typeof(ushort), () => pixelIdValue = itk.simple.PixelIDValueEnum.sitkUInt16 },
                { typeof(uint), () => pixelIdValue = itk.simple.PixelIDValueEnum.sitkUInt32 },
                { typeof(ulong), () => pixelIdValue = itk.simple.PixelIDValueEnum.sitkUInt64 },
                { typeof(float), () => pixelIdValue = itk.simple.PixelIDValueEnum.sitkFloat32 },
                { typeof(double), () => pixelIdValue = itk.simple.PixelIDValueEnum.sitkFloat64 }
            };

            @switch[type]();

            return pixelIdValue;
        }

        public static PixelType Type2PixelType(Type type)
        {
            PixelType pixelType = PixelType.Unknown;

            var @switch = new Dictionary<Type, Action> {
                { typeof(sbyte), () => pixelType = PixelType.Int8 },
                { typeof(short), () => pixelType = PixelType.Int16 },
                { typeof(int), () => pixelType = PixelType.Int32 },
                { typeof(long), () => pixelType = PixelType.Int64 },
                { typeof(byte), () => pixelType = PixelType.UInt8 },
                { typeof(ushort), () => pixelType = PixelType.UInt16 },
                { typeof(uint), () => pixelType = PixelType.UInt32 },
                { typeof(ulong), () => pixelType = PixelType.UInt64 },
                { typeof(float), () => pixelType = PixelType.Float },
                { typeof(double), () => pixelType = PixelType.Double }
            };

            @switch[type]();

            return pixelType;
        }

        public static itk.simple.Image ConstructImage(uint width, uint height, uint depth, Type pixelType)
        {
            itk.simple.Image itkImage = new itk.simple.Image(width, height, depth, SimpleITKHelper.Type2PixelID(pixelType));

            VectorDouble direction = (depth > 0)

                ? new VectorDouble(new double[] {1, 0, 0,
                                                 0, 1, 0,
                                                 0, 0, 1})

                : new VectorDouble(new double[] {1, 0,
                                                 0, 1});

            itkImage.SetDirection(direction);

            VectorDouble origin = (depth) > 0
                ? new VectorDouble(new double[] { 0, 0, 0 })
                : new VectorDouble(new double[] { 0, 0 });

            itkImage.SetOrigin(origin);

            VectorDouble spacing = (depth > 0)
                ? new VectorDouble(new double[] { 1, 1, 1 })
                : new VectorDouble(new double[] { 1, 1 });

            itkImage.SetSpacing(spacing);

            return itkImage;
        }
    }
}