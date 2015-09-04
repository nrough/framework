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
    public class PermutationList : ICloneable
    {
        #region Globals

        private List<Permutation> internalList;

        #endregion

        #region Constructors

        public PermutationList()
        {
            this.internalList = new List<Permutation>();
        }

        public PermutationList(List<Permutation> permutationList)
        {
            this.internalList = (List<Permutation>)permutationList.Clone();
        }

        public PermutationList(PermutationList permutationList)
        {
            this.internalList = (List<Permutation>)permutationList.InternalList.Clone();
        }

        public PermutationList(Permutation permutation)
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

        public static PermutationList LoadFromCsvFile(string fileName)
        {
            PermutationList permutationList = new PermutationList();
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

        public static void SaveToCsvFile(PermutationList permutationList, string fileName)
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

        public static PermutationList LoadFromJson(String jsonText)
        {
            PermutationList permutationList = new PermutationList();
            JObject json = JObject.Parse(jsonText);

            return permutationList;
        }

        public static String GetJson(PermutationList permutationList)
        {
            return permutationList.ToString();
        }

        public static PermutationList LoadFromJsonFile(string fileName)
        {
            String jsonText = String.Empty;
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    jsonText = streamReader.ReadToEnd();
                }
            }

            return PermutationList.LoadFromJson(jsonText);
        }

        public static void SaveToJsonFile(PermutationList permutationList, string fileName)
        {
            using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.Write(PermutationList.GetJson(permutationList));
                }
            }
        }

        public void Add(Permutation permutation)
        {
            this.internalList.Add(permutation);
        }
        
        public IEnumerator GetEnumerator()
        {
            return internalList.GetEnumerator();
        }

        #region ICloneable Members

        public object Clone()
        {
            return new PermutationList(this);
        }

        #endregion

        #endregion
    }
}
