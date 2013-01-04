using System.Collections.Generic;
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
        private readonly CoffeeSearcher _coffeeSearcher = new CoffeeSearcher();
        private const decimal MinLatitude = 47.45M;
        private const decimal MaxLatitude = 47.78M;

        private const decimal MinLongitude = -122.45M;
        private const decimal MaxLongitude = -122.24M;

        private const decimal CoordinateStep = 0.001M;

        private const double SearchOverlapPercent = 0.1;

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

            Dictionary<string, CoffeeShop> shopDict = new Dictionary<string, CoffeeShop>();

            for (decimal latitude = MinLatitude; latitude < MaxLatitude; latitude += CoordinateStep)
            for (decimal longitude = MinLongitude; longitude < MaxLongitude; longitude += CoordinateStep)
            {
                SearchArea searchArea = new SearchArea
                {
                    SouthwestCorner = new Coordinate
                    {
                        latitude = (double)latitude,
                        longitude = (double)longitude,
                    },
                    NortheastCorner = new Coordinate
                    {
                        latitude = (double)latitude + ((double)CoordinateStep * (1 + SearchOverlapPercent)),
                        longitude = (double)longitude + ((double)CoordinateStep * (1 + SearchOverlapPercent)),
                    }
                };

                IEnumerable<CoffeeShop> shops = _coffeeSearcher.SearchYelp(yelp, searchArea).Result;

                int numShops = 0;
                int numNewShops = 0;
                foreach (var coffeeShop in shops)
                {
                    if (!shopDict.ContainsKey(coffeeShop.YelpId))
                    {
                        shopDict[coffeeShop.YelpId] = coffeeShop;
                        coffeeShops.Add(coffeeShop);
                        numNewShops++;
                    }
                    numShops++;
                }
            }

            entities.SaveChanges();
        }
    }
}
