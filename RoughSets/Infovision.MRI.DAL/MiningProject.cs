using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Raccoon.MRI.DAL
{
    [Serializable]
    public class MiningProject
    {
        private long nextId;
        private Dictionary<long, IMiningObject> miningObjects;
        private string projectFileName = String.Empty;

        public MiningProject()
        {
            Name = "New project";
            IsDirty = false;
            Init();
        }

        public string Name
        {
            get;
            set;
        }

        public string ProjectFileName
        {
            get
            {
                return this.projectFileName;
            }

            set
            {
                this.projectFileName = value;
            }
        }

        public bool IsDirty
        {
            get;
            private set;
        }

        private void Init()
        {
            nextId = 0;
            miningObjects = new Dictionary<long, IMiningObject>();
        }

        public IEnumerable<IMiningObject> GetMiningObjects()
        {
            return miningObjects.Values;
        }

        public IMiningObject GetMiningObject(long id)
        {
            if (this.miningObjects.ContainsKey(id))
            {
                return this.miningObjects[id];
            }

            return new MiningObject
            {
                TypeId = MiningObjectType.Types.Dummy,
                Id = 0,
                Name = "Dummy mining object",
            };
        }

        public void AddMiningObject(IMiningObject miningObject)
        {
            if (miningObject.Id != 0
                && miningObjects.ContainsKey(miningObject.Id))
            {
                throw new InvalidOperationException(String.Format("Mining object with Id {0} already exists", miningObject.Id));
            }

            long id = miningObject.Id != 0 ? miningObject.Id : this.GetMiningObjectId();

            miningObjects.Add(id, miningObject);
            miningObject.Id = id;

            if (nextId < id)
            {
                nextId = id;
            }

            this.IsDirty = true;
        }

        public bool RemoveMiningObject(long miningObjectId)
        {
            if (this.miningObjects.ContainsKey(miningObjectId))
            {
                this.miningObjects.Remove(miningObjectId);
                this.IsDirty = true;
                return true;
            }

            return false;
        }

        public bool RemoveMiningObject(IMiningObject miningObject)
        {
            if (miningObject == null)
                return false;

            if (this.RemoveMiningObject(miningObject.Id))
            {
                miningObject.Id = 0;
                this.IsDirty = true;
                return true;
            }

            return false;
        }

        private long GetMiningObjectId()
        {
            return ++nextId;
        }

        public IImage[] GetImageArray(long[] objectIds)
        {
            IImage[] result = new Raccoon.MRI.ImageITK[objectIds.Length];
            int i = 0;
            foreach (long id in objectIds)
            {
                IMiningObjectViewImage miningObject = this.GetMiningObject(id) as IMiningObjectViewImage;
                if (miningObject == null)
                    throw new InvalidOperationException("Unexpected error");

                ImageITK itkImage = miningObject.Image as ImageITK;
                if (itkImage == null)
                    throw new InvalidOperationException("Unexpected error");

                result[i++] = itkImage;
            }
            return result;
        }

        public itk.simple.Image[] GetITKImageArray(long[] objectIds)
        {
            itk.simple.Image[] result = new itk.simple.Image[objectIds.Length];
            int i = 0;
            foreach (long id in objectIds)
            {
                IMiningObjectViewImage miningObject = this.GetMiningObject(id) as IMiningObjectViewImage;
                if (miningObject == null)
                {
                    throw new InvalidOperationException("Unexpected error");
                }

                ImageITK itkImage = miningObject.Image as ImageITK;

                if (itkImage == null)
                {
                    throw new InvalidOperationException("Unexpected error");
                }

                result[i] = itkImage.ItkImage;
                i++;
            }

            return result;
        }

        public static MiningProject Load(string fileName)
        {
            XDocument xmlDoc = XDocument.Load(fileName);

            MiningProject project = new MiningProject();
            project.Name = xmlDoc.Descendants("MiningProject").SingleOrDefault<XElement>().Element("Name").Value;

            foreach (XElement mo in xmlDoc.Descendants("MiningObject"))
            {
                string miningObjectType = mo.Attribute("Type").Value;
                long id = Convert.ToInt64(mo.Attribute("Id").Value);
                string name = mo.Element("Name").Value;

                XElement parameters = mo.Element("Parameters");
                IMiningObject miningObject = MiningObject.Create(miningObjectType);

                miningObject.Id = id;
                miningObject.Name = name;
                miningObject.XMLParseParameters(parameters);

                project.AddMiningObject(miningObject);
            }

            project.Reload();
            project.ProjectFileName = fileName;

            return project;
        }

        public void Save(string fileName)
        {
            XDocument xmlDoc = this.GetXDocument();
            xmlDoc.Save(fileName);
            this.ProjectFileName = fileName;
            this.IsDirty = false;
        }

        public void Save()
        {
            this.Save(this.ProjectFileName);
        }

        public XDocument GetXDocument()
        {
            XNamespace xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-newInstance");
            XNamespace xsd = XNamespace.Get("http://www.w3.org/2001/XMLSchema");
            //XNamespace ns = XNamespace.Get("http://schema.Raccoon.pl/MRI");

            XDocument xmlDoc = new XDocument(
                                    new XDeclaration("1.0", "utf-8", null),
                                    new XElement(/* ns + */ "MiningProject",
                //new XAttribute("xmlns", ns.NamespaceName),
                                        new XAttribute(XNamespace.Xmlns + "xsd", xsd.NamespaceName),
                                        new XAttribute(XNamespace.Xmlns + "xsi", xsi.NamespaceName),
                                        new XElement("Name", this.Name),
                                        new XElement("MiningObjects",
                                            from o in this.miningObjects
                                            select new XElement("MiningObject",
                                                new XAttribute("Id", o.Value.Id),
                                                new XAttribute("Type", o.Value.TypeId),
                                                new XElement("Name", o.Value.Name),
                                                o.Value.XMLParametersElement
                                                )))
                                            );
            return xmlDoc;
        }

        private void Reload()
        {
            foreach (IMiningObject mo in this.GetMiningObjects())
            {
                mo.ReloadReferences(this);
            }
        }
    }
}