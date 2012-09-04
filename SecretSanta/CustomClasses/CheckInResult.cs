using System.Runtime.Serialization;

namespace SecretSanta.CustomClasses
{
    [DataContract]
    public class CheckInResult
    {
        [DataMember]
        public string Key { get; set; }
    }
}
