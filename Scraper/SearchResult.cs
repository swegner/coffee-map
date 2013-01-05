using System.Collections.Generic;
using Entities;
using System.Diagnostics;

namespace Scraper
{
    [DebuggerDisplay("Results = {Results.Count} Error = {Error}")]
    public class SearchResult
    {
        public SearchError Error { get; set; }
        public IEnumerable<CoffeeShop> Results { get; set; }
    }

    [DebuggerDisplay("{Description}")]
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
