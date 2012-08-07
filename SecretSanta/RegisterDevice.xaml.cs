using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
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
using System.Device.Location;

namespace SecretSanta
{
    public partial class RegisterDevice : PhoneApplicationPage
    {
        // Constructor
        public RegisterDevice()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;
            if (!settings.Contains("DeviceId"))
            {
                progressBar.Visibility = Visibility.Collapsed;
                textBlockStatus.Text = "Device Registered";
                //TODO: Wait a second, then open next page
            }
            else
            {
                
                progressBar.Visibility = Visibility.Collapsed;
                textBlockStatus.Text = "Device Registered";
            }

        }
    }
}