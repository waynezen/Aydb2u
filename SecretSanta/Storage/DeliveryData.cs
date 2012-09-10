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
using System.Linq;
using System.ComponentModel;

namespace SecretSanta.Storage
{
    public class DeliveryData
    {
        private BackgroundWorker deliveryWebWorker;

        public List<Delivery> GetDeliveriesLocal()
        {
            var results = IsoStoreHelper.LoadList<Delivery>("", "SSDeliveryList.txt");
            return results;
        }

        public void SaveDeliveryToLocal(Delivery delivery)
        {
            var deliveries = GetDeliveriesLocal();
            deliveries.Add(delivery);
            IsoStoreHelper.SaveList<Delivery>("", "SSDeliveryList.txt", deliveries);
        }

        public void UpdateDeliveryToLocalAndWeb(Delivery delivery)
        {
            var deliveries = GetDeliveriesLocal();
            var oldDelivery = deliveries.SingleOrDefault(d => d.Id == delivery.Id);
            deliveries.Remove(oldDelivery);
            deliveries.Add(delivery);

            IsoStoreHelper.SaveList<Delivery>("", "SSDeliveryList.txt", deliveries);

            deliveryWebWorker = new BackgroundWorker();
            deliveryWebWorker.DoWork += deliveryWebWorker_DoWork;

            var changedDeliveries = (from d in GetDeliveriesLocal() where d.Changed == true select d).ToList();

            deliveryWebWorker.RunWorkerAsync(changedDeliveries);
        }

        private static void deliveryWebWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var deliveries = (List<Delivery>)e.Argument;
                var _localResx = LocalResource.GetInstance;
                var settings = IsolatedStorageSettings.ApplicationSettings;
                string sessionKey = (string)settings["SessionKey"];

                foreach (var delivery in deliveries)
                {
                    var request = System.Net.HttpWebRequest.Create(_localResx.WebAPIUrl + "api/Deliveries/" + delivery.Id + "?key=" + sessionKey + "&status=" + delivery.Status);
                    request.Method = "POST";

                    request.BeginGetResponse(deliveryWebWorker_Completed, request);
                }
            }
            catch (Exception ex)
            {
				// TODO: add logging with Log4Net
                // Do nothing -- will attempt an update to web next time a local update is done.
            }           
        }

        private static void deliveryWebWorker_Completed(IAsyncResult result)
        {
			var request = (HttpWebRequest)result.AsyncState;
			var response = (HttpWebResponse)request.EndGetResponse(result);
            // Do Nothing
        }
    }
}
