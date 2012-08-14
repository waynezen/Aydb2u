using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SecretSanta.CustomClasses;
using System.IO.IsolatedStorage;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Json;

namespace SecretSanta.Storage
{
    public static class DeliveryData
    {
        public static List<Delivery> GetDeliveriesLocal()
        {
            var results = IsoStoreHelper.LoadList<Delivery>("", "SSDeliveryList.txt");
            return results;
        }

        public static void SaveDeliveryToLocal(Delivery delivery)
        {
            var deliveries = GetDeliveriesLocal();
            deliveries.Add(delivery);
            IsoStoreHelper.SaveList<Delivery>("", "SSDeliveryList.txt", deliveries);
        }
    }
}
