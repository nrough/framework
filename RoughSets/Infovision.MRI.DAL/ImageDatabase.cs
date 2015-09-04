using System;
using System.Data;
using System.Text;

namespace Infovision.MRI.DAL
{
    public class ImageDatabase
    {
        public static long NextImageId = 1;
        
        public class Tables
        {
            public const string Image = "Image";
        }

        public class ImageFields
        {
            public const string Id = "Id";
            public const string Name = "Name";
            public const string ParentId = "ParentId";
            public const string Width = "Width";
            public const string Height = "Height";
            public const string Depth = "Depth";
            public const string FileName = "FileName";
            public const string Endianness = "Endianness";
            public const string PixelType = "PixelTypeId";
            public const string Header = "Header";
            public const string SliceFrom = "SliceFrom";
            public const string SliceTo = "SliceTo";
            public const string ImageType = "ImageTypeId";
        }

        private DataSet imageStore;
        private DataRelation parentImageRelation;

        public ImageDatabase()
        {
            imageStore = new DataSet();

            DataTable imageTable = new DataTable(Tables.Image);

            /*
            DataColumn column = new DataColumn();
            column.DataType = typeof(int?);
            column.AutoIncrement = true;
            column.AutoIncrementSeed = 1000;
            column.AutoIncrementStep = 10;
            imageTable.Columns.Add(column);
            */

            imageTable.Columns.Add(ImageFields.Id, typeof(long));
            imageTable.Columns.Add(ImageFields.Name, typeof(string));
            imageTable.Columns.Add(ImageFields.ParentId, typeof(long));
            imageTable.Columns.Add(ImageFields.Width, typeof(int));
            imageTable.Columns.Add(ImageFields.Height, typeof(int));
            imageTable.Columns.Add(ImageFields.Depth, typeof(int));
            imageTable.Columns.Add(ImageFields.FileName, typeof(string));
            imageTable.Columns.Add(ImageFields.Endianness, typeof(Endianness));
            imageTable.Columns.Add(ImageFields.PixelType, typeof(PixelType));
            imageTable.Columns.Add(ImageFields.Header, typeof(int));
            imageTable.Columns.Add(ImageFields.SliceFrom, typeof(int));
            imageTable.Columns.Add(ImageFields.SliceTo, typeof(int));
            imageTable.Columns.Add(ImageFields.ImageType, typeof(ImageType));

            imageStore.Tables.Add(imageTable);

            parentImageRelation = new DataRelation("ParentImage",
                imageStore.Tables[Tables.Image].Columns[ImageFields.Id], 
                imageStore.Tables[Tables.Image].Columns[ImageFields.ParentId]);
            imageStore.Relations.Add(parentImageRelation);
        }

        public DataRow[] GetChildImages(long? parentId)
        {
            DataTable imageTable = imageStore.Tables[Tables.Image];
            string expression = parentId == null ? "ParentID is null"  : "ParentId = " + parentId.ToString();

            //string sortOrder = "Id ASC";
            //return imageTable.Select(expression, sortOrder);
            return imageTable.Select(expression);
        }

        public DataRow[] GetChildImages(DataRow rowParent)
        {
            return rowParent.GetChildRows(parentImageRelation);
        }

        public string GetDisplayText(DataRow row)
        {
            StringBuilder text = new StringBuilder();

            switch (row.Table.TableName)
            {
                case Tables.Image:
                    text.AppendFormat("Id: {0} Name: {1} ParentId: {2}", row[ImageFields.Id], row[ImageFields.Name], row[ImageFields.ParentId]);
                    break;
            }
            return text.ToString();
        }

        public DataRow AddImage(Infovision.MRI.DAL.ImageRead image)
        {
            DataTable imageTable = imageStore.Tables[Tables.Image];
            DataRow row = imageTable.NewRow();

            if (image.Id == 0)
            {
                image.Id = ImageDatabase.NextImageId;
                ImageDatabase.NextImageId++;
            }

            row.SetField<long>(ImageFields.Id, image.Id);
            row.SetField<string>(ImageFields.Name, image.Name);
            row.SetField<long?>(ImageFields.ParentId, image.ParentId);
            row.SetField<int>(ImageFields.Width, (int) image.Width);
            row.SetField<int>(ImageFields.Height, (int) image.Height);
            row.SetField<int>(ImageFields.Depth, (int) image.Depth);
            row.SetField<string>(ImageFields.FileName, image.FileName);
            row.SetField<Endianness>(ImageFields.Endianness, image.Endianness);
            row.SetField<PixelType>(ImageFields.PixelType, image.PixelType);
            row.SetField<int>(ImageFields.Header, image.Header);
            row.SetField<int>(ImageFields.SliceFrom, image.SliceFrom);
            row.SetField<int>(ImageFields.SliceTo, image.SliceTo);
            row.SetField<ImageType>(ImageFields.ImageType, image.ImageTypeId);
            
            imageTable.Rows.Add(row);
            
            return row;
        }

        public Infovision.MRI.DAL.ImageRead GetImage(DataRow dataRow)
        {
            DAL.ImageRead image = new DAL.ImageRead();

            image.Id = (long)dataRow[ImageFields.Id];
            image.Name = (string)dataRow[ImageFields.Name];
            
            if (dataRow[ImageFields.ParentId] != DBNull.Value)
            {
                image.ParentId = (long)dataRow[ImageFields.ParentId];
            }
            else
            {
                image.ParentId = null;
            }

            image.Width = (uint)dataRow[ImageFields.Width];
            image.Height = (uint)dataRow[ImageFields.Height];
            image.Depth = (uint)dataRow[ImageFields.Depth];
            image.FileName = (string)dataRow[ImageFields.FileName];
            image.Endianness = (Endianness)dataRow[ImageFields.Endianness];
            image.PixelType = (PixelType)dataRow[ImageFields.PixelType];
            image.Header = (int)dataRow[ImageFields.Header];
            image.SliceFrom = (int)dataRow[ImageFields.SliceFrom];
            image.SliceTo = (int)dataRow[ImageFields.SliceTo];
            image.ImageTypeId = (ImageType)dataRow[ImageFields.ImageType];

            return image;
        }

        public Infovision.MRI.DAL.ImageRead GetImage(long id)
        {
            if (id != 0)
            {
                DataRow dataRow = imageStore.Tables[Tables.Image].Rows.Find(id);
                return this.GetImage(dataRow);
            }

            return null;
        }
    }
}
