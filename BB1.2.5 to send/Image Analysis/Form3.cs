using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Jai_FactoryDotNET;
//using System.Threading;
using System.Runtime.InteropServices;
using System.IO;

using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;

namespace Image_Analysis
{   


    
   unsafe  public partial class Form3 : Form
    {

        // Main factory object
        CFactory myFactory = new CFactory();

        Jai_FactoryWrapper.ImageInfo myConversionBuffer = new Jai_FactoryWrapper.ImageInfo();
        Jai_FactoryWrapper.ImageInfo myImageProcessingBuffer = new Jai_FactoryWrapper.ImageInfo();

        // Opened camera object
        CCamera myCamera;


        uint index;
        string selectedPath;
        Bitmap image;
        String timestmp_folder = DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss");

        public Form3()
        {
            InitializeComponent();

            Jai_FactoryWrapper.EFactoryError error = Jai_FactoryWrapper.EFactoryError.Success;

            // Open the factory with the default Registry database
            error = myFactory.Open("");

            // Search for cameras and update all controls
            SearchButton_Click(null, null);
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            if (null != myCamera)
            {
                if (myCamera.IsOpen)
                {
                    myCamera.Close();
                }

                myCamera = null;
            }

            // Discover GigE and/or generic GenTL devices using myFactory.UpdateCameraList(in this case specifying Filter Driver for GigE cameras).
            myFactory.UpdateCameraList(Jai_FactoryDotNET.CFactory.EDriverType.FilterDriver);

            // Open the camera - first check for GigE devices
            for (int i = 0; i < myFactory.CameraList.Count; i++)
            {
                myCamera = myFactory.CameraList[i];
                if (Jai_FactoryWrapper.EFactoryError.Success == myCamera.Open())
                {
                    break;
                }
            }

            if (null != myCamera && myCamera.IsOpen)
            {
                CameraIDTextBox.Text = myCamera.CameraID;

                if (myCamera.NumOfDataStreams > 0)
                {
                    StartButton.Enabled = true;
                    StopButton.Enabled = true;
                }
                else
                {
                    StartButton.Enabled = false;
                    StopButton.Enabled = false;
                }

                int currentValue = 0;
            }
            else
            {
                StartButton.Enabled = false;
                StopButton.Enabled = false;

                MessageBox.Show("No Cameras Found!");
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (myCamera != null)
            {
                CNode nodeAcquisitionMode = myCamera.GetNode("AcquisitionMode");
                if (null != nodeAcquisitionMode)
                {
                    nodeAcquisitionMode.Value = "Continuous";

                    myCamera.AcquisitionCount = UInt32.MaxValue;
                }


                myCamera.NewImageDelegate += new Jai_FactoryWrapper.ImageCallBack(HandleImage);
                myCamera.StartImageAcquisition(false, 5);


            }
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            Jai_FactoryWrapper.EFactoryError retsta;

            if (myCamera != null)
            {
                myCamera.StopImageAcquisition();
                myCamera.NewImageDelegate -= new Jai_FactoryWrapper.ImageCallBack(HandleImage);
            }
        }

        // Local callback function used for handle new images
        void HandleImage(ref Jai_FactoryWrapper.ImageInfo ImageInfo)
        {
            string sModelName = myCamera.ModelName;
           // string sSubDirName = selectedPath + "\\Images\\Device";
            string sSubDirName = selectedPath + "\\Images\\" + sModelName + "\\" + timestmp_folder;
            Directory.CreateDirectory(sSubDirName);
            Jai_FactoryWrapper.EFactoryError retsta;


            //Convert image to Bitmap
            //Jai_FactoryWrapper.ImageInfo localImageInfo = new Jai_FactoryWrapper.ImageInfo();
            //retsta = Jai_FactoryWrapper.J_Image_FromRawToImage(ref ImageInfo, ref localImageInfo, 4096, 4096, 4096);

            Bitmap image = GetBitmap((int)ImageInfo.SizeX, (int)ImageInfo.SizeY, 8, (byte*)ImageInfo.ImageBuffer);

  
            //to grayscale
            Grayscale gray_filter = new Grayscale(0.33, 0.33, 0.33);
            image = gray_filter.Apply(image);
            
            String timestmp = DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss");
            image.Save(sSubDirName + "\\" + sModelName + "_" + timestmp + "_O.jpg", ImageFormat.Jpeg);

            this.Invoke((MethodInvoker)delegate
            {
                pictureBox1.Image = image;
                pictureBox1.Refresh();
            });

            //Linear level correction Filter
            LevelsLinear filter_level = new LevelsLinear();
            AForge.Imaging.ImageStatistics statistics =
                new AForge.Imaging.ImageStatistics(image);
            AForge.Math.Histogram histogram = statistics.Gray;
            filter_level.InGray = histogram.GetRange(0.95);

            //opening filter
            Opening filter_open = new Opening();

            //closing filter
            Closing filter_close = new Closing();

            //Binarization filter
            Threshold filter_threshold = new Threshold(255);

            // Sharpen filter
            Sharpen filter_sharpen = new Sharpen();

            // Edge detection
           // CannyEdgeDetector filter_canny = new CannyEdgeDetector();

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
           
            this.Invoke((MethodInvoker)delegate
            {
               pictureBox2.Image = image;
               pictureBox2.Refresh();
              });

            
            image.Save(sSubDirName + "\\" + sModelName + "_" + timestmp + "_A.jpg", ImageFormat.Jpeg);

            ////no crash
            switch(objectCount)
            {
             case 0: break;
               default:

           
           //BB;
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
            int totalblobd = 10;
            int averageareablobd = 0;
            int averageblobarea = 0;

            Bitmap img = new Bitmap(381, 286);

            int[] array = new int[5000];
            int[] Count = new int[5000];
            int mostcommona = 0; int commonda = 0;

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
                }
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

            //richTextBox3.Text += "\n Number of most common crystal diameter " + mostcommona + " db";
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

            pictureBox3.Image = img;
            pictureBox3.Refresh();


            break;
            }
            //BB;
           
            return;
        }

      

        //Create bitmap image
        public Bitmap GetBitmap(int nWidth, int nHeight, int nBpp, byte* DataColor)
        {
            Bitmap BitmapImage = new Bitmap(nWidth, nHeight, PixelFormat.Format24bppRgb);

            BitmapData srcBmpData = BitmapImage.LockBits(new Rectangle(0, 0, BitmapImage.Width, BitmapImage.Height),
                ImageLockMode.ReadWrite, BitmapImage.PixelFormat);

            switch (BitmapImage.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    unsafe
                    {
                        byte* psrcBuffer = (byte*)srcBmpData.Scan0.ToPointer();

                        int nCount = srcBmpData.Width * srcBmpData.Height;
                        int nIndex = 0;

                        for (int y = 0; y < nCount; y++)
                        {
                            psrcBuffer[nIndex++] = DataColor[y];
                            psrcBuffer[nIndex++] = DataColor[y];
                            psrcBuffer[nIndex++] = DataColor[y];
                        }
                    }
                    break;
            }

            BitmapImage.UnlockBits(srcBmpData);

            return BitmapImage;
        }

        private void SaveFolderButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                selectedPath = dialog.SelectedPath;
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

       
    }
}
