using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities;
using YelpSharp;
using YelpSharp.Data;
using YelpSharp.Data.Options;

namespace Scraper
{
    public class CoffeeSearcher
    {
        public async Task<IEnumerable<CoffeeShop>> SearchYelp(Yelp yelp, SearchArea searchArea)
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
    }
}