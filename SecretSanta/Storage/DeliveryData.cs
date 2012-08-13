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

namespace SecretSanta.Storage
{
    public static class DeliveryData
    {
        public static List<Delivery> GetDeliveriesLocal()
        {
            var results = IsoStoreHelper.LoadList<Delivery>("", "SSDeliveryList.txt");
            
            // Test Code Only
            if (results.Count == 0)
            {
                results = CreateDataForTesting();
            }

            return results;
        }

        public static void SaveDeliveryToLocal(Delivery delivery)
        {
            var deliveries = GetDeliveriesLocal();
            deliveries.Add(delivery);
            IsoStoreHelper.SaveList<Delivery>("", "SSDeliveryList.txt", deliveries);
        }

        public static Delivery GetDelivery(string deliveryID)
        {
            return new Delivery("20", "", "10145 104 Street Northwest, Edmonton, AB T5J 1A7");
        }

        private static List<Delivery> CreateDataForTesting()
        {
            var results = new List<Delivery>();

            results.Add(new Delivery("1","", "207 8315 105 Street Edmonton Alberta T6E-4H4"));
            results.Add(new Delivery("2", "Delivered", "6565 Gateway Blvd Edmonton Alberta T6H-2J1"));
            results.Add(new Delivery("3", "Undelivered", "10702-10922 51 St NW Edmonton, AB T6H-0L2"));
            results.Add(new Delivery("4", "Delivered", "6562 Gateway Blvd Edmonton Alberta T6H-2J1"));
            results.Add(new Delivery("5", "Undelivered", "10702-10911 51 St NW Edmonton, AB T6H-0L2"));
            results.Add(new Delivery("6", "", "1407 8315 105 Street Edmonton Alberta T6E-4H4"));
            results.Add(new Delivery("7", "", "Additional Address"));

            return results;
        }
    }
}
