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
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace NRough.MRI
{
    [Serializable]
    public abstract class ImageBase : IImage, ISerializable
    {
        private uint depth;

        public ImageBase()
            : base()
        {
            this.PixelType = typeof(Int16);
            this.Depth = 1;
        }

        protected ImageBase(SerializationInfo si, StreamingContext context)
            : this()
        {
            this.depth = si.GetUInt32("depth");
            this.Height = si.GetUInt32("Height");
            this.Width = si.GetUInt32("Width");
            this.PixelType = (Type)si.GetValue("PixelType", typeof(Type));

            var localData = si.GetValue("innerImageData", this.PixelType.MakeArrayType());
            Type localType = this.PixelType;

            var mi = typeof(ImageBase).GetMethod("SetImageData");
            var mRef = mi.MakeGenericMethod(this.PixelType.MakeArrayType());
            mRef.Invoke(this, new object[] { localData });
        }

        public virtual uint Depth
        {
            get
            {
                return this.depth;
            }

            set
            {
                if (value == 0)
                    this.depth = 1;
                else
                    this.depth = value;
            }
        }

        public virtual uint Height { get; set; }
        public virtual uint Width { get; set; }
        public virtual Type PixelType { get; set; }
        public virtual PixelType PixelTypeId { get; set; }

        #region IImage interface implementation

        public virtual IImage Extract(int z)
        {
            throw new System.NotImplementedException("Extract method is not implemented");
        }

        public virtual T GetPixel<T>(uint[] position) where T : IComparable, IConvertible
        {
            throw new System.NotImplementedException("GetPixel method is not implemented");
        }

        public virtual void SetPixel<T>(uint[] position, T pixelValue) where T : IComparable, IConvertible
        {
            throw new System.NotImplementedException("SetPixel method is not implemented");
        }

        public virtual T[] GetData<T>() where T : IComparable, IConvertible
        {
            throw new System.NotImplementedException("GetData method is not implemented");
        }

        public virtual void SetImageData<T>(T[] data) where T : IComparable, IConvertible
        {
            throw new System.NotImplementedException("SetImageData method is not implemented");
        }

        public virtual Bitmap GetBitmap()
        {
            throw new System.NotImplementedException("GetBitmap method is not implemented");
        }

        public virtual Bitmap GetBitmap(uint z)
        {
            throw new System.NotImplementedException("GetBitmap method is not implemented");
        }

        public virtual void Save(string filename)
        {
            throw new System.NotImplementedException("Save method is not implemented");
        }

        #endregion IImage interface implementation

        #region Serialization methods

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo si, StreamingContext context)
        {
            si.AddValue("depth", this.depth);
            si.AddValue("Height", this.Height);
            si.AddValue("Width", this.Width);
            si.AddValue("PixelType", this.PixelType);

            var @switch = new Dictionary<Type, Action> {
                { typeof(sbyte), () => si.AddValue("innerImageData", this.GetData<sbyte>()) },
                { typeof(short), () => si.AddValue("innerImageData", this.GetData<short>()) },
                { typeof(int), () => si.AddValue("innerImageData", this.GetData<int>()) },
                { typeof(long), () => si.AddValue("innerImageData", this.GetData<long>()) },
                { typeof(byte), () => si.AddValue("innerImageData", this.GetData<byte>()) },
                { typeof(ushort), () => si.AddValue("innerImageData", this.GetData<ushort>()) },
                { typeof(uint), () => si.AddValue("innerImageData", this.GetData<uint>()) },
                { typeof(ulong), () => si.AddValue("innerImageData", this.GetData<ulong>()) },
                { typeof(float), () => si.AddValue("innerImageData", this.GetData<float>()) },
                { typeof(double), () => si.AddValue("innerImageData", this.GetData<double>()) }
            };

            @switch[this.PixelType]();
        }

        #endregion Serialization methods
    }
}