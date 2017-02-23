using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NRough.MRI.DAL;

namespace NRough.MRI.UI
{
    public sealed class MRIApplication
    {
        private MiningProject activeProject = new MiningProject();
        private static MRIApplication appl;
        private Dictionary<string, EventHandler<MiningObjectEventArgs>> viewers = new Dictionary<string, EventHandler<MiningObjectEventArgs>>();

        private MRIApplication()
        {
            InitViewers();
        }

        public static MRIApplication Default
        {
            get
            {
                if (appl == null)
                {
                    appl = new MRIApplication();
                }

                return appl;
            }
        }

        public MiningProject ActiveProject
        {
            get { return this.activeProject; }
            private set { this.activeProject = value; }
        }

        private void InitViewers()
        {
            viewers.Add(MiningObjectType.Types.ImageRAW, miningObjectViewImage_Viewing);
            viewers.Add(MiningObjectType.Types.ImageITK, miningObjectViewImage_Viewing);
            viewers.Add(MiningObjectType.Types.ImageExtract, miningObjectViewImage_Viewing);
            viewers.Add(MiningObjectType.Types.ImageMask, miningObjectViewImage_Viewing);
            viewers.Add(MiningObjectType.Types.ImageEdge, miningObjectViewImage_Viewing);
            viewers.Add(MiningObjectType.Types.ImageHistogram, miningObjectViewImage_Viewing);
        }

        public void AddDefaultViewer(IMiningObject miningObject)
        {
            EventHandler<MiningObjectEventArgs> viewer;
            if (viewers.TryGetValue(miningObject.TypeId, out viewer))
            {
                miningObject.Viewing += viewer;
            }
        }

        public void AddDefaultViewer()
        {
            foreach (IMiningObject mo in this.GetMiningObjects())
            {
                this.AddDefaultViewer(mo);
            }
        }

        private void miningObjectViewImage_Viewing(object sender, MiningObjectEventArgs e)
        {
            IMiningObjectViewImage imageView = e.MiningObject as IMiningObjectViewImage;

            if (imageView == null)
            {
                MessageBox.Show("Nothing to view");
                return;
            }

            this.ShowImage(new DAL.ImageRead(imageView.Image));
        }

        private void ShowImage(NRough.MRI.DAL.ImageRead image)
        {
            ImageForm childImageForm = new ImageForm();
            childImageForm.Image = image;
            childImageForm.Text = image.Name;
            childImageForm.Show();
        }

        public IMiningObject GetMiningObject(long id)
        {
            return this.ActiveProject.GetMiningObject(id);
        }

        public IEnumerable<IMiningObject> GetMiningObjects()
        {
            return this.ActiveProject.GetMiningObjects();
        }

        private void AddMiningObject(IMiningObject miningObject)
        {
            appl.ActiveProject.AddMiningObject(miningObject);
            this.AddDefaultViewer(miningObject);
        }

        public bool RemoveMiningObject(long id)
        {
            return this.ActiveProject.RemoveMiningObject(this.GetMiningObject(id));
        }

        public IMiningObject GetDummyMiningObject(string name)
        {
            IMiningObject miningObject = new MiningObject
            {
                TypeId = MiningObjectType.Types.Dummy,
                Id = 0,
                Name = name,
                RefId = 0
            };

            return miningObject;
        }

        public IMiningObject OpenRAWImage()
        {
            RawImageImportDialog importRAWForm = new RawImageImportDialog();

            NRough.MRI.DAL.ImageRead image = new NRough.MRI.DAL.ImageRead();
            image.ImageTypeId = ImageType.ITKRawImage;
            importRAWForm.ImageBindingSource.Add(image);

            if (importRAWForm.ShowDialog() == DialogResult.OK)
            {
                IMiningObject miningObject = MiningObject.Create(image);
                this.AddMiningObject(miningObject);

                if (image.ViewImage)
                {
                    miningObject.View();
                }

                return miningObject;
            }

            return null;
        }

        public IMiningObject OpenImage()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.RestoreDirectory = false;

            fileDialog.Filter = "Analyze 7.5 (*.img)|*.img";
            fileDialog.Filter += "|" + "PNG Files (*.png)|*.png";
            fileDialog.Filter += "|" + "BMP Files (*.bmp)|*.bmp";
            fileDialog.Filter += "|" + "All files (*.*)|*.*";

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                NRough.MRI.DAL.ImageRead image = new NRough.MRI.DAL.ImageRead
                {
                    ImageTypeId = ImageType.ITKStandard,
                    FileName = fileDialog.FileName,
                    Name = fileDialog.FileName
                };

                IMiningObject miningObject = MiningObject.Create(image);
                this.AddMiningObject(miningObject);
                return miningObject;
            }

