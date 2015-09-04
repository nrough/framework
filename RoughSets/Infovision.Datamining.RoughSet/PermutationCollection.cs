using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Infovision.Utils;
using Newtonsoft.Json.Linq;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class PermutationCollection : ICloneable, IEnumerable<Permutation>
    {
        #region Globals

        private List<Permutation> internalList;

        #endregion

        #region Constructors

        public PermutationCollection()
        {
            this.internalList = new List<Permutation>();
        }

        public PermutationCollection(List<Permutation> permutationList)
        {
            this.internalList = (List<Permutation>)permutationList.Clone();
        }

        public PermutationCollection(PermutationCollection permutationList)
        {
            this.internalList = (List<Permutation>)permutationList.InternalList.Clone();
        }

        public PermutationCollection(Permutation permutation)
        {
            this.internalList = new List<Permutation>(1);
            this.internalList.Add(permutation);
        }

        #endregion

        #region Properties

        public List<Permutation> InternalList
        {
            get { return this.internalList; }
        }

        public int Count
        {
            get { return internalList.Count; }
        }

        public Permutation this[int index]
        {
            get { return internalList[index]; }
        }

        #endregion

        #region Methods

        public static PermutationCollection LoadFromCsvFile(string fileName)
        {
            PermutationCollection permutationList = new PermutationCollection();
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    string line = streamReader.ReadLine();
                    while ( ! String.IsNullOrEmpty(line))
                    {
                        string [] values = line.Split(new Char [] {' ', ';', ',', '\t', '|'}, StringSplitOptions.RemoveEmptyEntries); 
                        int [] elements = new int[values.Length];
                        for(int i= 0 ; i < values.Length; i++)
                        {
                            elements[i] = Int32.Parse(values[i], CultureInfo.InvariantCulture);
                        }
                        permutationList.Add(new Permutation(elements));
                        line = streamReader.ReadLine();
                    }
                }
            }
            return permutationList;
        }

        public static void SaveToCsvFile(PermutationCollection permutationList, string fileName)
        {
            using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                {
                    foreach (Permutation permutation in permutationList)
                    {
                        streamWriter.WriteLine(permutation);
                    }
                }
            }
        }

        public static PermutationCollection LoadFromJson(String jsonText)
        {
            PermutationCollection permutationList = new PermutationCollection();
            JObject json = JObject.Parse(jsonText);

            return permutationList;
        }

        public static String GetJson(PermutationCollection permutationList)
        {
            return permutationList.ToString();
        }

        public static PermutationCollection LoadFromJsonFile(string fileName)
        {
            String jsonText = String.Empty;
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    jsonText = streamReader.ReadToEnd();
                }
            }

            return PermutationCollection.LoadFromJson(jsonText);
        }

        public static void SaveToJsonFile(PermutationCollection permutationList, string fileName)
        {
            using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.Write(PermutationCollection.GetJson(permutationList));
                }
            }
        }

        public void Add(Permutation permutation)
        {
            this.internalList.Add(permutation);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return internalList.GetEnumerator();
        }

        public IEnumerator<Permutation> GetEnumerator()
        {
            return internalList.GetEnumerator();
        }

        #region ICloneable Members

        public object Clone()
        {
            return new PermutationCollection(this);
        }

        #endregion

        #endregion
    }
}
