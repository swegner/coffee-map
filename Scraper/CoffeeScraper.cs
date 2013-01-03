using System.Linq;
using System.Threading.Tasks;
using Entities;
using YelpSharp;
using YelpSharp.Data;
using YelpSharp.Data.Options;
using Scraper.Configuration;
using System.Configuration;
using System;

namespace Scraper
{
    public class CoffeeScraper
    {
        private const double MinLatitude = 47.45;
        private const double MaxLatitude = 47.78;

        private const double MinLongitude = -122.45;
        private const double MaxLongitude = -122.24;

        private static string[] Neighborhoods = new[]
        {
            "Beacon Hill",
            "Belltown",
            "Bitter Lake",
            "Broadview",
            "Bryant",
            "Capitol Hill",
            "Central District",
            "Columbia City",
            "Downtown",
            "Eastlake",
            "First Hill",
            "Fremont",
            "Georgetown",
            "Green Lake",
            "Greenwood",
            "Haller Lake",
            "International District",
            "Lake City",
            "Laurelhurst",
            "Madison Park",
            "Madrona/Leschi",
            "Magnolia",
            "Maple Leaf",
            "Matthews Beach",
            "Meadowbrook",
            "Montlake",
            "Mt. Baker",
            "Northgate",
            "Phinney Ridge",
            "Pioneer Square",
            "Queen Anne",
            "Rainier Beach",
            "Rainier Valley",
            "Ravenna",
            "Roosevelt",
            "Sand Point",
            "Seward Park",
            "SODO",
            "University District",
            "Wallingford",
            "Wedgwood/View Ridge",
            "West Seattle",
            "White Center",
        };

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

            foreach (string neighborhood in Neighborhoods)
            {
                string location = string.Format("{0}, Seattle, WA", neighborhood);

                int offset = 0;
                int numFound;
                do
                {
                    SearchOptions query = new SearchOptions
                    {
                        GeneralOptions = new GeneralOptions
                        {
                            category_filter = "coffee",
                            limit = 20,
                            offset = offset,
                            sort = 1,
                        },
                        LocationOptions = new LocationOptions
                        {
                            location = location,
                        },
                    };
                    Task<SearchResults> searchTask = yelp.Search(query);

                    var results = searchTask.Result;
                    numFound = results.businesses.Count;
                    if (results.error != null)
                    {
                        throw new Exception(results.error.text);
                    }

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

                    offset += numFound;
                } while (numFound > 0);
            }

            entities.SaveChanges();
        }
    }
}
