using System.Text.Json.Serialization;

namespace AnimalCrossingTracker.Models
{
    public class Variation
    {
        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("variation")]
        public string VariantName { get; set; }

        [JsonPropertyName("colors")]
        public List<string> Colors { get; set; }
    }
}
