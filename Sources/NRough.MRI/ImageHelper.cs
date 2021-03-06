﻿// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace NRough.MRI
{
    public class ImageHelper
    {
        public static double MaxPixelValue(PixelType pixelType)
        {
            switch (pixelType)
            {
                case PixelType.Double: return (double)Double.MaxValue;
                case PixelType.Int16: return (double)Int16.MaxValue;
                case PixelType.UInt8: return (double)Byte.MaxValue;
                case PixelType.UInt16: return (double)UInt16.MaxValue;
                case PixelType.UInt32: return (double)UInt32.MaxValue;
                case PixelType.Int8: return (double)sbyte.MaxValue;
                case PixelType.Int32: return (double)Int32.MaxValue;
                case PixelType.Float: return (double)float.MaxValue;
                case PixelType.Int64: return (double)Int64.MaxValue;
                case PixelType.UInt64: return (double)Int64.MaxValue;

                default:
                    throw new InvalidOperationException("Unknown pixel type");
            }
        }

        public static double MaxPixelValue(Type pixelType)
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
                { typeof(double), () => maxPixelValue = (double)double.MaxValue },
                { typeof(long), () => maxPixelValue = (double)long.MaxValue },
                { typeof(ulong), () => maxPixelValue = (double)ulong.MaxValue }
            };

            @switch[pixelType]();

            return maxPixelValue;
        }

        public static int PixelSize(PixelType pixelType)
        {
            switch (pixelType)
            {
                case PixelType.Double: return sizeof(double);
                case PixelType.Int16: return sizeof(short);
                case PixelType.UInt8: return sizeof(byte);
                case PixelType.UInt16: return sizeof(ushort);
                case PixelType.UInt32: return sizeof(uint);
                case PixelType.Int8: return sizeof(sbyte);
                case PixelType.Int32: return sizeof(int);
                case PixelType.Float: return sizeof(float);
                case PixelType.Int64: return sizeof(long);
                case PixelType.UInt64: return sizeof(ulong);

                default:
                    throw new InvalidOperationException("Unknown pixel type");
            }
        }

        public static int GetNumberOfHistogramBuckets(IImage image, int bucketSize)
        {
            if (bucketSize <= 0)
            {
                throw new ArgumentOutOfRangeException("bucketSize", "Parameter bucketSize should be grater than zero");
            }

            return (int)Math.Ceiling(ImageHelper.MaxPixelValue(image.PixelType) / (double)bucketSize);
        }

        public static Bitmap Convert(byte[] input, int width, int height, int bits)
        {
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            uint[] lut = CreateLut(bits);
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
            ConvertCore(width, height, bits, input, bitmapData, lut);
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }

        private static unsafe void ConvertCore(int width, int height, int bits, byte[] input, BitmapData output, uint[] lut)
        {
            // Copy pixels from input to output, applying LUT
            ushort mask = (ushort)((1 << bits) - 1);

            int inStride = output.Stride;
            int outStride = width * 2;

            byte* outData = (byte*)output.Scan0;

            fixed (byte* inData = input)
            {
                for (int y = 0; y < height; y++)
                {
                    ushort* inRow = (ushort*)(inData + (y * outStride));
                    uint* outRow = (uint*)(outData + (y * inStride));

                    for (int x = 0; x < width; x++)
                    {
                        ushort inPixel = (ushort)(inRow[x] & mask);
                        outRow[x] = lut[inPixel];
                    }
                }
            }
        }

        private static uint[] CreateLut(int bits)
        {
            // Construct a linear LUT to convert from grayscale to ARGB
            int maxInput = 1 << bits;
            uint[] lut = new uint[maxInput];
            for (int i = 0; i < maxInput; i++)
            {
                // map input key to 8-bit range
                byte intensity = (byte)((i * 0xFF) / maxInput);

                // create ARGB output key A=255, R=G=B=intensity
                lut[i] = (uint)(0xFF000000L | (intensity * 0x00010101L));
            }

            return lut;
        }

        private static byte[] Wedge(int width, int height, int bits)
        {
            // horizontal wedge
            int max = 1 << bits;

            byte[] pixels = new byte[width * height * 2];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pixel = x % max;
                    int addr = ((y * width) + x) * 2;
                    pixels[addr + 1] = (byte)((pixel & 0xFF00) >> 8);
                    pixels[addr + 0] = (byte)((pixel & 0x00FF));
                }
            }

            return pixels;
        }
    }
}