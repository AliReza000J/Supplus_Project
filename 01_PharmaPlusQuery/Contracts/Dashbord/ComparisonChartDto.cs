using Newtonsoft.Json;

namespace _01_PharmaPlusQuery.Contracts.Dashbord
{
    public class ComparisonChartDto
    {
        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; } // عنوان کلی نمودار (مثلاً "مقایسه فروش")

        [JsonProperty(PropertyName = "currentValue")]
        public double CurrentValue { get; set; } // مقدار دوره فعلی

        [JsonProperty(PropertyName = "previousValue")]
        public double PreviousValue { get; set; } // مقدار دوره قبلی

        [JsonProperty(PropertyName = "labels")]
        public List<string> Labels { get; set; } = new List<string>(); // نام دوره‌ها (مثلاً "آبان" و "آذر")

        public double GrowthPercentage
        {
            get
            {
                if (PreviousValue == 0) return CurrentValue > 0 ? 100 : 0;
                return Math.Round(((CurrentValue - PreviousValue) / PreviousValue) * 100, 1);
            }
        }
    }
}
