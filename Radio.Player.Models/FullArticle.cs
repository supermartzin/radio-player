namespace Radio.Player.Models;

public record FullArticle
{
    public string Title { get; set; }

    public string Subtitle { get; set; }

    public string Section { get; set; }

    public string Body { get; set; }

    public string AuthorName { get; set; }
        
    public DateTime PublishDate { get; set; }
        
    public string Url { get; set; }

    public string ImageUrl { get; set; }
}