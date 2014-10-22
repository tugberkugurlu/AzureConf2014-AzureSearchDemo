using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using MoviesSample.AzureSearch.Web.Models;
using RedDog.Search;
using RedDog.Search.Http;
using RedDog.Search.Model;

namespace MoviesSample.AzureSearch.Web.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View(new HomeViewModel
            {
                Movies = Enumerable.Empty<MovieResult>(),
                SearchRequest = new SearchRequest(),
                Facets = new Dictionary<string, long>()
            });
        }

        [HttpPost]
        [ActionName("Index")]
        public async Task<ActionResult> IndexPost([Bind(Prefix = "SearchRequest")]SearchRequest searchRequest)
        {
            string serviceName = ConfigurationManager.AppSettings["AzureSearch:ServiceName"];
            string primaryApiKey = ConfigurationManager.AppSettings["AzureSearch:ApiKey"];
            using (var apicConnection = ApiConnection.Create(serviceName, primaryApiKey))
            {
                var queryClient = new IndexQueryClient(apicConnection);
                var query = new SearchQuery(searchRequest.SearchTerm)
                    .Filter(string.Format("Year gt {0} and Year lt {1}", searchRequest.YearStart, searchRequest.YearEnd))
                    .Facet("Year")
                    .Top(100);

                IApiResponse<SearchQueryResult> searchResults = await queryClient.SearchAsync("movies", query);
                var movies = searchResults.Body.Records.Select(x => new MovieResult
                {
                    Id = x.Properties["Id"].ToString(),
                    Title = x.Properties["Name"].ToString(),
                    Year = int.Parse(x.Properties["Year"].ToString()),
                    Rating = double.Parse(x.Properties["Rating"].ToString()),
                    ImageUrl = x.Properties["Image"].ToString(),
                    Url = x.Properties["Url"].ToString(),
                    Episode = x.Properties["Episode"] != null
                        ? x.Properties["Episode"].ToString()
                        : null
                });

                KeyValuePair<string, FacetResult[]> yearFacet = searchResults.Body.Facets.FirstOrDefault();

                return View(new HomeViewModel
                {
                    Movies = movies,
                    SearchRequest = searchRequest,
                    Facets = (yearFacet.Value != null)
                        ? yearFacet.Value.ToDictionary(x => x.Value, x => x.Count)
                        : new Dictionary<string, long>()
                });
            }
        }
    }
}