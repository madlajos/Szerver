using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Windows.Media;
using Jai_FactoryDotNET;
//using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math;
using AForge.Math.Geometry;

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


        //uint index;
        string selectedPath;
       // Bitmap image;
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

                //new
                bool goes = false;
            }
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            Jai_FactoryWrapper.EFactoryError retsta;
            //goes = true;

            if (myCamera != null)
            {
                myCamera.StopImageAcquisition();
                myCamera.NewImageDelegate -= new Jai_FactoryWrapper.ImageCallBack(HandleImage);
            }
        }

        // Local callback function used for handle new images
        void HandleImage(ref Jai_FactoryWrapper.ImageInfo ImageInfo)
        {      //new
           // if (goes == false)
           // {

                string sModelName = myCamera.ModelName;
                // string sSubDirName = selectedPath + "\\Images\\Device";
                string sSubDirName = selectedPath + "\\Images\\" + sModelName + "\\" + timestmp_folder;
                Directory.CreateDirectory(sSubDirName);
                Jai_FactoryWrapper.EFactoryError retsta;

                Bitmap image = GetBitmap((int)ImageInfo.SizeX, (int)ImageInfo.SizeY, 8, (byte*)ImageInfo.ImageBuffer);
                int TresholdNumber = 255;

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

                this.Invoke((MethodInvoker)delegate
                {
                    pictureBox2.Image = image;
                    pictureBox2.Refresh();
                });


                image.Save(sSubDirName + "\\" + sModelName + "_" + timestmp + "_A.jpg", ImageFormat.Jpeg);

                ////no crash
            //if (objectCount ==0)
            //{ MessageBox.Show("No image");
            //}
           // else
            //{
              //  switch (objectCount)
              //  {
                 //   case 0:
                      //  MessageBox.Show("No image");
                        //    break;
                    //default:


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
                        int totalblobd = 0;
                        int averageareablobd = 0;

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
                                richTextBox1.Text += "\n Crystal diameter " + blobd + " µm";
                            }
                        }
                        averageareablobd = totalblobd / totalblobnumber;

                        for (int m = 0; m < array.Length; m++)
                        {
                            Count[array[m]]++;
                        }

                        for (int b = 0; b < Count.Length; b++)
                        {
                            if (Count[b] != 0 & b != 0)
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

                        //pictureBox3.Image = img;
                        // pictureBox3.Refresh();


                    //mosstmst    break;

              //  }
                //BB;
        //}
              //  return;
           // }
          

        }

        //private void refreshPicturebox1()
        //{
        //    if (this.InvokeRequired)
        //    {
        //        MethodInvoker del = delegate { refreshPicturebox1(); };
        //        this.Invoke(del);
        //        return;
        //    }
        //    this.pictureBox1.Image = image;
        //    this.pictureBox1.Refresh();
        //}

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

        private void CameraIDTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

       
    }
}
