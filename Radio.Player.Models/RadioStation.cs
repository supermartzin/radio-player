namespace Radio.Player.Models
{
    public class RadioStation
    {
        public string Name { get; set; }

        public RadioStreamUrls StreamUrls { get; set; }

        public string WebsiteUrl { get; internal set; }
    }
}
