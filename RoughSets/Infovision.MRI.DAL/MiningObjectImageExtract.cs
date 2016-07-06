using System;

namespace Infovision.MRI.DAL
{
    public class MiningObjectImageExtract : MiningObjectImage, IMiningObjectViewImage
    {
        public MiningObjectImageExtract()
            : base()
        {
        }

        public override void ReloadReferences(MiningProject project)
        {
            if (this.RefId > 0)
            {
                IMiningObjectViewImage selectedMiningObject = project.GetMiningObject(this.RefId) as IMiningObjectViewImage;

                if (selectedMiningObject != null)
                {
                    ImageExtract imageExtgract = new ImageExtract(selectedMiningObject.Image, this.SliceFrom);
                    this.Image = imageExtgract.GetExtractedImage();
                }
            }
        }

        public override void InitFromViewModel(MiningObjectViewModel viewModel)
        {
            base.InitFromViewModel(viewModel);

            ImageExtract extractModel = viewModel as ImageExtract;

            if (extractModel != null)
            {
                this.TypeId = MiningObjectType.Types.ImageExtract;

                this.SliceFrom = extractModel.Slice;
                this.SliceTo = extractModel.Slice;
                this.Name = String.Format("Image slice {0}", extractModel.Slice);

                IImage image = extractModel.Image as IImage;

                this.Width = (int)image.Width;
                this.Height = (int)image.Height;
                this.Depth = 0;

                this.Header = 0;
                this.ImageType = DAL.ImageType.ITKStandard;
                this.PixelType = SimpleITKHelper.Type2PixelType(image.PixelType);
                this.Endianness = MRI.Endianness.LittleEndian;
                this.FileName = String.Empty;
            }
        }
    }
}