using System;
using System.Windows;
using Microsoft.Phone.Controls;
using SecretSanta.Storage;
using System.Windows.Controls;
using SecretSanta.CustomClasses;
using Microsoft.Phone.Tasks;
using System.Device.Location;

namespace SecretSanta
{
    public partial class Deliveries : PhoneApplicationPage
    {
        public Deliveries()
        {
            InitializeComponent();
            DeliveryList.ItemsSource = DeliveryData.GetDeliveriesLocal();
        }

        private void DeliveryAddressButton_Click(object sender, RoutedEventArgs e)
        {
            var source = (Button)e.OriginalSource;
            var selectedDelivery = (Delivery)source.DataContext;          

            var mapDirectionsTask = new BingMapsDirectionsTask();

            LabeledMapLocation originatingLocation;
            var currentLocation = new GeoCoordinateWatcher(GeoPositionAccuracy.Default).Position;
            if (currentLocation == null || currentLocation.Location.IsUnknown)
            {
                currentLocation.Location = new GeoCoordinate(53.579022, -113.522769);
            }

            originatingLocation = new LabeledMapLocation("Current Location", currentLocation.Location); 

            var desintation = new LabeledMapLocation(selectedDelivery.Address.ToString(), null);

            mapDirectionsTask.Start = originatingLocation;
            mapDirectionsTask.End = desintation;

            mapDirectionsTask.Show();
        }

        private void ApplicationBarIconAddDeliveryButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AddDelivery.xaml", UriKind.Relative));
        }

        private void ApplicationBarIconDeliverButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Deliver.xaml", UriKind.Relative));
        }
        
    }

}