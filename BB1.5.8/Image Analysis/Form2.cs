using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


using System.Windows.Media;

using System.Diagnostics;

using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math;
using AForge.Math.Geometry;


namespace Image_Analysis
{
    public partial class Form2 : Form
    {
        client.ClientUtil clientUtil = new client.ClientUtil();
        public Form2()
        {
            InitializeComponent();
           // this.applyFilter.BackgroundImage = Image_Analysis.Properties.Resources.playbutton;

        }

        System.Drawing.Image image_readIn;
        Bitmap image;
        Bitmap original_image;
        int TresholdNumber=255;
      
       
        private void openImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                image_readIn = System.Drawing.Image.FromFile(ofd.FileName);
                //image=AForge.Imaging.Image.FormatImage(ref original_image);
                pictureBox1.Image = image_readIn;
                image = (Bitmap)image_readIn;
                original_image = image;

            }
        }


        void filters_b(Bitmap image)
        {//Crystal size analysis
            //to grayscale
            Grayscale gray_filter = new Grayscale(0.33, 0.33, 0.33);
            image = gray_filter.Apply(image);

            //Linear level correction Filter
            LevelsLinear filter_level = new LevelsLinear();
            AForge.Imaging.ImageStatistics statistics =
                new AForge.Imaging.ImageStatistics(image);
            AForge.Math.Histogram histogram = statistics.Gray;
            filter_level.InGray = histogram.GetRange(0.99);

            //opening filter
            Opening filter_open = new Opening();

            //opening filter
            Closing filter_close = new Closing();

            //binarization filter
            Threshold filter_threshold = new Threshold(TresholdNumber);

            // Sharpen filter
            Sharpen filter_sharpen = new Sharpen();

            // create filter
            ConnectedComponentsLabeling filter_connection = new ConnectedComponentsLabeling();

            // check objects count
            int objectCount = filter_connection.ObjectCount;


            FiltersSequence filter = new FiltersSequence();
            filter.Add(filter_level);
            filter.Add(filter_open);
            filter.Add(filter_close);
            filter.Add(filter_threshold);
            filter.Add(filter_sharpen);
            filter.Add(filter_connection);

            // apply the filter
            image = filter.Apply(image);

            //objects counter part;
            //innen
            int totalblobnumber = 0;
            int blobarea = 0;
            int blobd = 0;
            int totalblobarea = 0;
            int totalblobd = 0;
            int averageareablobd = 0;

            Bitmap img = new Bitmap(381, 286);

            int[] Count = new int[5000];
            int[] array = new int[5000];
            int mostcommona = 0; int commonda = 0;
            //eddig

            BlobCounter blobCounter = new BlobCounter();
            blobCounter.FilterBlobs = true;
            blobCounter.MinWidth = 0;
            blobCounter.MinHeight = 0;

            blobCounter.CoupledSizeFiltering = true;
            blobCounter.ProcessImage(image);
            Blob[] blobs = blobCounter.GetObjectsInformation();


            // process blobs
            foreach (Blob blob in blobs)
            {
                if (blob.Area > 0)
                {
                    totalblobnumber = totalblobnumber + 1;
                    blobarea = Convert.ToInt32(blob.Area);
                    blobd = Convert.ToInt32(2 * (Math.Sqrt(blobarea / (Math.PI))) * 4.477794148);

                    totalblobarea += blobarea;
                    totalblobd += blobd;

                    array[totalblobnumber] = blobd;
                    richTextBox1.Text += "\n Crystal diameter " + blobd + " µm";
                }
            }
            averageareablobd = totalblobd / totalblobnumber;

            //int[] Count = new int[totalblobnumber];
            for (int m = 0; m < array.Length; m++)
            {
                Count[array[m]]++;
            }

            for (int b = 0; b < Count.Length; b++)
            {
                if (Count[b] != 0 & b!=0)
                {
                    //creating chart
                    this.chart1.Series["Number"].Points.AddXY(b, Count[b]);

                    if (mostcommona < Count[b] & b > 0 & Count[b] > 0)
                    {
                        mostcommona = Count[b];
                        commonda = b;
                    }
                }
            }

            richTextBox3.Text += "\n Number of most common crystal diameter " + mostcommona + " db";
            richTextBox3.Text += "\n Most common crystal diameter " + commonda + " µm";

            richTextBox3.Text += "\nNumber of crystals " + totalblobnumber;
            richTextBox3.Text += "\nAverage crystal diameter " + averageareablobd + " µm";
            clientUtil.Send(averageareablobd.ToString());
            pictureBox1.Image = image;
            pictureBox1.Refresh();


        }
     

        private void applyFilter_Click(object sender, EventArgs e)
        {
            //Stopwatch stopw = new Stopwatch();

          //  stopw.Start();
            string sw = filterSelect.Text;

            switch (sw)
            {
                case "Crystal size detection":
                    filters_b(image);
                    break;
              //  case "c":
               //     filters_c(image);
               //     break;
                //case "a":
                //    filters_a(image);
                //    break;
                default:
                    break;
            }
          //  stopw.Stop();
          //  MessageBox.Show(stopw.Elapsed.ToString());
           // Console.WriteLine("Elapsed={0}", stopw.Elapsed);

        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            TresholdNumber = int.Parse(textBox1.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = original_image;
            image = original_image;
            pictureBox1.Refresh();
        }


    }
}