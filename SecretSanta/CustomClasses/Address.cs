using System.Runtime.Serialization;
using System.Text;
using System;

namespace SecretSanta.CustomClasses
{
    [DataContract]
    public class Address
    {
        private double _latitude;
        private double _longitude;

        [DataMember]
        public string UnitOrSuite { get; set; }
        [DataMember]
        public string StreetNumber { get; set; }
        [DataMember]
        public string StreetName { get; set; }
        [DataMember]
        public string StreetType { get; set; }
        [DataMember]
        public string StreetDirection { get; set; }
        [DataMember]
        public string City { get; set; }
        [DataMember]
        public string Province { get; set; }
        [DataMember]
        public string PostalCode { get; set; }

        public double Latitude
        {
            get
            {
                if (_latitude == 0)
                {
                    var rand = new Random();
                    _latitude = (rand.Next(1, 180) - 91) + rand.NextDouble();
                }
                return _latitude;
            }
            set
            {
                _latitude = value;
            }
        }

        public double Longitude
        {
            get
            {
                if (_longitude == 0)
                {
                    var rand = new Random();
                    _longitude = (rand.Next(1, 360) - 181) + rand.NextDouble();
                }
                return _longitude;
            }
            set
            {
                _longitude = value;
            }
        }

        public override string ToString()
        {
            var addressBuilder = new StringBuilder();
            addressBuilder.Append(UnitOrSuite);

            if (!string.IsNullOrEmpty(addressBuilder.ToString()))
            {
                addressBuilder.Append(" ");    
            }

            addressBuilder.Append(StreetNumber);

            if (!string.IsNullOrEmpty(addressBuilder.ToString()))
            {
                addressBuilder.Append(" ");
            }

            addressBuilder.Append(StreetName);

            if (!string.IsNullOrEmpty(addressBuilder.ToString()))
            {
                addressBuilder.Append(" ");
            }

            addressBuilder.Append(StreetType);

            if (!string.IsNullOrEmpty(addressBuilder.ToString()))
            {
                addressBuilder.Append(" ");
            }

            addressBuilder.Append(StreetDirection);

            if (!string.IsNullOrEmpty(addressBuilder.ToString()))
            {
                addressBuilder.Append(" ");
            }

            addressBuilder.Append(City);

            if (!string.IsNullOrEmpty(addressBuilder.ToString()))
            {
                addressBuilder.Append(" ");
            }

            addressBuilder.Append(Province);

            if (!string.IsNullOrEmpty(addressBuilder.ToString()))
            {
                addressBuilder.Append(" ");
            }

            addressBuilder.Append(PostalCode);

            return addressBuilder.ToString();
        }
    }
}
