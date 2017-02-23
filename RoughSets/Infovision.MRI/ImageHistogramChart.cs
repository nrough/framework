using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;

namespace NRough.MRI
{
    public class ImageHistogram
    {
        public ImageHistogram()
        {
            this.HistogramBucketSize = 4;
            this.ChartHeight = ImageHistogram.DefaultChartHeight;
            this.ChartWidth = ImageHistogram.DefaultChartWidth;
        }

        public ImageHistogram(IImage image)
            : this()
        {
            this.Image = image;
        }

        public int HistogramBucketSize { get; set; }
        public int ChartWidth { get; set; }
        public int ChartHeight { get; set; }
        public IImage Image { get; set; }

        public Chart Chart
        {
            get;
            private set;
        }

        public static int DefaultChartWidth
        {
            get { return 600; }
        }

        public static int DefaultChartHeight
        {
            get { return 350; }
        }

        public static MathNet.Numerics.Statistics.Histogram GetHistogram(IImage image, int numberOfBuckets)
        {
            MathNet.Numerics.Statistics.Histogram histogram = new MathNet.Numerics.Statistics.Histogram(image.GetData<double>(), numberOfBuckets);
            return histogram;
        }

        public static MathNet.Numerics.Statistics.Histogram GetHistogram(itk.simple.Image image, int numberOfBuckets)
        {
            ImageITK imageITK = new ImageITK(image);
            return ImageHistogram.GetHistogram(imageITK, numberOfBuckets);
        }

        public static int FindGlobalMaxima(MathNet.Numerics.Statistics.Histogram histogram)
        {
            double maxValue = -1;
            int maxIndex = -1;
            for (int i = 0; i < histogram.BucketCount; i++)
            {
                if (histogram[i].Count > maxValue)
                {
                    maxValue = histogram[i].Count;
                    maxIndex = i;
                }
            }

            return maxIndex;
        }

        public static int FindLocalMinima(MathNet.Numerics.Statistics.Histogram histogram, int startAtIdx)
        {
            int j = startAtIdx;
            while (j < histogram.BucketCount)
            {
                if (j + 1 < histogram.BucketCount
                        && histogram[j + 1].Count > histogram[j].Count)
                {
                    break;
                }

                j++;
            }

            if (j == histogram.BucketCount)
            {
                j--;
            }

            return j;
        }

        public static Chart GetChart(itk.simple.Image image, int bucketCount)
        {
            ImageITK imageITK = new ImageITK(image);
            return ImageHistogram.GetChart(imageITK, bucketCount);
        }

        public static Chart GetChart(IImage image, int bucketCount)
        {
            int numberOfBuckets = ImageHelper.GetNumberOfHistogramBuckets(image, bucketCount);
            MathNet.Numerics.Statistics.Histogram histogram = ImageHistogram.GetHistogram(image, numberOfBuckets);

            DataSet dataSet = new DataSet();
            DataTable dt = new DataTable();

            dt.Columns.Add("Range", typeof(double));
            dt.Columns.Add("Counter", typeof(double));

            for (int i = 0; i < histogram.BucketCount; i++)
            {
                DataRow dataRow = dt.NewRow();
                dataRow[0] = histogram[i].UpperBound;
                dataRow[1] = histogram[i].Count;
                dt.Rows.Add(dataRow);
            }

            dataSet.Tables.Add(dt);

            Chart chart = new Chart();
            chart.DataSource = dataSet.Tables[0];
            chart.Width = ImageHistogram.DefaultChartWidth;
            chart.Height = ImageHistogram.DefaultChartHeight;

            Series serie1 = new Series();
            serie1.Name = "Range";
            serie1.Color = Color.FromArgb(112, 255, 200);
            serie1.BorderColor = Color.FromArgb(164, 164, 164);
            serie1.ChartType = SeriesChartType.Column;
            serie1.BorderDashStyle = ChartDashStyle.Solid;
            serie1.BorderWidth = 1;
            serie1.ShadowColor = Color.FromArgb(128, 128, 128);
            serie1.ShadowOffset = 1;
            serie1.IsValueShownAsLabel = false;
            serie1.XValueMember = "Range";
            serie1.YValueMembers = "Counter";
            serie1.Font = new Font("Tahoma", 8.0f);
            serie1.BackSecondaryColor = Color.FromArgb(0, 102, 153);
            serie1.LabelForeColor = Color.FromArgb(100, 100, 100);
            chart.Series.Add(serie1);

            ChartArea ca = new ChartArea();
            ca.Name = "ChartArea1";
            ca.BackColor = Color.White;
            ca.BorderColor = Color.FromArgb(26, 59, 105);
            ca.BorderWidth = 0;
            ca.BorderDashStyle = ChartDashStyle.Solid;
            ca.AxisX = new Axis();
            ca.AxisY = new Axis();
            chart.ChartAreas.Add(ca);

            chart.DataBind();

            return chart;
        }

