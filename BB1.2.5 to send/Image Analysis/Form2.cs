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


using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math;
using AForge.Math.Geometry;


namespace Image_Analysis
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            this.applyFilter.BackgroundImage = Image_Analysis.Properties.Resources.playbutton;

        }

        System.Drawing.Image image_readIn;
        Bitmap image;
        Bitmap previous_image;
        Bitmap original_image;
        int objectCount;


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
        {
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

            //Binarization filter
            Threshold filter_threshold = new Threshold(255);

            // Sharpen filter
            Sharpen filter_sharpen = new Sharpen();


            // Edge detection
            //CannyEdgeDetector filter_canny = new CannyEdgeDetector();

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
            //2.számol;
            BlobCounter blobCounter = new BlobCounter();
            blobCounter.FilterBlobs = true;
            blobCounter.MinWidth = 0;
            blobCounter.MinHeight = 0;

            blobCounter.CoupledSizeFiltering = true;
            blobCounter.ProcessImage(image);
            Blob[] blobs = blobCounter.GetObjectsInformation();

            int totalblobnumber = 0;
            int blobarea = 0;
            int blobd = 0;
            int totalblobarea = 0;
            int totalblobd = 0;
            int averageareablobd = 0;
            int averageblobarea = 0;
            
            Bitmap img = new Bitmap(381, 286); 

            int[] array = new int[5000];
            int[] Count = new int[5000];
            int[] kount= { 0, 5, 6, 5,3,0,0 };  int[] kounto= new int[5000];
            int mostcommon = 0; int commond = 0; int mostcommona = 0; int commonda = 0;

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

            //averageblobarea = totalblobarea / totalblobnumber;
            //averageareablobd = totalblobd / totalblobnumber;

            for (int m = 0; m < array.Length; m++)
            {
                Count[array[m]]++;
                using (Graphics g = Graphics.FromImage(img))
                    g.DrawLine(Pens.Black,
                                         new System.Drawing.Point(array[m], 200),
                                         new System.Drawing.Point(array[m], (200 - ((Count[array[m]]) * 10)))
                                         );
             //innen kivesz;
            }

            //ezt;
            for (int b = 0; b < Count.Length; b++)
            {
                if (mostcommona < Count[b] & b > 0 & Count[b]>0)
            {
                mostcommona = Count[b];
                commonda = b;
            }
           }

            for (int m = 0; m < kount.Length; m++)
            {   
                kounto[kount[m]]++;

                //    Count[array[m]] = Count[array[m]]+1;
                if (mostcommon < kounto[kount[m]] & kount[m] > 0)
                {
                    mostcommon = kounto[kount[m]];
                    commond = kount[m];
                }
            }

           // richTextBox3.Text += "\n piupiu " + mostcommon + " db";
            //richTextBox3.Text += "\n piupu " + commond + " mikom";
            richTextBox3.Text += "\n Number of most common crystal diameter " + mostcommona + " db";
            richTextBox3.Text += "\n Most common crystal diameter " + commonda + " µm"; 


            richTextBox3.Text += "\nNumber of crystals " + totalblobnumber;
            richTextBox3.Text += "\nAverage crystal diameter " + averageareablobd + " µm";


            //creating bitmap to draw on;
            
            using (Graphics g = Graphics.FromImage(img))
            {       //tg feliratok;
                for (int k = 0; k <= 15; k++)
                {
                    int ab = k * 20;
                    g.DrawString(ab.ToString(), new Font("Tahoma", 8), System.Drawing.Brushes.Black, ab, 200);
                    g.DrawLine(Pens.Black,
                        new System.Drawing.Point(0, 200),
                        new System.Drawing.Point(300, 200)
                               );
                    //uj
                    g.DrawLine(Pens.Black,
                      new System.Drawing.Point(ab, 200),
                      new System.Drawing.Point(ab, 250)
                             );
                }
                for (int l = 1; l <= 4; l++)
                {
                    int cd = l * 5;
                    g.DrawString(cd.ToString(), new Font("Tahoma", 8), System.Drawing.Brushes.Black, 0, (200 - (10 * cd)));
                    g.DrawLine(Pens.Black,
                        new System.Drawing.Point(0, 200),
                        new System.Drawing.Point(0, 0)
                                );
                }

            }


                pictureBox2.Image = img;
                pictureBox2.Refresh();


                pictureBox1.Image = image;
                pictureBox1.Refresh();
            
        }
    

        void filters_c(Bitmap image)
        {
            //to grayscale
            Grayscale gray_filter = new Grayscale(0.33, 0.33, 0.33);
            image = gray_filter.Apply(image);

            //Linear level correction Filter
            LevelsLinear filter_level = new LevelsLinear();
            AForge.Imaging.ImageStatistics statistics =
                new AForge.Imaging.ImageStatistics(image);
            AForge.Math.Histogram histogram = statistics.Gray;
            filter_level.InGray = histogram.GetRange(0.90);

            //opening filter
            Opening filter_open = new Opening();

            //opening filter
            Closing filter_close = new Closing();

            //Binarization filter
            Threshold filter_threshold = new Threshold(180);

            // Sharpen filter
            Sharpen filter_sharpen = new Sharpen();


            // Edge detection
            //CannyEdgeDetector filter_canny = new CannyEdgeDetector();

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
            //2.számol;
            BlobCounter blobCounter = new BlobCounter();
            blobCounter.FilterBlobs = true;
            blobCounter.MinWidth = 0;
            blobCounter.MinHeight = 0;

            blobCounter.CoupledSizeFiltering = true;
            blobCounter.ProcessImage(image);
            Blob[] blobs = blobCounter.GetObjectsInformation();

            int totalblobnumber = 0;
            int blobarea = 0;
            int blobd = 0; int blobu = 0;
            int totalblobarea = 0;
            int totalblobd = 0;
            int averageareablobd = 0;
            int averageblobarea = 0;

            Bitmap img = new Bitmap(381, 286);

            int[] array = new int[50000];
            int[] Count = new int[50000];
            int[] kount = { 0, 5, 6, 5, 3, 0, 0 }; int[] kounto = new int[5000];
            int mostcommon = 0; int commond = 0; int mostcommona = 0; int commonda = 0;

            // process blobs
            foreach (Blob blob in blobs)
            {
               // if (blob.Area >0)
               // {
                    totalblobnumber = totalblobnumber + 1;
                    blobarea = Convert.ToInt32(blob.Area);
                    blobu = Convert.ToInt32((blob.Rectangle.Width*1000));
                    blobd = Convert.ToInt32((blobu / 492 * 10));

                    totalblobarea += blobarea;
                    totalblobd += blobd;

                    array[totalblobnumber] = blobd;
                    richTextBox1.Text += "\n Fiber diameter " + blobd + " nm";
              //  }
            }

            averageblobarea = totalblobarea / totalblobnumber;
            averageareablobd = totalblobd / totalblobnumber;

            for (int m = 0; m < array.Length; m++)
            {
                Count[array[m]]++;
                using (Graphics g = Graphics.FromImage(img))
                    g.DrawLine(Pens.Black,
                                         new System.Drawing.Point(array[m], 200),
                                         new System.Drawing.Point(array[m], (200 - ((Count[array[m]]) * 10)))
                                         );
                //innen kivesz;
            }

            //ezt;
            for (int b = 0; b < Count.Length; b++)
            {
                if (mostcommona < Count[b] & b > 0 & Count[b] > 0)
                {
                    mostcommona = Count[b];
                    commonda = b;
                }
            }



            richTextBox3.Text += "\n Number of most common fiber diameter " + mostcommona + " db";
            richTextBox3.Text += "\n Most common fiber diameter " + commonda + " nm";



            richTextBox3.Text += "\nNumber of detected fibers " + totalblobnumber;
            //pontositás kell richTextBox3.Text += "\nAverage fiber diameter " + averageareablobd + " µm";

            //vége:
           // image.UnlockBits(data);
            //picProcessed.Image = clonimage;
            //picProcessed.Refresh();
            pictureBox1.Image = image;
            pictureBox1.Refresh();



        }
        public FiltersSequence filter { get; set; }
        void filters_a(Bitmap image)
        {   // create filter
            ConnectedComponentsLabeling filter_connection = new ConnectedComponentsLabeling();

            // check objects count
            int objectCount = filter_connection.ObjectCount;

            FiltersSequence filter = new FiltersSequence();
            filter.Add(filter_connection);

            // apply the filter
            image = filter.Apply(image);

            //objects counter part;
            //2.számol;
            BlobCounter blobCounter = new BlobCounter();
            blobCounter.FilterBlobs = true;
            blobCounter.MinWidth = 0;
            blobCounter.MinHeight = 0;

            blobCounter.CoupledSizeFiltering = true;
            blobCounter.ProcessImage(image);
            Blob[] blobs = blobCounter.GetObjectsInformation();

            int totalblobnumber = 0;
            int blobarea = 0;
            int blobd = 0;
            int totalblobarea = 0;
            int totalblobd = 0;
            int averageareablobd = 0;
            int averageblobarea = 0;

            Bitmap img = new Bitmap(381, 286);

            int[] array = new int[5000];
            int[] Count = new int[5000];
            int[] kount = { 0, 5, 6, 5, 3, 0, 0 }; int[] kounto = new int[5000];
            int mostcommon = 0; int commond = 0; int mostcommona = 0; int commonda = 0;

      

            richTextBox3.Text += "\nNumber of crystals " + totalblobnumber;
            richTextBox3.Text += "\nAverage crystal diameter " + averageareablobd + " µm";


            pictureBox1.Image = image;
            pictureBox1.Refresh();
        }

        private void applyFilter_Click(object sender, EventArgs e)
        {
            string sw = filterSelect.Text;

            switch (sw)
            {
                case "Crystal size detection":
                    filters_b(image);
                    break;
                case "c":
                    filters_c(image);
                    break;
                case "a":
                    filters_a(image);
                    break;
                default:
                    break;
            }

        }


    }
}