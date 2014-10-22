using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MoviesSample.AzureSearch.Models
{
    public class Movie
    {
        /*
          
          {
            "name": "Felicity",
            "url": "/title/tt0578654/",
            "image": "http://ia.media-imdb.com/images/M/MV5BMjA5OTgyMDE3Nl5BMl5BanBnXkFtZTcwNjE1NzcyMQ@@._V1._SX54_CR0,0,54,74_.jpg",
            "rating": 7.6,
            "year": "(1998 TV Series)",
            "nb_voters": 14,
            "episode": "And to All a Good Night",
            "rank": 35973
          }
         
          {
            "name": "The Shawshank Redemption",
            "url": "/title/tt0111161/",
            "image": "http://ia.media-imdb.com/images/M/MV5BMTc3NjM4MTY3MV5BMl5BanBnXkFtZTcwODk4Mzg3OA@@._V1._SX54_CR0,0,54,74_.jpg",
            "rating": 9.3,
            "year": "(1994)",
            "nb_voters": 1010572,
            "rank": 1
          }
         
         */

        private static readonly Regex Regex = new Regex(@"^.*?\([^\d]*(\d+)[^\d]*\).*$", RegexOptions.Compiled);

        public Movie()
        {
        }

        public Movie(IDictionary<string, object> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            Name = (string)values["name"];
            Url = (string)values["url"];
            Image = (string)values["image"];
            Rating = (double)values["rating"];
            Year = int.Parse(Regex.Match((string)values["year"]).Groups[1].ToString());
            NbVoters = int.Parse(values["nb_voters"].ToString());
            Rank = int.Parse(values["nb_voters"].ToString());
        }

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