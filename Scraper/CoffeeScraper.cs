﻿using System.Collections.Generic;
using System.Diagnostics;
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

                IEnumerable<CoffeeShop> shops = this.SearchYelp(yelp, searchArea).Result;

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

        private async Task<IEnumerable<CoffeeShop>> SearchYelp(Yelp yelp, SearchArea searchArea)
        {
            List<CoffeeShop> coffeeShops = new List<CoffeeShop>();

            int numFound;
            int offset = 0;
            do
            {
                SearchOptions query = new SearchOptions
                {
                    GeneralOptions = new GeneralOptions
                    {
                        category_filter = "coffee",
                        offset = offset,
                        limit = 20,
                        sort = 1,
                    },
                    LocationOptions = new BoundOptions
                    {
                        sw_latitude = searchArea.SouthwestCorner.latitude,
                        sw_longitude = searchArea.SouthwestCorner.longitude,
                        ne_latitude = searchArea.NortheastCorner.latitude,
                        ne_longitude = searchArea.NortheastCorner.longitude,
                    },
                };
                Task<SearchResults> searchTask = yelp.Search(query);
                SearchResults results = await searchTask;

                if (results.error != null)
                {
                    throw new Exception(results.error.text);
                }

                numFound = results.businesses.Count;
                if (results.total > 40)
                {
                    throw new InvalidOperationException("Greater than 40 results returned-- use a smaller region");
                }

                IEnumerable<CoffeeShop> newShops = results.businesses
                    .Where(b => b.location.city.IndexOf("seattle", StringComparison.OrdinalIgnoreCase) != -1)
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

                offset += numFound;

                foreach (var shop in newShops)
                {
                    coffeeShops.Add(shop);
                }
            }
            while (numFound > 0);

            return coffeeShops;
        }

        private class SearchArea
        {
            public Coordinate SouthwestCorner { get; set; }
            public Coordinate NortheastCorner { get; set; }
        }
    }
}
