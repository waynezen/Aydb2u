using System;
using System.Windows;
using Microsoft.Phone.Controls;
using SecretSanta.Storage;
using System.Windows.Controls;
using SecretSanta.CustomClasses;
using Microsoft.Phone.Tasks;
using System.Device.Location;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace SecretSanta
{
    public partial class Deliveries : PhoneApplicationPage
    {
        public Deliveries()
        {
            InitializeComponent();
            DeliveryList.ItemsSource = SortDeliveriesByDistance(new DeliveryData().GetDeliveriesLocal());

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

        private void ApplicationBarIconToggleButton_Click(object sender, EventArgs e)
        {
            switch (PageTitle.Text)
            {
                case "Deliveries":
                    PageTitle.Text = "Undelivered";
                    List<Delivery> undeliveredList = new DeliveryData().GetDeliveriesLocal().Where(dl => dl.Status == 0).ToList();
                    DeliveryList.ItemsSource = SortDeliveriesByDistance(undeliveredList);
                    break;
                case "Undelivered":
                    PageTitle.Text = "Delivered";
                    List<Delivery> deliveredList = new DeliveryData().GetDeliveriesLocal().Where(dl => dl.Status == 1).ToList();
                    DeliveryList.ItemsSource = SortDeliveriesByDistance(deliveredList);
                    break;
                case "Delivered":
                    PageTitle.Text = "Deliveries";
                    DeliveryList.ItemsSource = SortDeliveriesByDistance(new DeliveryData().GetDeliveriesLocal());
                    break;
            }

        }

        private ObservableCollection<Delivery> SortDeliveriesByDistance(List<Delivery> deliveries)
        {
            var sortedDeliveries = new ObservableCollection<Delivery>();
            var orderedDeliveries = deliveries.OrderBy(d => d.Distance);
            foreach (var delivery in orderedDeliveries)
            {
                sortedDeliveries.Add(delivery);
            }

            return sortedDeliveries;
        }
    }
}