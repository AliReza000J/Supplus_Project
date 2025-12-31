using Newtonsoft.Json;

namespace _01_PharmaPlusQuery.Contracts.Dashbord
{
    public class ChartDto
    {
        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        [JsonProperty(PropertyName = "data")]
        public List<double> Data { get; set; }

        [JsonProperty(PropertyName = "backgroundColor")]
        public List<string> BackgroundColor { get; set; } = new List<string>();

        [JsonProperty(PropertyName = "labels")]
        public List<string> labels { get; set; } = new List<string>();

        [JsonProperty(PropertyName = "borderColor")]
        public string BorderColor { get; set; }
    }
}
