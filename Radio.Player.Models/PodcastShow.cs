using System;

namespace Radio.Player.Models
{
    public class PodcastShow
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string Url { get; set; }

        public DateTime TimeAired { get; set; }
    }
}