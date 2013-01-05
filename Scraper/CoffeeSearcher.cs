using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Entities;
using Scraper.Configuration;
using YelpSharp;
using YelpSharp.Data;
using YelpSharp.Data.Options;

namespace Scraper
{
    public class CoffeeSearcher : ISearcher
    {
        private const int MaxSearchResults = 40;
        private readonly Yelp _yelp;

        public CoffeeSearcher()
        {
            YelpElement yelpConfig = ((ApiKeySection)ConfigurationManager.GetSection(ApiKeySection.SectionName)).Yelp;
            this._yelp = new Yelp(new Options
            {
                ConsumerKey = yelpConfig.ConsumerKey,
                ConsumerSecret = yelpConfig.ConsumerSecret,
                AccessToken = yelpConfig.Token,
                AccessTokenSecret = yelpConfig.TokenSecret,
            });
        }

        public async Task<SearchResult> Search(SearchArea searchArea)
        {
            SearchError error = null;
            List<CoffeeShop> coffeeShops = new List<CoffeeShop>();

            int numFound;
            int offset = 0;
            do
            {
                numFound = 0;
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
                Task<SearchResults> searchTask = this._yelp.Search(query);
                SearchResults results = await searchTask;

                if (results.error != null)
                {
                    YelpSharp.Data.SearchError yelpError = results.error;
                    error = new SearchError
                    {
                        Id = ErrorId.YelpError,
                        Description = string.Format("ID: {0}; Description: {1}; Text: {2}; Field: {3}",
                            yelpError.id, yelpError.description, yelpError.text, yelpError.field),
                    };
                }
                else if (results.total > MaxSearchResults)
                {
                    error = new SearchError
                    {
                        Id = ErrorId.TooManyResults,
                        Description = string.Format("Maximum number of results exceeded, use a smaller region. Max Results: {0}", MaxSearchResults),
                    };
                }
                else
                {
                    numFound = results.total;
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

                    coffeeShops.AddRange(newShops);
                    offset += numFound;
                }
            } while (error == null && numFound > 0);

            return new SearchResult
            {
                Error = error,
                Results = coffeeShops,
            };
        }
    }
}