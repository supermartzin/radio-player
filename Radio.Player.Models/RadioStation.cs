namespace Radio.Player.Models
{
    public class RadioStation
    {
        public string Name { get; set; }

        public RadioStreamUrls StreamUrls { get; set; }

        public string WebsiteUrl { get; internal set; }

        public string PlaylistUrl { get; set; }

        public string PodcastsUrl { get; set; }

        public string ScheduleUrl { get; set; }

        public string GetStreamUrl(StreamQualityType streamQualityType) => streamQualityType switch
        {
            StreamQualityType.Low => StreamUrls.LowQualityUrl ?? StreamUrls.DefaultQualityUrl,
            StreamQualityType.Medium => StreamUrls.MediumQualityUrl ?? StreamUrls.DefaultQualityUrl,
            StreamQualityType.High => StreamUrls.HighQualityUrl ?? StreamUrls.DefaultQualityUrl,
            _ => StreamUrls.DefaultQualityUrl
        };
    }
}
