using itk.simple;

namespace Infovision.MRI
{
    public class MRIHoleFillImageFilter
    {
        public MRIHoleFillImageFilter()
        {
        }

        public IImage Execute(IImage image)
        {
            uint width = image.Width;
            uint height = image.Height;
            uint depth = (image.Depth >= 1) ? image.Depth : 1;
            bool isImage3D = (image.Depth >= 1);

            VectorOfImage imageSeries = new VectorOfImage((int)depth);
            VectorUInt32 inSize = new VectorUInt32(new uint[] { width, height, 0 });
            VectorInt32 inIndex = new VectorInt32(new int[] { 0, 0, 0 });

            ImageITK imageITK = ImageITK.GetImageITK(image);

            for (int z = 0; z < depth; z++)
            {
                inIndex[2] = z;
                itk.simple.Image slice = (isImage3D == true)
                                           ? SimpleITK.Extract(imageITK.ItkImage, inSize, inIndex)
                                           : new itk.simple.Image(imageITK.ItkImage);

                GrayscaleFillholeImageFilter holeFillImageFilter = new GrayscaleFillholeImageFilter();
                itk.simple.Image sliceResult = holeFillImageFilter.Execute(slice);

                imageSeries.Add(sliceResult);
            }
            
            itk.simple.Image result = SimpleITK.JoinSeries(imageSeries);
            return new ImageITK(result);
        }
    }
}
