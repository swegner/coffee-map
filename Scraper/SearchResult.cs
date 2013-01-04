using System.Collections.Generic;
using Entities;

namespace Scraper
{
    public class SearchResult
    {
        public SearchError? Error { get; set; }
        public IEnumerable<CoffeeShop> Results { get; set; }
    }

    public enum SearchError
    {
        TooManyResults,
    }
}
