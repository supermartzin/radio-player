namespace Radio.Player.Models;

public record Track
{
    public string Title { get; set; }

    public string Artist { get; set; }

    public DateTime TimeAired { get; set; }
}