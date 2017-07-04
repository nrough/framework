//
// NRough Framework
// http://www.nrough.net
//
// Copyright © Sebastian Widz, 2017
// authors at nrough.net

//This file is part of NRough.

//Foobar is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 3 of the License, or
//(at your option) any later version.
//
//Foobar is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Foobar; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using itk.simple;

namespace NRough.MRI
{
    [Serializable]
    public class ImageITK : ImageBase
    {
        [NonSerialized]
        private itk.simple.Image image;

        [NonSerialized]
        private VectorUInt32 positionVector;

        public ImageITK()
        {
        }

        public ImageITK(itk.simple.Image image)
            : this()
        {
            this.ItkImage = image;
            this.Width = image.GetWidth();
            this.Height = image.GetHeight();
            this.Depth = image.GetDepth();
            this.PixelType = SimpleITKHelper.PixelIdValue2Type(image.GetPixelIDValue());
            this.PixelTypeId = SimpleITKHelper.ItkPixelIDValue2PixelType(image.GetPixelIDValue());
        }

        public ImageITK(IImage image)
            : this()
        {
            this.Width = image.Width;
            this.Height = image.Height;
            this.Depth = image.Depth;
            this.PixelType = image.PixelType;
            this.ItkImage = SimpleITKHelper.ConstructImage(this.Width, this.Height, this.Depth, this.PixelType);

            var setPixelMethod = typeof(ImageITK).GetMethod("SetPixel");
            var setPixelRef = setPixelMethod.MakeGenericMethod(this.PixelType);

            var getPixelMethod = typeof(IImage).GetMethod("GetPixel");
            var getPixelRef = getPixelMethod.MakeGenericMethod(this.PixelType);

            uint[] position = new uint[3];

            for (uint z = 0; z < this.Depth; z++)
                for (uint y = 0; y < this.Height; y++)
                    for (uint x = 0; x < this.Width; x++)
                    {
                        position[0] = x;
                        position[1] = y;
                        position[2] = z;

                        setPixelRef.Invoke(this, new object[] { position, getPixelRef.Invoke(image, new object[] { position }) });
                    }
        }

        public itk.simple.Image ItkImage
        {
            get { return this.image; }

            set
            {
                this.image = value;
                if (this.image != null)
                {
                    this.Width = image.GetWidth();
                    this.Height = image.GetHeight();
                    this.Depth = image.GetDepth();
                    this.PixelType = SimpleITKHelper.PixelIdValue2Type(this.image.GetPixelIDValue());
                    this.PixelTypeId = SimpleITKHelper.ItkPixelIDValue2PixelType(this.image.GetPixelIDValue());

                    if (this.Depth > 0)
                    {
                        positionVector = new VectorUInt32(new uint[] { 0, 0, 0 });
                    }
                    else
                    {
                        positionVector = new VectorUInt32(new uint[] { 0, 0 });
                    }
                }
            }
        }

        public override Bitmap GetBitmap()
        {
            return SimpleITKHelper.ConvertToBitmap(this.ItkImage);
        }

        public override Bitmap GetBitmap(uint index)
        {
            return SimpleITKHelper.ConvertToBitmap(this.ItkImage, (int)index);
        }

        private void InitPositionVector()
        {
            if (positionVector == null)
            {
                if (this.Depth > 1)
                {
                    positionVector = new VectorUInt32(new uint[] { 0, 0, 0 });
                }
                else
                {
                    positionVector = new VectorUInt32(new uint[] { 0, 0 });
                }
            }
        }

        private void SetPositionVector(uint[] position)
        {
            this.InitPositionVector();

            positionVector[0] = position[0];
            positionVector[1] = position[1];

            if (this.Depth > 1)
            {
                positionVector[2] = position[2];
            }
        }

