namespace SiteManagement.Domain.SiteSettingAgg
{
    public class SiteSetting
    {
        public long Id { get; private set; }
        public string Data { get; private set; } = "{}";
        public DateTime UpdatedAt { get; private set; }

        private SiteSetting() { }

        public static SiteSetting CreateDefault()
        {
            return new SiteSetting
            {
                Data = "{}",
                UpdatedAt = DateTime.Now
            };
        }

        public void Update(string json)
        {
            Data = json;
            UpdatedAt = DateTime.Now;
        }
    }
}
