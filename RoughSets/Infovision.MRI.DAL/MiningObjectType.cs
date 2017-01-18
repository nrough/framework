namespace Raccoon.MRI.DAL
{
    public class MiningObjectType
    {
        public class Types
        {
            public const string ImageRAW = "IMAGE_RAW";
            public const string ImageITK = "IMAGE_ITK";
            public const string ImageMask = "IMAGE_MASK";
            public const string ImageEdge = "IMAGE_EDGE";
            public const string ImageHistogram = "IMAGE_HISTOGRAM";
            public const string ImageHistogramCluster = "IMAGE_HISTOGRAM_CLUSTER";
            public const string ImageSOMCluster = "IMAGE_SOM_CLUSTER";
            public const string ImageExtract = "IMAGE_EXTRACT";
            public const string ImageNeighbour = "IMAGE_NEIGHBOUR";
            public const string Dummy = "DUMMY";
        }
    }
}