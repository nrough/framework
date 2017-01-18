﻿namespace Raccoon.MRI.DAL
{
    public class ImageHelper
    {
        public static ITKImageType ImageType2ITKImageType(ImageType imageType)
        {
            ITKImageType ret;

            switch (imageType)
            {
                case ImageType.ITKStandard:
                    ret = ITKImageType.ITKStandardImage;
                    break;

                case ImageType.ITKRawImage:
                    ret = ITKImageType.ITKRawImage;
                    break;

                default:
                    ret = ITKImageType.Unknown;
                    break;
            }

            return ret;
        }
    }
}