using System.Runtime.Serialization;

namespace Entities
{
    [DataContract]
    public class CoffeeShop
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string YelpId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Coordinates Location { get; set; }
    }
}
