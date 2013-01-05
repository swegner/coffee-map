using System.Linq;
using System.Threading.Tasks;
using YelpSharp.Data;

namespace Scraper
{
    public class SplitRegionSearcher : ISearcher
    {
        private const double AreaOverlapPercent = 0.1;

        private readonly ISearcher _innerSearcher;

        public SplitRegionSearcher(ISearcher innerSearcher)
        {
            this._innerSearcher = innerSearcher;
        }

        public async Task<SearchResult> Search(SearchArea searchArea)
        {
            SearchResult result = await this._innerSearcher.Search(searchArea);
            if (result.Error != null && result.Error.Id == ErrorId.TooManyResults)
            {
                SearchArea regionA;
                SearchArea regionB;

                double latitudeDist = searchArea.NortheastCorner.latitude - searchArea.SouthwestCorner.latitude;
                double longitudeDist = searchArea.NortheastCorner.longitude - searchArea.SouthwestCorner.longitude;
                if (latitudeDist > longitudeDist)
                {
                    double latitudeDelta = latitudeDist*(1 + AreaOverlapPercent)/2;
                    regionA = new SearchArea
                    {
                        SouthwestCorner = searchArea.SouthwestCorner,
                        NortheastCorner = new Coordinate
                        {
                            latitude = searchArea.SouthwestCorner.latitude + latitudeDelta,
                            longitude = searchArea.NortheastCorner.longitude,
                        },
                    };
                    regionB = new SearchArea
                    {
                        SouthwestCorner = new Coordinate
                        {
                            latitude = searchArea.NortheastCorner.latitude - latitudeDelta,
                            longitude = searchArea.SouthwestCorner.longitude,
                        },
                        NortheastCorner = searchArea.NortheastCorner,
                    };
                }
                else
                {
                    double longitudeDelta = longitudeDist*(1 + AreaOverlapPercent)/2;
                    regionA = new SearchArea
                    {
                        SouthwestCorner = searchArea.SouthwestCorner,
                        NortheastCorner = new Coordinate
                        {
                            latitude = searchArea.NortheastCorner.latitude,
                            longitude = searchArea.SouthwestCorner.longitude + longitudeDelta,
                        },
                    };
                    regionB = new SearchArea
                    {
                        SouthwestCorner = new Coordinate
                        {
                            latitude = searchArea.SouthwestCorner.latitude,
                            longitude = searchArea.NortheastCorner.longitude - longitudeDelta,
                        },
                        NortheastCorner = searchArea.NortheastCorner,
                    };
                }

                Task<SearchResult>[] subTasks = new[]
                {
                    this.Search(regionA),
                    this.Search(regionB),
                };

                SearchResult[] subResults = new[]
                {
                    subTasks[0].Result,
                    subTasks[1].Result,
                };

                SearchError error = subResults
                    .Where(r => r.Error != null)
                    .Select(r => r.Error)
                    .FirstOrDefault();

                if (error != null)
                {
                    result = new SearchResult
                    {
                        Error = error,
                    };
                }
                else
                {
                    result = new SearchResult
                    {
                        Results = subResults
                            .SelectMany(r => r.Results),
                    };
                }
            }

            return result;
        }
    }
}
