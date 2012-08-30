using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using System.Windows.Threading;
using System.ComponentModel;
using ZXing;
using Microsoft.Devices;
using System.Windows.Media.Imaging;
using ZXing.Common;
using System.Windows.Navigation;
using System.Xml.Serialization;
using System.IO.IsolatedStorage;
using System.IO;
using SecretSanta.CustomClasses;
using SecretSanta.Storage;
using System.Text;
using System.Runtime.Serialization.Json;

namespace SecretSanta
{
    public partial class AddDelivery : PhoneApplicationPage
    {
        private readonly PhotoChooserTask photoChooserTask;
        private readonly BackgroundWorker scannerWorker;
 
        private DispatcherTimer timer;
        private PhotoCameraLuminanceSource luminance;
        private Reader reader;
        private PhotoCamera photoCamera;
		private LocalResource _localResx = null;

        public AddDelivery()
        {
            InitializeComponent();

            bool isDebug = false;
#if DEBUG
            isDebug = true;
#endif

            if (isDebug)
            {
                AddDeliveryButton.Click += new RoutedEventHandler(OpenImageButton_Click);
            }
            else
            {
                AddDeliveryButton.Click += new RoutedEventHandler(AddDeliveryButton_Click);
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

        private void scannerWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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

        private static void scannerWorker_DoWork(object sender, DoWorkEventArgs e)
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

        private void AddDeliveryButton_Click(object sender, RoutedEventArgs e)
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
                GetDeliveryWeb(result.Text);
            }
            else
            {
                Message.Text = "No barcode found.";
            }
        }

        private void AddNewDelivery( string deliveryId )
        {

           GetDeliveryWeb(deliveryId);
           
        }

        private void GetDeliveryWeb(string deliveryId)
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;
            string sessionKey = (string)settings["SessionKey"];

            var request = HttpWebRequest.Create(_localResx.WebAPIUrl + "api/Deliveries/" + deliveryId + "?key=" + sessionKey);
            request.Method = "GET";

            request.BeginGetResponse(GetDeliveryWeb_Completed, request);
        }

        private void GetDeliveryWeb_Completed(IAsyncResult result)
        {
            var request = (HttpWebRequest)result.AsyncState;
            var response = (HttpWebResponse)request.EndGetResponse(result);

            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
            {
                string responseString = streamReader.ReadToEnd();
                using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(responseString)))
                {
                    var ser = new DataContractJsonSerializer(typeof(Delivery));
                    var deliveryResult = (Delivery)ser.ReadObject(ms);

                    DeliveryData.SaveDeliveryToLocal(deliveryResult);
                }
            }

            Deployment.Current.Dispatcher.BeginInvoke(() => { NavigationService.Navigate(new Uri("/Deliveries.xaml", UriKind.Relative)); });
        }

    }
}