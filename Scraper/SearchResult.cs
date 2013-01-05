using System.Collections.Generic;
using Entities;

namespace Scraper
{
    public class SearchResult
    {
        public SearchError Error { get; set; }
        public IEnumerable<CoffeeShop> Results { get; set; }
    }

    public class SearchError
    {
        public ErrorId Id { get; set; }

        public string Description { get; set; }
    }

    public enum ErrorId
    {
        YelpError,

        TooManyResults,
    }
}
