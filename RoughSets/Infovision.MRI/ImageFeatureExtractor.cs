using System;
using System.Collections.Generic;
using System.Data;

namespace Infovision.MRI
{
    public class ImageFeatureExtractor
    {
        private Dictionary<string, IImageFeature> generators = new Dictionary<string, IImageFeature>();

        public uint ImageWidth { get; set; }
        public uint ImageHeight { get; set; }
        public uint ImageDepth { get; set; }

        public DataTable GetDataTable(uint x0, uint y0, uint z0, uint xn, uint yn, uint zn)
        {
            if (xn > this.ImageWidth
                || yn > this.ImageHeight
                || zn > this.ImageDepth)
            {
                throw new ArgumentOutOfRangeException();
            }

            DataTable dataTable = new DataTable();
            foreach (KeyValuePair<string, IImageFeature> kvp in generators)
                dataTable.Columns.Add(kvp.Key);

            uint[] position = new uint[3];
            for (uint z = z0; z < zn; z++)
            {
                for (uint y = y0; y < yn; y++)
                {
                    for (uint x = x0; x < xn; x++)
                    {
                        position[0] = x;
                        position[1] = y;
                        position[2] = z;

                        DataRow row = dataTable.NewRow();
                        foreach (KeyValuePair<string, IImageFeature> kvp in generators)
                        {
                            row[kvp.Key] = kvp.Value.GetValue(position);
                        }
                        dataTable.Rows.Add(row);
                    }
                }
            }

            return dataTable;
        }

        public DataTable GetDataTable()
        {
            return this.GetDataTable(0, 0, 0, this.ImageWidth, this.ImageHeight, this.ImageDepth);
        }

        public void AddFeature(IImageFeature imageFeature, string name)
        {
            generators.Add(name, imageFeature);
        }
    }
}