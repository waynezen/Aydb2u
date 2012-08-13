using System;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;

namespace SecretSanta
{
    public partial class RegisterDevice : PhoneApplicationPage
    {
        private IsolatedStorageSettings settings; 
        // Constructor
        public RegisterDevice()
        {
            InitializeComponent();
            settings = IsolatedStorageSettings.ApplicationSettings;
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!settings.Contains("DeviceId"))
            {
                BackgroundWorker bg = new BackgroundWorker();
                bg.DoWork += new DoWorkEventHandler(GetDeviceId);
                bg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bg_GetDeviceIdComplete);
                bg.RunWorkerAsync();
            }
            else
            {
                Navigate();
            }
        }

        void GetDeviceId(object sender, DoWorkEventArgs args)
        {
                object uniqueID;
                string deviceID = "";
                if (Microsoft.Phone.Info.DeviceExtendedProperties.TryGetValue("DeviceUniqueID", out uniqueID) == true)
                {
                    byte[] bID = (byte[])uniqueID;
                    deviceID = Convert.ToBase64String(bID);   // There you go
                }

                if (string.IsNullOrEmpty(deviceID))
                {
                    deviceID = Guid.NewGuid().ToString();
                }

                settings.Add("DeviceId", deviceID);
                System.Threading.Thread.Sleep(6000);

        }

        void bg_GetDeviceIdComplete(object sender, RunWorkerCompletedEventArgs args)
        {
            ProgressBar.Visibility = Visibility.Collapsed;
            BlockStatus.Text = "Device Registered";

            Navigate();
        }

        void Navigate()
        {
            // Choose where to navigate
            NavigationService.Navigate(new Uri("/Deliveries.xaml", UriKind.Relative));
            //NavigationService.Navigate(new Uri("/CheckIn.xaml", UriKind.Relative));
        }

    }
}