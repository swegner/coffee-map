using System.Runtime.Serialization;

namespace Entities
{
    [DataContract]
    public class Coordinates
    {
        [DataMember]
        public double Latitude { get; set; }

        [DataMember]
        public double Longitude { get; set; }
    }
}
