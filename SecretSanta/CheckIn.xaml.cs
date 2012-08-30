using System;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Devices;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using ZXing;
using ZXing.Common;
using System.Net;
using System.IO;
using System.Xml;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SecretSanta.CustomClasses;

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

		private LocalResource _localResx = null;

        public CheckIn()
        {
            InitializeComponent();

            bool isDebug = false;
#if DEBUG
            isDebug = true;
#endif

            if (isDebug)
            {
                CheckInButton.Click += new RoutedEventHandler(OpenImageButton_Click);
            }
            else
            {
                CheckInButton.Click += new RoutedEventHandler(CheckInButton_Click);
            }

            
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

			 // get local resources
			 _localResx = LocalResource.GetInstance;
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
            CheckInWeb(barcodeContent);

            LayoutButtons.Visibility = System.Windows.Visibility.Collapsed;
            ProgressMeter.Visibility = System.Windows.Visibility.Visible;
         }
         else
         {
            Message.Text = "No barcode found.";
         }
      }

      private void CheckInWeb(string checkinBarcodeValue)
      {
          try
          {
              var settings = IsolatedStorageSettings.ApplicationSettings;
              string deviceId = (string)settings["DeviceId"];


              var request = HttpWebRequest.Create(_localResx.WebAPIUrl + "api/Sessions?deviceId=" + deviceId + "&authenticationKey=" + checkinBarcodeValue);
              request.Method = "POST";


              request.BeginGetResponse(CheckInWeb_Completed, request);
          }
          catch (Exception ex)
          {
              Message.Text = "Could not connect to Internet.";
              
              LayoutButtons.Visibility = System.Windows.Visibility.Visible;
              ProgressMeter.Visibility = System.Windows.Visibility.Collapsed;
          }
      }

      private void CheckInWeb_Completed(IAsyncResult result)
      {
          try
          {
              var request = (HttpWebRequest)result.AsyncState;
              var response = (HttpWebResponse)request.EndGetResponse(result);

              using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
              {
                  string responseString = streamReader.ReadToEnd();
                  using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(responseString)))
                  {
                      var ser = new DataContractJsonSerializer(typeof(CheckInResult));
                      var checkInResult = (CheckInResult)ser.ReadObject(ms);

                      var settings = IsolatedStorageSettings.ApplicationSettings;
                      settings.Add("SessionKey", checkInResult.Key);
                      settings.Save();
                  }
              }

              Deployment.Current.Dispatcher.BeginInvoke(() => { NavigationService.Navigate(new Uri("/Deliveries.xaml", UriKind.Relative)); });
          }
          catch (Exception ex)
          {
              Deployment.Current.Dispatcher.BeginInvoke(() => { Message.Text = "Invalid Barcode. Please try again."; });
              Deployment.Current.Dispatcher.BeginInvoke(() => { LayoutButtons.Visibility = System.Windows.Visibility.Visible; });
              Deployment.Current.Dispatcher.BeginInvoke(() => { ProgressMeter.Visibility = System.Windows.Visibility.Collapsed; });
          }
      }

    }
}
