using System.Runtime.Serialization;
using System.Text;

namespace SecretSanta.CustomClasses
{
    [DataContract]
    public class Address
    {
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