            return null;
        }

        public IMiningObject CreateImageHistogram(long id)
        {
            HistogramDialog histogramDialog = new HistogramDialog();
            IMiningObjectViewImage selectedMiningObject = this.GetMiningObject(id) as IMiningObjectViewImage;

            NRough.MRI.DAL.Histogram histParm = new NRough.MRI.DAL.Histogram
            {
                SliceFrom = 0,
                SliceTo = 0,
                Image = selectedMiningObject.Image
            };

            histogramDialog.HistogramParametersBindingSource.Add(histParm);

            if (histogramDialog.ShowDialog() == DialogResult.OK)
            {
                IMiningObject miningObject = MiningObject.Create(histParm);
                miningObject.RefId = id;
                this.AddMiningObject(miningObject);
                return miningObject;
            }

            return null;
        }

        public IMiningObject CreateImageMask(long id)
        {
            ImageMaskDialog imageMaskDialog = new ImageMaskDialog();
            ImageMaskItems imageMaskItems = new ImageMaskItems();

            //TODO Construct logic for storing default values in a single place
            imageMaskItems.AddMaskItem(150, 10);
            imageMaskItems.AddMaskItem(100, 20);
            imageMaskItems.AddMaskItem(51, 50);

            imageMaskDialog.ImageMaskItemsBindingSource.DataSource = imageMaskItems;

            if (imageMaskDialog.ShowDialog() == DialogResult.OK)
            {
                IMiningObjectViewImage selectedMiningObject = this.GetMiningObject(id) as IMiningObjectViewImage;
                NRough.MRI.ImageITK itkImage = (NRough.MRI.ImageITK)selectedMiningObject.Image;
                itk.simple.Image binaryMaskImage = new MRIMaskBinaryImageFilter().Execute(itkImage.ItkImage);

                MRIMaskConcentricImageFilter imageMaskFilter = new MRIMaskConcentricImageFilter();
                foreach (var item in imageMaskItems)
                {
                    imageMaskFilter.AddMaskItem(item);
                }

                NRough.MRI.ImageITK maskImage = new NRough.MRI.ImageITK(imageMaskFilter.Execute(binaryMaskImage));

                NRough.MRI.DAL.ImageMask mask = new ImageMask
                {
                    Image = maskImage,
                    MaskItems = imageMaskItems.ToList<MRIMaskItem>()
                };

                IMiningObject miningObject = MiningObject.Create(mask);
                miningObject.RefId = id;
                this.AddMiningObject(miningObject);
                return miningObject;
            }

            return null;
        }

        public IMiningObject CreateImageEdge(long id)
        {
            ImageEdgeDialog edgeDialog = new ImageEdgeDialog();

            DAL.ImageEdge imageEdge = new DAL.ImageEdge
            {
                Background = 0,
                Foreground = 254,
                Noise = 0.20
            };

            edgeDialog.ImageEdgeBindingSource.Add(imageEdge);

            if (edgeDialog.ShowDialog() == DialogResult.OK)
            {
                IMiningObjectViewImage selectedMiningObject = this.GetMiningObject(id) as IMiningObjectViewImage;
                NRough.MRI.ImageITK itkImage = (NRough.MRI.ImageITK)selectedMiningObject.Image;

                EdgeThresholdFilter edgeFilter = new EdgeThresholdFilter(imageEdge.Noise,
                                                                        (double)imageEdge.Foreground,
                                                                        (double)imageEdge.Background);

                itk.simple.Image binaryMaskImage = new MRIMaskBinaryImageFilter().Execute(itkImage.ItkImage);
                NRough.MRI.ImageITK edgeImage = new NRough.MRI.ImageITK(edgeFilter.Execute(itkImage.ItkImage));

                imageEdge.Image = edgeImage;

                IMiningObject miningObject = MiningObject.Create(imageEdge);
                miningObject.RefId = id;
                this.AddMiningObject(miningObject);

                return miningObject;
            }

            return null;
        }

        public IMiningObject ExtractImage(long id)
        {
            IMiningObjectViewImage selectedMiningObject = this.GetMiningObject(id) as IMiningObjectViewImage;

            ImageExtractDialog ImageExtractDialog = new ImageExtractDialog();
            ImageExtract imageExtract = new ImageExtract(selectedMiningObject.Image);
            ImageExtractDialog.ImageExtractBiningSource.Add(imageExtract);

            if (ImageExtractDialog.ShowDialog() == DialogResult.OK)
            {
                IMiningObject miningObject = MiningObject.Create(imageExtract);
                miningObject.RefId = id;
                this.AddMiningObject(miningObject);

                if (imageExtract.ViewImage)
                {
                    miningObject.View();
                }

                return miningObject;
            }

            return null;
        }

        public IMiningObject CreateSOMCluster(long id)
        {
            SOMClusteringDialog somClusteringDialog = new SOMClusteringDialog();
            IMiningObject selectedMiningObject = this.GetMiningObject(id) as IMiningObject;
            IMiningObjectViewImage imageMiningObject = selectedMiningObject as IMiningObjectViewImage;

            NRough.MRI.DAL.SOMClustering somClustering = new NRough.MRI.DAL.SOMClustering
            {
                LearningRate = 0.1,
                NumberOfClusters = 9,
                NumberOfIterations = 100,
                Radius = 15
            };

            somClustering.AddSelectedObject(new MiningObjectDisplay(selectedMiningObject));

            var images = from img in this.GetMiningObjects()
                         where (img.TypeId == MiningObjectType.Types.ImageRAW
                                || img.TypeId == MiningObjectType.Types.ImageITK
                                || img.TypeId == MiningObjectType.Types.ImageExtract)
                            && img.Id != id
                         select new MiningObjectDisplay(img);

            foreach (MiningObjectDisplay item in images)
            {
                somClustering.AddAvailableObject(item);
            }

            somClusteringDialog.SOMClusteringBindingSource.Add(somClustering);

            if (somClusteringDialog.ShowDialog() == DialogResult.OK)
            {
                IMiningObject miningObject = MiningObject.Create(somClustering);
                this.AddMiningObject(miningObject);
                return miningObject;
            }

            return null;
        }

        public IMiningObject CreateHistCluster(long id)
        {
            IMiningObjectViewImage selectedMiningObject = this.GetMiningObject(id) as IMiningObjectViewImage;
            if (selectedMiningObject == null)
            {
                throw new InvalidOperationException("Label is not image");
            }

            HistogramClusteringDialog histClusteringDialog = new HistogramClusteringDialog();

            NRough.MRI.DAL.HistogramClustering histClustering = new NRough.MRI.DAL.HistogramClustering
            {
                ApproximationDegree = 0,
                BucketCountWeight = 100,
                HistogramBucketSize = 4,
                MaxNumberOfRepresentatives = 3,
                MinimumClusterDistance = 100,
                Image = selectedMiningObject.Image
            };

            histClusteringDialog.HistogramClusteringBindingSource.Add(histClustering);

            if (histClusteringDialog.ShowDialog() == DialogResult.OK)
            {
                IMiningObject miningObject = MiningObject.Create(histClustering);
                miningObject.RefId = id;
                this.AddMiningObject(miningObject);
                return miningObject;
            }

            return null;
        }

        public IMiningObject CreateNeighbour()
        {
            NeighbourDialog neighbourDialog = new NeighbourDialog();

            ImageNeighbour imageNeighbour = new ImageNeighbour();

            var masks = from obj in this.GetMiningObjects()
                        where (obj.TypeId == MiningObjectType.Types.ImageEdge)
                        select obj;

            foreach (IMiningObject mask in masks)
            {
                imageNeighbour.AddMask(mask);
            }

            var labels = from obj in this.GetMiningObjects()
                         where (obj.TypeId == MiningObjectType.Types.ImageSOMCluster
                            || obj.TypeId == MiningObjectType.Types.ImageHistogramCluster)
                         select obj;

            foreach (IMiningObject label in labels)
            {
                imageNeighbour.AddLabels(label);
            }

            neighbourDialog.FormBindingSource.Add(imageNeighbour);

            if (neighbourDialog.ShowDialog() == DialogResult.OK)
            {
                IMiningObject miningObject = MiningObject.Create(imageNeighbour);
                this.AddMiningObject(miningObject);
                return miningObject;
            }

            return null;
        }

        public void View(long id)
        {
            IMiningObject miningObject = this.GetMiningObject(id);
            miningObject.View();
        }

        public bool PromptSave()
        {
            if (MessageBox.Show("Active project has not been saved. Do you want to save it?",
                                    "Warning",
                                    MessageBoxButtons.OKCancel,
                                    MessageBoxIcon.Warning,
                                    MessageBoxDefaultButton.Button1,
                                    MessageBoxOptions.RightAlign,
                                    true) == DialogResult.OK)
            {
                return true;
            }

            return false;
        }

        public void LoadProject(string projectFileName)
        {
            if (this.ActiveProject != null
                    && this.ActiveProject.IsDirty)
            {
                if (this.PromptSave())
                {
                    this.ActiveProject.Save();
                }
            }

            try
            {
                this.ActiveProject = MiningProject.Load(projectFileName);
                this.AddDefaultViewer();
            }
            catch
            {
                MessageBox.Show(String.Format("Cannot load project from file {0}", projectFileName));
            }
        }

        public void SaveProject(string projectFileName)
        {
            try
            {
                this.ActiveProject.Save(projectFileName);
            }
            catch
            {
                MessageBox.Show(String.Format("Cannot save project to file {0}", projectFileName));
            }
        }

        public void CreateProject(string name)
        {
            this.ActiveProject = new MiningProject();
            this.ActiveProject.Name = name;
        }
    }
}