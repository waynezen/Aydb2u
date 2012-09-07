using System;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using SecretSanta.CustomClasses;
using ZXing;

namespace SecretSanta
{
    public partial class CheckIn : PhoneApplicationPage
    {
        private Imaging imager;
        private LocalResource _localResx = null;

        public CheckIn()
        {
            InitializeComponent();
            
            // get local resources
            _localResx = LocalResource.GetInstance;
            imager = new Imaging(BarcodeImage, imaging_RunWorkerCompleted);
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

        private void CheckInButton_Click(object sender, RoutedEventArgs e)
        {
            imager.Show();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
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
                Message.Text = "Could not connect to Internet. Please try again.";

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
                Deployment.Current.Dispatcher.BeginInvoke(() => { Message.Text = "Invalid check in barcode. Please try again."; });
                Deployment.Current.Dispatcher.BeginInvoke(() => { LayoutButtons.Visibility = System.Windows.Visibility.Visible; });
                Deployment.Current.Dispatcher.BeginInvoke(() => { ProgressMeter.Visibility = System.Windows.Visibility.Collapsed; });
            }
        }

        private void DisplayResult(Result result)
        {
            if (result != null)
            {
                Message.Text = string.Empty;
                CheckInWeb(result.Text);
                LayoutButtons.Visibility = System.Windows.Visibility.Collapsed;
                ProgressMeter.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                Message.Text = "No barcode found. Please try again.";
            }
        }

    }
}
