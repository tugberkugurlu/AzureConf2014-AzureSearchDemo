using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using MoviesSample.AzureSearch.Infrastructure;
using MoviesSample.AzureSearch.Models;
using Newtonsoft.Json;

namespace MoviesSample.AzureSearch
{
    class Program
    {
        private const string IndexName = "movies";

        static void Main(string[] args)
        {
            using (HttpClient client = CreateClient())
            {
                // 1-) Create the index schema
                CreateIndexSchema(client);

                // 2-) Populate the index
                PopulateTheIndex(client);
            }
        }

        static void PopulateTheIndex(HttpClient client)
        {
            List<MovieIndexCreateRequest> moviesBag = new List<MovieIndexCreateRequest>();
            const string filePath = @"C:\Dev\imdb.json\imdb.json";
            using (Stream fileStream = File.OpenRead(filePath))
            using (StreamReader reader = new StreamReader(fileStream))
            using (JsonTextReader jsonReader = new JsonTextReader(reader))
            {
                IDictionary<string, object> values = new Dictionary<string, object>();
                while (jsonReader.Read())
                {
                    JsonToken tokenType = jsonReader.TokenType;
                    if (tokenType == JsonToken.StartArray || tokenType == JsonToken.StartObject || tokenType == JsonToken.StartConstructor)
                    {
                        continue;
                    }

                    if (tokenType == JsonToken.EndArray || tokenType == JsonToken.EndConstructor)
                    {
                        continue;
                    }

                    if (tokenType == JsonToken.EndObject)
                    {
                        var request = GetMovieIndexCreateRequest(values);
                        if (request != null)
                        {
                            moviesBag.Add(request);
                        }

                        values.Clear();
                        continue;
                    }

                    object tokenValue = jsonReader.Value;
                    if (tokenType == JsonToken.PropertyName)
                    {
                        values.Add(tokenValue.ToString(), null);
                    }
                    else
                    {
                        KeyValuePair<string, object> lastObject = values.Last();
                        values[lastObject.Key] = tokenValue;
                    }

                    if (moviesBag.Count == 500)
                    {
                        AddDocuments(client, moviesBag);
                        moviesBag.Clear();
                    }
                }

                if (moviesBag.Any())
                {
                    AddDocuments(client, moviesBag);
                    moviesBag.Clear();
                }
            }
        }

        static MovieIndexCreateRequest GetMovieIndexCreateRequest(IDictionary<string, object> values)
        {
            // If contains rating, this is a movie.
            if (values.ContainsKey("rating"))
            {
                Movie movie;
                try
                {
                    movie = new Movie(values);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to create a Movie object. Skipping.");
                    movie = null;
                }

                if (movie != null)
                {
                    string id = Guid.NewGuid().ToString();
                    return new MovieIndexCreateRequest(id, movie);
                }
            }

            return null;
        }

        static HttpClient CreateClient()
        {
            string serviceUri = string.Format("https://{0}.search.windows.net", ConfigurationManager.AppSettings["AzureSearch:ServiceName"]);
            string primaryApiKey = ConfigurationManager.AppSettings["AzureSearch:ApiKey"];

            VersionAppenderHandler versionAppenderHandler = new VersionAppenderHandler();
            HttpMessageHandler innerHandler = HttpClientFactory.CreatePipeline(
                new HttpClientHandler(), new DelegatingHandler[] { versionAppenderHandler });

            var client = new HttpClient(innerHandler);
            client.BaseAddress = new Uri(serviceUri);
            client.DefaultRequestHeaders.TryAddWithoutValidation("api-key", primaryApiKey);

            return client;
        }

        static void CreateIndexSchema(HttpClient client)
        {
            var schemaPayload = File.ReadAllText(@".\movie-index-schema.json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "indexes")
            {
                Content = new StringContent(schemaPayload, Encoding.UTF8, "application/json")
            };

            client.SendAsync(request).Wait();
        }

        static void AddDocuments(HttpClient client, IEnumerable<MovieIndexCreateRequest> createRequests)
        {
            IndexCreateRequest<MovieIndexCreateRequest> request = new IndexCreateRequest<MovieIndexCreateRequest>
            {
                Values = createRequests
            };

            var response = client.PostAsJsonAsync(string.Format("indexes/{0}/docs/index", IndexName), request).Result;
            if (response.IsSuccessStatusCode)
            {
                IndexUploadResult indexUploadResult = response.Content.ReadAsAsync<IndexUploadResult>().Result;
                foreach (IndexUploadResultItem indexUploadResultItem in indexUploadResult.Values)
                {
                    if (indexUploadResultItem.Status == true)
                    {
                        Console.WriteLine(indexUploadResultItem.Status == true
                            ? "Added one Movie. Id: {0}"
                            : "Couldn't added Movie. Id: {0}, Error: {1}", indexUploadResultItem.Key, indexUploadResultItem.ErrorMessage);
                    }
                }
            }
        }
    }

    public class IndexUploadResult
    {
        [JsonProperty(PropertyName = "value")]
        public IEnumerable<IndexUploadResultItem> Values { get; set; }
    }

    public class IndexUploadResultItem
    {
        [JsonProperty(PropertyName = "key")]
        public string Key { get; set; }

        [JsonProperty(PropertyName = "status")]
        public bool Status { get; set; }

        [JsonProperty(PropertyName = "errorMessage")]
        public string ErrorMessage { get; set; }
    }
}