        private void CreateChart()
        {
            int numberOfBuckets = ImageHelper.GetNumberOfHistogramBuckets(this.Image, this.HistogramBucketSize);
            MathNet.Numerics.Statistics.Histogram histogram = ImageHistogram.GetHistogram(this.Image, numberOfBuckets);

            DataSet dataSet = new DataSet();
            DataTable dt = new DataTable();

            dt.Columns.Add("Range", typeof(double));
            dt.Columns.Add("Counter", typeof(double));

            for (int i = 0; i < histogram.BucketCount; i++)
            {
                DataRow dataRow = dt.NewRow();
                dataRow[0] = histogram[i].UpperBound;
                dataRow[1] = histogram[i].Count;
                dt.Rows.Add(dataRow);
            }

            dataSet.Tables.Add(dt);

            Chart chart = new Chart();
            chart.DataSource = dataSet.Tables[0];
            chart.Width = this.ChartWidth;
            chart.Height = this.ChartHeight;

            Series serie1 = new Series();
            serie1.Name = "Range";
            serie1.Color = Color.FromArgb(112, 255, 200);
            serie1.BorderColor = Color.FromArgb(164, 164, 164);
            serie1.ChartType = SeriesChartType.Column;
            serie1.BorderDashStyle = ChartDashStyle.Solid;
            serie1.BorderWidth = 1;
            serie1.ShadowColor = Color.FromArgb(128, 128, 128);
            serie1.ShadowOffset = 1;
            serie1.IsValueShownAsLabel = false;
            serie1.XValueMember = "Range";
            serie1.YValueMembers = "Counter";
            serie1.Font = new Font("Tahoma", 8.0f);
            serie1.BackSecondaryColor = Color.FromArgb(0, 102, 153);
            serie1.LabelForeColor = Color.FromArgb(100, 100, 100);
            chart.Series.Add(serie1);

            ChartArea ca = new ChartArea();
            ca.Name = "ChartArea1";
            ca.BackColor = Color.White;
            ca.BorderColor = Color.FromArgb(26, 59, 105);
            ca.BorderWidth = 0;
            ca.BorderDashStyle = ChartDashStyle.Solid;
            ca.AxisX = new Axis();
            ca.AxisY = new Axis();
            chart.ChartAreas.Add(ca);

            chart.DataBind();

            this.Chart = chart;
        }

        public Chart GetChart()
        {
            if (this.Chart == null)
                this.CreateChart();

            return this.Chart;
        }

        public static Bitmap GetChartBitmap(IImage image, int bucketCount)
        {
            Chart chart = ImageHistogram.GetChart(image, bucketCount);

            Bitmap bm;
            using (MemoryStream ms = new MemoryStream())
            {
                chart.SaveImage(ms, ChartImageFormat.Bmp);
                bm = new Bitmap(ms);
            }

            return bm;
        }

        public Bitmap GetChartBitmap()
        {
            Chart chart = this.GetChart();

            Bitmap bm;
            using (MemoryStream ms = new MemoryStream())
            {
                chart.SaveImage(ms, ChartImageFormat.Bmp);
                bm = new Bitmap(ms);
            }

            return bm;
        }
    }
}