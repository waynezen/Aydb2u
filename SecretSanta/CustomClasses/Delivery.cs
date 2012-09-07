using System.Runtime.Serialization;
using System.Device.Location;

namespace SecretSanta.CustomClasses
{
    [DataContract]
    public class Delivery
    {
        private int _status;
        private string _note;
        private string _secondaryNote;
        private string _comment;

        [DataMember]
        public string Id { get; set; }
        
        [DataMember]
        public int Status 
        { 
            get
            {
                return _status;
            } 
            set
            {
                if (value != _status)
                {
                    _status = value;
                    Changed = true;
                }
            } 
        }

        public string Note
        {
            get
            {
                return _note;
            }
            set
            {
                if (value != _note)
                {
                    _note = value;
                    Changed = true;
                }
            }
        }

        public string SecondaryNote
        {
            get
            {
                return _secondaryNote;
            }
            set
            {
                if (value != _secondaryNote)
                {
                    _secondaryNote = value;
                    Changed = true;
                }
            }
        }

        public string Comment
        {
            get
            {
                return _comment;
            }
            set
            {
                if (value != _comment)
                {
                    _comment = value;
                    Changed = true;
                }
            }
        }
        
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
                    case 1:
                        result = "Images/gift.png";
                        break;
                    case 2:
                        result = "Images/gift-red.png";
                        break;
                }
                return result;
            }
        }

        public bool Changed { get; private set; }
        
        [DataMember]
        public Address Address { get; set; }

        public double Distance
        {
            get
            {
                var currentLocation = new GeoCoordinateWatcher(GeoPositionAccuracy.Default).Position;
                if (currentLocation == null || currentLocation.Location.IsUnknown)
                {
                    currentLocation.Location = new GeoCoordinate(53.579022, -113.522769);
                }

                var destination = new GeoCoordinate(Address.Latitude, Address.Longitude);

                return (double)(decimal.Round(((decimal)currentLocation.Location.GetDistanceTo(destination) / 1000), 1));
            }
        }

        public Delivery(string id, int status, Address address)
        {
            Id = id;
            Status = status;
            Address = address;
            Changed = false;
        }

    }
}
