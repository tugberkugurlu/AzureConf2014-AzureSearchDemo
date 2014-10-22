using System.Collections.Generic;

namespace MoviesSample.AzureSearch.Web.Models
{
    public class HomeViewModel
    {
        public SearchRequest SearchRequest { get; set; }
        public IEnumerable<MovieResult> Movies { get; set; }
        public IDictionary<string, long> Facets { get; set; }
    }
}