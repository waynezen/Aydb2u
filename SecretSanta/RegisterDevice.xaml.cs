using System;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Info;

namespace SecretSanta
{
    public partial class RegisterDevice : PhoneApplicationPage
    {
        private IsolatedStorageSettings settings;

        public RegisterDevice()
        {
            InitializeComponent();
            settings = IsolatedStorageSettings.ApplicationSettings;
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!settings.Contains("DeviceId"))
            {
                BlockStatus.Visibility = System.Windows.Visibility.Visible;
                ProgressBar.Visibility = System.Windows.Visibility.Visible;

                BackgroundWorker bg = new BackgroundWorker();
                bg.DoWork += new DoWorkEventHandler(GetDeviceId);
                bg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(GetDeviceId_Complete);
                bg.RunWorkerAsync();
            }
            else
            {
                Navigate();
            }
        }

        private void GetDeviceId(object sender, DoWorkEventArgs args)
        {
            object uniqueID;
            string deviceID = "";
            if (DeviceExtendedProperties.TryGetValue("DeviceUniqueID", out uniqueID) == true)
            {
                byte[] bID = (byte[])uniqueID;
                deviceID = Convert.ToBase64String(bID);
            }


            if (string.IsNullOrEmpty(deviceID))
            {
                // If no device Id is provided we create one.
                deviceID = Guid.NewGuid().ToString();
            }

            settings.Add("DeviceId", deviceID);
            settings.Save();
        }

        private void GetDeviceId_Complete(object sender, RunWorkerCompletedEventArgs args)
        {
            ProgressBar.Visibility = Visibility.Collapsed;
            BlockStatus.Text = "Device Registered";

            Navigate();
        }

        private void Navigate()
        {
            string sessionKey = "";
            var results = settings.TryGetValue<string>("SessionKey", out sessionKey);

            if (string.IsNullOrEmpty(sessionKey))
            {
                NavigationService.Navigate(new Uri("/CheckIn.xaml", UriKind.Relative));
            }
            else
            {
                NavigationService.Navigate(new Uri("/Deliveries.xaml", UriKind.Relative));
            }
        }

    }
}