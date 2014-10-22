namespace MoviesSample.AzureSearch.Web.Models
{
    public class MovieResult
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public double Rating { get; set; }
        public string ImageUrl { get; set; }
        public string Url { get; set; }
        public string Episode { get; set; }
    }
}