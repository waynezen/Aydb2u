using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Devices;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using SecretSanta.CustomClasses;
using SecretSanta.Storage;
using ZXing;
using ZXing.Common;

namespace SecretSanta
{
    public partial class AddDelivery : PhoneApplicationPage
    {
        private Imaging imager;
        private LocalResource _localResx = null;

        public AddDelivery()
        {
            InitializeComponent();

            _localResx = LocalResource.GetInstance;

            AddDeliveryButton.Click += new RoutedEventHandler(AddDeliveryButton_Click);
            imager = new Imaging(BarcodeImage, imaging_RunWorkerCompleted);
        }

        private void AddDeliveryButton_Click(object sender, RoutedEventArgs e)
        {
            imager.Show();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private void DisplayResult(Result result)
        {
            if (result != null)
            {
                GetDeliveryWeb(result.Text);

                LayoutButtons.Visibility = System.Windows.Visibility.Collapsed;
                ProgressMeter.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                Message.Text = "No barcode found. Please try again.";
            }
        }

        private void imaging_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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
                var result = (Result)e.Result;
                DisplayResult(result);
            }
        }

        private void GetDeliveryWeb(string deliveryId)
        {
            try
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                string sessionKey = (string)settings["SessionKey"];

                var request = System.Net.HttpWebRequest.Create(_localResx.WebAPIUrl + "api/Deliveries/" + deliveryId + "?key=" + sessionKey);
                request.Method = "GET";

                request.BeginGetResponse(GetDeliveryWeb_Completed, request);
            }
            catch (Exception ex)
            {
                Message.Text = "Could not connect to Internet. Please try again.";

                LayoutButtons.Visibility = System.Windows.Visibility.Visible;
                ProgressMeter.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void GetDeliveryWeb_Completed(IAsyncResult result)
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
                        var ser = new DataContractJsonSerializer(typeof(Delivery));
                        var deliveryResult = (Delivery)ser.ReadObject(ms);

                        new DeliveryData().SaveDeliveryToLocal(deliveryResult);
                    }
                }

                Deployment.Current.Dispatcher.BeginInvoke(() => { NavigationService.Navigate(new Uri("/Deliveries.xaml", UriKind.Relative)); });
            }
            catch (Exception ex)
            {
				// TODO: add logging with Log4Net
				Debug.WriteLine(String.Format("AddDelivery error: {0}", ex.Message));

                Deployment.Current.Dispatcher.BeginInvoke(() => { Message.Text = "Invalid delivery barcode. Please try again."; });
                Deployment.Current.Dispatcher.BeginInvoke(() => { LayoutButtons.Visibility = System.Windows.Visibility.Visible; });
                Deployment.Current.Dispatcher.BeginInvoke(() => { ProgressMeter.Visibility = System.Windows.Visibility.Collapsed; });
            }
        }

    }
}