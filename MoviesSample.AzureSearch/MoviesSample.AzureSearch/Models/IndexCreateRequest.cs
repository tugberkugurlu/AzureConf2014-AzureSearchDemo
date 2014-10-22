using System.Collections.Generic;
using Newtonsoft.Json;

namespace MoviesSample.AzureSearch.Models
{
    public class IndexCreateRequest<T>
    {
        [JsonProperty(PropertyName = "value")]
        public IEnumerable<T> Values { get; set; }
    }
}