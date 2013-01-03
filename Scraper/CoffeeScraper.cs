using System.Linq;
using System.Threading.Tasks;
using Entities;
using YelpSharp;
using YelpSharp.Data;
using YelpSharp.Data.Options;
using Scraper.Configuration;
using System.Configuration;

namespace Scraper
{
    public class CoffeeScraper
    {
        public void Run()
        {
            CoffeeEntities entities = new CoffeeEntities();
            var coffeeShops = entities.CoffeeShops;

            foreach (var shop in coffeeShops)
            {
                coffeeShops.Remove(shop);
            }
            entities.SaveChanges();

            YelpElement yelpConfig = ((ApiKeySection)ConfigurationManager.GetSection(ApiKeySection.SectionName)).Yelp;
            Yelp yelp = new Yelp(new Options
            {
                ConsumerKey = yelpConfig.ConsumerKey,
                ConsumerSecret = yelpConfig.ConsumerSecret,
                AccessToken = yelpConfig.Token,
                AccessTokenSecret = yelpConfig.TokenSecret,
            });
            Task<SearchResults> searchTask = yelp.Search(new SearchOptions
            {
                GeneralOptions = new GeneralOptions
                {
                    category_filter = "Coffee",
                },
                LocationOptions = new LocationOptions
                {
                    location = "Seattle, WA",
                },
            });

            var results = searchTask.Result;
            var newShops = results.businesses
                .Select(b => new CoffeeShop
                {
                    Name = b.name,
                    Location = new Coordinates
                    {
                        Latitude = b.location.coordinate.latitude,
                        Longitude = b.location.coordinate.longitude,
                    },
                    YelpId = b.id,
                });
            foreach (var shop in newShops)
            {
                coffeeShops.Add(shop);
            }

            entities.SaveChanges();
        }
    }
}
