using System.Runtime.Serialization;

namespace Entities
{
    public class CoffeeShop
    {
        public int Id { get; set; }
        public string YelpId { get; set; }
        public string Name { get; set; }
        public Coordinates Location { get; set; }
    }
}
