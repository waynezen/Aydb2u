using System.Runtime.Serialization;

namespace SecretSanta.CustomClasses
{
    [DataContract]
    public class Delivery
    {
        [DataMember]
        public string Id { get; set; }
        
        [DataMember]
        public int Status { get; set; }
        
        public string Image
        {
            get
            {
                string result ="";
                switch (Status)
                {
                    case 0:
                        result= "Images/gift-yellow.png";
                        break;
                    case 2:
                        result = "Images/gift.png";
                        break;
                    case 1:
                        result = "Images/gift-red.png";
                        break;
                }
                return result;
            }
        }

        public bool Updated { get; private set; }
        
        [DataMember]
        public Address Address { get; set; }

        public string Distance
        {
            get
            {
                //var currentLocation = new GeoCoordinateWatcher(GeoPositionAccuracy.Default).Position;
                //if (currentLocation == null || currentLocation.Location.IsUnknown)
                //{
                //    currentLocation.Location = new GeoCoordinate(53.579022, -113.522769);
                //}

                //var destination = new LabeledMapLocation(Address, null);

                //return currentLocation.Location.GetDistanceTo(destination.Location);
                return "10 KM";
            }
        }

        public Delivery(string id, int status, Address address)
        {
            Id = id;
            Status = status;
            Address = address;           
        }

    }
}
