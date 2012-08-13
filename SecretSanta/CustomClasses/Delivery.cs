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
using System.Runtime.Serialization;

namespace SecretSanta.CustomClasses
{
    [DataContract]
    public class Delivery
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Status { get; set; }
        public string Image
        {
            get
            {
                string result ="";
                switch (Status)
                {
                    case "":
                        result= "Images/gift-yellow.png";
                        break;
                    case "Delivered":
                        result = "Images/gift.png";
                        break;
                    case "Undelivered":
                        result = "Images/gift-red.png";
                        break;
                }
                return result;
            }
            set
            {
            }
        }
        [DataMember]
        public string Address { get; set; }
        public string Distance
        {
            get
            {
                return "10 km";
            }
            set
            {
            }
        }

        public Delivery(string id, string status, string address)
        {
            Id = id;
            Status = status;
            Address = address;           
        }

    }
}