        public override T GetPixel<T>(uint[] position)
        {
            T ret = default(T);
            if (this.PixelType == typeof(double))
            {
                double value;
                this.GetPixel(position, out value);
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else if (this.PixelType == typeof(int))
            {
                int value;
                this.GetPixel(position, out value);
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else if (this.PixelType == typeof(short))
            {
                short value;
                this.GetPixel(position, out value);
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else if (this.PixelType == typeof(byte))
            {
                byte value;
                this.GetPixel(position, out value);
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else if (this.PixelType == typeof(sbyte))
            {
                sbyte value;
                this.GetPixel(position, out value);
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else if (this.PixelType == typeof(ushort))
            {
                ushort value;
                this.GetPixel(position, out value);
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else if (this.PixelType == typeof(uint))
            {
                uint value;
                this.GetPixel(position, out value);
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else if (this.PixelType == typeof(float))
            {
                float value;
                this.GetPixel(position, out value);
                return (T)Convert.ChangeType(value, typeof(T));
            }

            return ret;
        }

        private void GetPixel(uint[] position, out double value)
        {
            this.SetPositionVector(position);
            value = this.ItkImage.GetPixelAsDouble(this.positionVector);
        }

        private void GetPixel(uint[] position, out int value)
        {
            this.SetPositionVector(position);
            value = this.ItkImage.GetPixelAsInt32(this.positionVector);
        }

        private void GetPixel(uint[] position, out uint value)
        {
            this.SetPositionVector(position);
            value = this.ItkImage.GetPixelAsUInt32(this.positionVector);
        }

        private void GetPixel(uint[] position, out short value)
        {
            this.SetPositionVector(position);
            value = this.ItkImage.GetPixelAsInt16(this.positionVector);
        }

        private void GetPixel(uint[] position, out ushort value)
        {
            this.SetPositionVector(position);
            value = this.ItkImage.GetPixelAsUInt16(this.positionVector);
        }

        private void GetPixel(uint[] position, out byte value)
        {
            this.SetPositionVector(position);
            value = this.ItkImage.GetPixelAsUInt8(this.positionVector);
        }

        private void GetPixel(uint[] position, out sbyte value)
        {
            this.SetPositionVector(position);
            value = this.ItkImage.GetPixelAsInt8(this.positionVector);
        }

        private void GetPixel(uint[] position, out float value)
        {
            this.SetPositionVector(position);
            value = this.ItkImage.GetPixelAsFloat(this.positionVector);
        }

        public override void SetPixel<T>(uint[] position, T pixelValue)
        {
            Type t = typeof(T);
            if (t == typeof(double))
            {
                this.SetPixel(position, (double)Convert.ChangeType(pixelValue, typeof(double)));
            }
            else if (t == typeof(ushort))
            {
                this.SetPixel(position, (ushort)Convert.ChangeType(pixelValue, typeof(ushort)));
            }
            else if (t == typeof(short))
            {
                this.SetPixel(position, (short)Convert.ChangeType(pixelValue, typeof(short)));
            }
            else if (t == typeof(int))
            {
                this.SetPixel(position, (int)Convert.ChangeType(pixelValue, typeof(int)));
            }
            else if (t == typeof(byte))
            {
                this.SetPixel(position, (byte)Convert.ChangeType(pixelValue, typeof(byte)));
            }
            else if (t == typeof(sbyte))
            {
                this.SetPixel(position, (sbyte)Convert.ChangeType(pixelValue, typeof(sbyte)));
            }
            else if (t == typeof(uint))
            {
                this.SetPixel(position, (uint)Convert.ChangeType(pixelValue, typeof(uint)));
            }
            else if (t == typeof(float))
            {
                this.SetPixel(position, (float)Convert.ChangeType(pixelValue, typeof(float)));
            }
            else
            {
                throw new System.InvalidOperationException();
            }
        }

        private void SetPixel(uint[] position, sbyte pixelValue)
        {
            this.SetPositionVector(position);
            this.ItkImage.SetPixelAsInt8(this.positionVector, pixelValue);
        }

        private void SetPixel(uint[] position, short pixelValue)
        {
            this.SetPositionVector(position);
            this.ItkImage.SetPixelAsInt16(this.positionVector, pixelValue);
        }

        private void SetPixel(uint[] position, int pixelValue)
        {
            this.SetPositionVector(position);
            this.ItkImage.SetPixelAsInt32(this.positionVector, pixelValue);
        }

        private void SetPixel(uint[] position, byte pixelValue)
        {
            this.SetPositionVector(position);
            this.ItkImage.SetPixelAsUInt8(this.positionVector, pixelValue);
        }

        private void SetPixel(uint[] position, ushort pixelValue)
        {
            this.SetPositionVector(position);
            this.ItkImage.SetPixelAsUInt16(this.positionVector, pixelValue);
        }

        private void SetPixel(uint[] position, uint pixelValue)
        {
            this.SetPositionVector(position);
            this.ItkImage.SetPixelAsUInt32(this.positionVector, pixelValue);
        }

        private void SetPixel(uint[] position, float pixelValue)
        {
            this.SetPositionVector(position);
            this.ItkImage.SetPixelAsFloat(this.positionVector, pixelValue);
        }

        private void SetPixel(uint[] position, double pixelValue)
        {
            this.SetPositionVector(position);
            this.ItkImage.SetPixelAsDouble(this.positionVector, pixelValue);
        }

        public override void Save(string filename)
        {
            itk.simple.ImageFileWriter fileWriter = new itk.simple.ImageFileWriter();
            fileWriter.SetFileName(filename);
            fileWriter.Execute(this.ItkImage);
        }

        public virtual void SaveRaw(string fileName)
        {
            SimpleITKHelper.WriteImageRaw(this.ItkImage, fileName);
        }

        public override T[] GetData<T>()
        {
            return this.GetITKImageData<T>(this.ItkImage);
        }

        private T[] GetITKImageData<T>(itk.simple.Image itkImage)
        {
            int numerOfPixels = itkImage.GetDepth() > 0
                ? (int)itkImage.GetWidth() * (int)itkImage.GetHeight() * (int)itkImage.GetDepth()
                : (int)itkImage.GetWidth() * (int)itkImage.GetHeight();

            Array array = Array.CreateInstance(typeof(T), numerOfPixels);
            itk.simple.Image imageConverted = SimpleITK.Cast(itkImage, SimpleITKHelper.Type2PixelID(typeof(T)));

            var @switch = new Dictionary<Type, Action> {
                { typeof(sbyte), () => Marshal.Copy((IntPtr)imageConverted.GetBufferAsInt8(), (byte[])array, (int)0, (int)numerOfPixels) },
                { typeof(short), () => Marshal.Copy((IntPtr)imageConverted.GetBufferAsInt16(), (short[])array, (int)0, (int)numerOfPixels) },
                { typeof(int), () => Marshal.Copy((IntPtr)imageConverted.GetBufferAsInt32(), (int[])array, (int)0, (int)numerOfPixels) },
                { typeof(byte), () => Marshal.Copy((IntPtr)imageConverted.GetBufferAsUInt8(), (byte[])array, (int)0, (int)numerOfPixels) },
                { typeof(ushort), () => Marshal.Copy((IntPtr)imageConverted.GetBufferAsUInt16(), (short[])array, (int)0, (int)numerOfPixels) },
                { typeof(uint), () => Marshal.Copy((IntPtr)imageConverted.GetBufferAsUInt32(), (int[])array, (int)0, (int)numerOfPixels) },
                { typeof(float), () => Marshal.Copy((IntPtr)imageConverted.GetBufferAsFloat(), (float[])array, (int)0, (int)numerOfPixels) },
                { typeof(double), () => Marshal.Copy((IntPtr)imageConverted.GetBufferAsDouble(), (double[])array, (int)0, (int)numerOfPixels) }
            };

            @switch[typeof(T)]();

            return (T[])array;
        }

        public static ImageITK Construct(uint width, uint height, uint depth, Type pixelType)
        {
            ImageITK image = new ImageITK
            {
                ItkImage = SimpleITKHelper.ConstructImage(width, height, depth, pixelType)
            };

            return image;
        }

        public static ImageITK GetImageITK(IImage image)
        {
            if (image is ImageITK)
                return image as ImageITK;
            ImageITK ret = new ImageITK(image);
            return ret;
        }

        public static ImageITK ReadImageRAW(string fileName,
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

            return new ImageITK(importImageFilter.Execute());
        }

        public virtual void Show()
        {
            SimpleITK.Show(this.ItkImage);
        }

        public virtual void Show(string title)
        {
            SimpleITK.Show(this.ItkImage, title);
        }

        public static void Show(ImageITK image)
        {
            SimpleITK.Show(image.ItkImage);
        }

        public static void Show(ImageITK image, string title)
        {
            SimpleITK.Show(image.ItkImage, title);
        }

        public override IImage Extract(int z)
        {
            if (this.Depth < z)
            {
                throw new ArgumentException("z", "Slice index cannot exceed image depth");
            }

            itk.simple.Image slice = SimpleITK.Extract(this.ItkImage,
                                            new VectorUInt32(new uint[] { this.ItkImage.GetWidth(), this.ItkImage.GetHeight(), 0 }),
                                            new VectorInt32(new int[] { 0, 0, z }),
                                            ExtractImageFilter.DirectionCollapseToStrategyType.DIRECTIONCOLLAPSETOSUBMATRIX);

            return new ImageITK(slice);
        }
    }

    [Serializable]
    public class ImageITKRaw : ImageITK
    {
        public ImageITKRaw()
            : base()
        {
        }

        public ImageITKRaw(itk.simple.Image image)
            : base(image)
        {
        }

        public ImageITKRaw(string fileName,
                           uint width,
                           uint height,
                           uint depth,
                           PixelType pixelType,
                           Endianness endianness = Endianness.LittleEndian,
                           uint header = 0)
        {
            this.Width = width;
            this.Height = height;
            this.Depth = depth;
            this.PixelTypeId = pixelType;

            this.ItkImage = SimpleITKHelper.ReadImageRAW(fileName,
                                                         this.Width,
                                                         this.Height,
                                                         this.Depth,
                                                         SimpleITKHelper.PixelType2ItkPixelIDValue(this.PixelTypeId),
                                                         endianness,
                                                         header);
        }
    }

    [Serializable]
    public class ImageBitmap : ImageBase
    {
        private Bitmap bitmap;

        public ImageBitmap()
            : base()
        {
        }

        public ImageBitmap(Bitmap image)
            : this()
        {
            this.Bitmap = image;
        }

        public Bitmap Bitmap
        {
            get { return this.bitmap; }
            set
            {
                this.bitmap = value;
                this.Width = (uint)this.bitmap.Width;
                this.Height = (uint)this.bitmap.Height;
                this.PixelType = typeof(int);
                this.PixelTypeId = NRough.MRI.PixelType.Int32;
            }
        }

        public override Bitmap GetBitmap()
        {
            return this.bitmap;
        }

        public override Bitmap GetBitmap(uint z)
        {
            return this.bitmap;
        }
    }
}