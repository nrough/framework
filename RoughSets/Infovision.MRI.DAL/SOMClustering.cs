using System;
using System.ComponentModel;

namespace Raccoon.MRI.DAL
{
    [Serializable]
    public class SOMClustering : MiningObjectViewModel
    {
        private int numberOfClusters = 1;
        private int numberOfIterations = 100;
        private double learningRate = 0.1;
        private int radius = 15;
        private int inputs = 3;

        private bool loadFile = false;
        private string fileNameLoad = String.Empty;
        private bool saveFile = true;
        private string fileNameSave = String.Empty;

        private BindingList<MiningObjectDisplay> selectedInputObjects = new BindingList<MiningObjectDisplay>();
        private BindingList<MiningObjectDisplay> availableObjects = new BindingList<MiningObjectDisplay>();

        private ImageSOMCluster cluster;

        public SOMClustering()
            : base()
        {
        }

        public int NumberOfClusters
        {
            get { return this.numberOfClusters; }
            set { SetField(ref this.numberOfClusters, value, () => NumberOfClusters); }
        }

        public int NumberOfIterations
        {
            get { return this.numberOfIterations; }
            set { SetField(ref this.numberOfIterations, value, () => NumberOfIterations); }
        }

        public double LearningRate
        {
            get { return this.learningRate; }
            set { SetField(ref this.learningRate, value, () => LearningRate); }
        }

        public int Radius
        {
            get { return this.radius; }
            set { SetField(ref this.radius, value, () => Radius); }
        }

        public int Inputs
        {
            get { return this.inputs; }
            set { SetField(ref this.inputs, value, () => Inputs); }
        }

        public bool LoadFile
        {
            get { return this.loadFile; }
            set { SetField(ref this.loadFile, value, () => LoadFile); }
        }

        public string FileNameLoad
        {
            get { return this.fileNameLoad; }
            set { SetField(ref this.fileNameLoad, value, () => FileNameLoad); }
        }

        public bool SaveFile
        {
            get { return this.saveFile; }
            set { SetField(ref this.saveFile, value, () => SaveFile); }
        }

        public string FileNameSave
        {
            get { return this.fileNameSave; }
            set { SetField(ref this.fileNameSave, value, () => FileNameSave); }
        }

        public BindingList<MiningObjectDisplay> SelectedObjects
        {
            get { return this.selectedInputObjects; }
        }

        public BindingList<MiningObjectDisplay> AvailableObjects
        {
            get { return this.availableObjects; }
            set { this.availableObjects = value; }
        }

        public ImageSOMCluster Cluster
        {
            get { return this.cluster; }
            private set { this.cluster = value; }
        }

        public override Type GetMiningObjectType()
        {
            return typeof(MiningObjectSOMCluster);
        }

        public void AddSelectedObject(MiningObjectDisplay item)
        {
            this.selectedInputObjects.Add(item);
            this.Inputs = this.selectedInputObjects.Count;
        }

        public void RemoveSelectedObject(MiningObjectDisplay item)
        {
            //this.selectedInputObjects.RemoveAll(element => element.Id == miningObject.Id);
            this.selectedInputObjects.Remove(item);
            this.Inputs = this.selectedInputObjects.Count;
        }

        public void AddAvailableObject(MiningObjectDisplay item)
        {
            this.availableObjects.Add(item);
        }

        public void RemoveAvailableObject(MiningObjectDisplay item)
        {
            //this.availableObjects.RemoveAll(element => element.Id == miningObject.Id);
            this.availableObjects.Remove(item);
        }

        public void Train(IImage[] images)
        {
            this.Cluster = new ImageSOMCluster(this.Inputs, this.NumberOfClusters);
            this.Cluster.Train(images, this.NumberOfIterations, this.LearningRate, this.Radius);
        }

        public void LoadFromFile()
        {
            this.Cluster = new ImageSOMCluster(this.Inputs, this.NumberOfClusters);
            this.Cluster.LoadNetwork(this.FileNameLoad);
        }

        public void Save(string fileName)
        {
            this.Cluster.SaveNetwork(fileName);
        }

        public IImage[] GetImageArray()
        {
            IImage[] imageArray = new ImageITK[this.SelectedObjects.Count];
            int i = 0;
            foreach (MiningObjectDisplay somInputImage in this.SelectedObjects)
            {
                IMiningObjectViewImage imageObject = somInputImage.MiningObject as IMiningObjectViewImage;
                imageArray[i++] = imageObject.Image;
            }
            return imageArray;
        }

        public itk.simple.Image[] GetITKImageArray()
        {
            itk.simple.Image[] imageArray = new itk.simple.Image[this.SelectedObjects.Count];
            int i = 0;
            foreach (MiningObjectDisplay somInputImage in this.SelectedObjects)
            {
                IMiningObjectViewImage imageObject = somInputImage.MiningObject as IMiningObjectViewImage;
                ImageITK imageITK = (ImageITK)imageObject.Image;
                imageArray[i] = imageITK.ItkImage;
                i++;
            }

            return imageArray;
        }
    }
}