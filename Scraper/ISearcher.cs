using System.Threading.Tasks;

namespace Scraper
{
    public interface ISearcher
    {
        Task<SearchResult> Search(SearchArea searchArea);
    }
}