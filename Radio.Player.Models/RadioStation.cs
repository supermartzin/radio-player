namespace Radio.Player.Models;

public record RadioStation
{
    public string Name { get; set; }

    public RadioStreamUrls StreamUrls { get; set; }

    public string WebsiteUrl { get; set; }

    public string PlaylistUrl { get; set; }

    public string PodcastsUrl { get; set; }

    public string ScheduleUrl { get; set; }
}