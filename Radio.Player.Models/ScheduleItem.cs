using System;

namespace Radio.Player.Models
{
    public class ScheduleItem
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public TimeSpan Length { get; set; }

        public string Category { get; set; }
    }
}
