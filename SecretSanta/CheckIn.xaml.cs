﻿using System.Windows;
using Microsoft.Phone;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Windows.Threading;
using Microsoft.Devices;
using System.Windows.Navigation;
using ZXing;
using ZXing.Common;


namespace SecretSanta
{
    public partial class CheckIn : PhoneApplicationPage
    {
        private readonly PhotoChooserTask photoChooserTask;
        private readonly BackgroundWorker scannerWorker;
 
        private DispatcherTimer timer;
        private PhotoCameraLuminanceSource luminance;
        private Reader reader;
        private PhotoCamera photoCamera;

        private string barcodeType;
        private string barcodeContent;

        public CheckIn()
        {
            InitializeComponent();
            
            // prepare Photo Chooser Task for the open button
             photoChooserTask = new PhotoChooserTask();
             photoChooserTask.Completed += (s, e) => { if (e.TaskResult == TaskResult.OK) ProcessImage(e); };

            // prepare the backround worker thread for the image processing
             scannerWorker = new BackgroundWorker();
             scannerWorker.DoWork += scannerWorker_DoWork;
             scannerWorker.RunWorkerCompleted += scannerWorker_RunWorkerCompleted;

             // open the default barcode which should be displayed when the app starts
             var uri = new Uri("", UriKind.Relative);
             var imgSource = new BitmapImage(uri);
             BarcodeImage.Source = imgSource;
             imgSource.ImageOpened += (s, e) =>
                                         {
                                            var bmp = (BitmapImage) s;
                                            scannerWorker.RunWorkerAsync(new WriteableBitmap(bmp));
                                         };
        }

        void scannerWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // processing the result of the background scanning
            if (e.Cancelled)
            {
                Message.Text = "Cancelled.";
            }
            else if (e.Error != null)
            {
                Message.Text = e.Error.Message;
            }
            else
            {
                var result = (Result) e.Result;
                DisplayResult(result);
            }
        }

      static void scannerWorker_DoWork(object sender, DoWorkEventArgs e)
      {
         // scanning for a barcode
         var bmp = (WriteableBitmap) e.Argument;
         var imageSource = new RGBLuminanceSource(bmp, bmp.PixelWidth, bmp.PixelHeight);
         var binarizer = new HybridBinarizer(imageSource);
         var binaryBitmap = new BinaryBitmap(binarizer);
         var reader = new MultiFormatReader();
         e.Result = reader.decode(binaryBitmap);
      }

      private void ProcessImage(PhotoResult e)
      {
         // setting the image in the display and start scanning in the background
         var bmp = new BitmapImage();
         bmp.SetSource(e.ChosenPhoto);
         BarcodeImage.Source = bmp;
         scannerWorker.RunWorkerAsync(new WriteableBitmap(bmp));
      }

      private void OpenImageButton_Click(object sender, RoutedEventArgs e)
      {
         if (timer != null)
         {
            timer.Stop();
            BarcodeImage.Visibility = System.Windows.Visibility.Visible;
            previewRect.Visibility = System.Windows.Visibility.Collapsed;
         }

         photoChooserTask.Show();
      }

      private void CheckInButton_Click(object sender, RoutedEventArgs e)
      {
          if (photoCamera == null)
          {
              photoCamera = new PhotoCamera();
              photoCamera.Initialized += OnPhotoCameraInitialized;
              previewVideo.SetSource(photoCamera);

              CameraButtons.ShutterKeyHalfPressed += (o, arg) => photoCamera.Focus();
          }

          if (timer == null)
          {
              timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
              timer.Tick += (o, arg) => ScanPreviewBuffer();
          }

          BarcodeImage.Visibility = System.Windows.Visibility.Collapsed;
          previewRect.Visibility = System.Windows.Visibility.Visible;
          timer.Start();
      }

      protected override void OnNavigatedFrom(NavigationEventArgs e)
      {
         base.OnNavigatedFrom(e);

         photoCamera = null;
         if (timer != null)
         {
            timer.Stop();
            timer = null;
         }
      }

      private void OnPhotoCameraInitialized(object sender, CameraOperationCompletedEventArgs e)
      {
         var width = Convert.ToInt32(photoCamera.PreviewResolution.Width);
         var height = Convert.ToInt32(photoCamera.PreviewResolution.Height);

         Dispatcher.BeginInvoke(() =>
         {
            previewTransform.Rotation = photoCamera.Orientation;
            reader = new MultiFormatReader();
            luminance = new PhotoCameraLuminanceSource(width, height);
         });
      }

      private void ScanPreviewBuffer()
      {
         if (luminance == null)
            return;

         photoCamera.GetPreviewBufferY(luminance.PreviewBufferY);
         var binarizer = new HybridBinarizer(luminance);
         var binBitmap = new BinaryBitmap(binarizer);
         var result = reader.decode(binBitmap);
         Dispatcher.BeginInvoke(() => DisplayResult(result));
      }

      private void DisplayResult(Result result)
      {
         if (result != null)
         {
            barcodeType = result.BarcodeFormat.ToString();
            barcodeContent = result.Text;
            CheckInLogic();
         }
         else
         {
            Message.Text = "No barcode found.";
         }
      }

      private void CheckInLogic()
      {
          // do check in code
          // if successful then navigate to Delivery
          NavigationService.Navigate(new Uri("/Deliveries.xaml", UriKind.Relative));
      }

    }

}
