using System;
using Newtonsoft.Json;

namespace MoviesSample.AzureSearch.Models
{
    public class MovieIndexCreateRequest
    {
        public MovieIndexCreateRequest(string id, Movie movie)
        {
            if (movie == null)
            {
                throw new ArgumentNullException("movie");
            }

            Id = id;
            Name = movie.Name;
            Url = movie.Url;
            Image = movie.Image;
            Rating = movie.Rating;
            Year = movie.Year;
            Episode = movie.Episode;
            NbVoters = movie.NbVoters;
            Rank = movie.Rank;
        }

        [JsonProperty(PropertyName = "@search.action")]
        public string Action
        {
            get { return "upload"; }
        }

        public string Id { get; set; }

        public string Name { get; set; }
        public string Url { get; set; }
        public string Image { get; set; }
        public double Rating { get; set; }
        public int Year { get; set; }
        public string Episode { get; set; }
        public int NbVoters { get; set; }
        public int Rank { get; set; }
    }
}