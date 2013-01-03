using System.Data.Entity;

namespace Entities
{
    public class CoffeeEntities : DbContext
    {
        public DbSet<CoffeeShop> CoffeeShops { get; set; }
    }
}
