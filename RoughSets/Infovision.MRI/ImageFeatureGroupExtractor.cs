using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Infovision.Test;
using Infovision.Utils;

namespace Infovision.MRI
{
    public class ImageFeatureGroupExtractor
    {

        private Dictionary<string, IImageFeature> generators = new Dictionary<string, IImageFeature>();
        private Dictionary<string, ParameterList> generatorsParms = new Dictionary<string, ParameterList>();

        public ImageFeatureGroupExtractor()
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
            return this.GetDataTable(0, (int) this.ImageWidth - 1, 0, (int) this.ImageHeight - 1, 0, (int) this.ImageDepth - 1);
        }

        public DataTable GetDataTable(int xStart, int xEnd, int yStart, int yEnd, int zStart, int zEnd)
        {
            int objectId = 0;
            DataTable dataTable = new DataTable();
            
            DataColumn objectIdColumn = dataTable.Columns.Add("Id", typeof(Int64));
            DataColumn positionXColumn = dataTable.Columns.Add("Position_X", typeof(uint));
            DataColumn positionYColumn = dataTable.Columns.Add("Position_Y", typeof(uint));
            DataColumn positionZColumn = dataTable.Columns.Add("Position_Z", typeof(uint));

            DataRow dataRow;

            for (int z = zStart; z <= zEnd; z++)
            {
                for (int y = yStart; y <= yStart; y++)
                {
                    for (int x = xStart; x <= xEnd; x++)
                    {
                        objectId++;
                        dataRow = dataTable.NewRow();
                        dataRow["Id"] = objectId;
                        
                        dataRow["Position_X"] = (uint) x;
                        dataRow["Position_Y"] = (uint) y;
                        dataRow["Position_Z"] = (uint) z;

                        dataTable.Rows.Add(dataRow);
                    }
                }
            }

            uint[] position = new uint[3];

            IImageFeature generator;
            ParameterList generatorParameterList;
            ParameterVectorEnumerator i_parm;
            string generatorName;
            foreach (KeyValuePair<string, IImageFeature> kvp in generators)
            {
                generatorName = kvp.Key;
                generator = kvp.Value;
                generatorParameterList = generatorsParms[generatorName];

                int i = 0;

                if (generatorParameterList != null)
                {
                    i_parm = (ParameterVectorEnumerator) generatorParameterList.ParmValueEnumerator;
        
                    while (i_parm.MoveNext())
                    {
                        ParameterValueVector parameterVector = (ParameterValueVector)i_parm.Current;
                        Args args = new Args(generatorParameterList.GetParameterNames(), parameterVector.GetArray());
                        this.SetGeneratorProperties(generator, args);

                        i++;
                        string columnName = generatorName + Convert.ToString(i);
                        DataColumn newColumn = new DataColumn(columnName);
                        newColumn.AllowDBNull = true;

                        dataTable.Columns.Add(newColumn);

                        foreach (DataRow row in dataTable.Rows)
                        {
                            position[0] = (uint) row["Position_X"];
                            position[1] = (uint) row["Position_Y"];
                            position[2] = (uint) row["Position_Z"];

                            row[columnName] = generator.GetValue(position);

                        }

                    }

                }
            }
            
            return dataTable;
        }

        private void SetGeneratorProperties(IImageFeature generator, Args properties)
        {
            foreach (KeyValuePair<string, object> arg in properties)
            {
                generator.GetType().InvokeMember(arg.Key,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty,
                    Type.DefaultBinder, generator, new object[] { arg.Value } );
            }
        }

        public void AddFeatureGenerator(string name, IImageFeature generator, ParameterList parameterList)
        {
            generators.Add(name, generator);
            generatorsParms.Add(name, parameterList);
        }
    }
}
