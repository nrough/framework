using System.Collections.Generic;
using System.Data;

namespace Infovision.MRI
{
    public class ImageFeatureExtractor
    {
        private Dictionary<string, IImageFeature> generators = new Dictionary<string, IImageFeature>();

        public ImageFeatureExtractor()
        {
        }

        public uint ImageWidth
        {
            get;
            set;
        }

        public uint ImageHeight
        {
            get;
            set;
        }

        public uint ImageDepth
        {
            get;
            set;
        }

        public DataTable GetDataTable()
        {
            DataTable dataTable = new DataTable();
            foreach (KeyValuePair<string, IImageFeature> kvp in generators)
            {
                dataTable.Columns.Add(kvp.Key);     
            }

            uint[] position = new uint[3];

            for (uint z = 0; z < this.ImageDepth; z++)
            {
                for (uint y = 0; y < this.ImageHeight; y++)
                {
                    for (uint x = 0; x < this.ImageWidth; x++)
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

        public void AddFeature(IImageFeature imageFeature, string name)
        {
            generators.Add(name, imageFeature);
        }
    }
}
