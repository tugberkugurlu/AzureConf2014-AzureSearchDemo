namespace MoviesSample.AzureSearch.Web.Models
{
    public class SearchRequest
    {
        public string SearchTerm { get; set; }
        public int? YearStart { get; set; }
        public int? YearEnd { get; set; }
    }
}