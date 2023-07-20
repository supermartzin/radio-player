using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

using Radio.Player.Models;
using Radio.Player.Providers.Rtvs.Utilities;
using Radio.Player.Services.Contracts;

namespace Radio.Player.Providers.Rtvs;

public partial class RtvsWebArticlesService : IArticlesService
{
    private const string DateTimeFormat = "d. MM. yyyy HH:mm";

    private readonly ILogger<RtvsWebArticlesService>? _logger;

    private readonly IHttpClientFactory _httpClientFactory;

    public RtvsWebArticlesService(IHttpClientFactory httpClientFactory,
                                  ILogger<RtvsWebArticlesService>? logger = default)
    {
        _httpClientFactory = httpClientFactory
                             ?? throw new ArgumentNullException(nameof(httpClientFactory));

        _logger = logger;
    }

    public async Task<FullArticle?> GetFullArticleAsync(string articleUrl,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(articleUrl);

        try
        {
            using var client = _httpClientFactory.CreateClient();

            var downloadedPage = await client.GetStringAsync(articleUrl, cancellationToken);

            var html = new HtmlDocument();

            html.LoadHtml(downloadedPage); // load a string

            var articles = html.DocumentNode
                               .Descendants("div")
                               .Where(d => d.AttributeValueOrEmpty("class").Contains("article"))
                               .Select(ParseArticle);

            var article = articles.FirstOrDefault();
            if (article is null)
                return null;

            article.Url = articleUrl;

            return article;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error getting Full Article from RTVS: {ex.Message}");

            return null;
        }
    }


    private static FullArticle ParseArticle(HtmlNode html)
    {
        var title = GetArticleHeader(html)?.Element("h2")
            .InnerText;
        var section = GetArticleHeader(html)?.Element("p")
            .InnerText;
        var dateTime = GetArticleContent(html)?.Descendants("p")
            .FirstOrDefault(e => e.ContainsAttributeValue("class", "day"))?
            .InnerText;
        var imageUrl = GetArticleContent(html)?.Descendants("span")
            .FirstOrDefault(e => e.ContainsAttributeValue("class", "article-image"))?
            .Element("img")
            .AttributeValueOrEmpty("src");
        var bodyParts = GetArticleContent(html)?.Descendants("p")
            .Where(e => !e.ContainsAttributeValue("class", "day"))
            .Select(e => $"<p>{e.InnerHtml}</p>");
        var body = string.Join("", bodyParts);
        var authorName = GetArticleContent(html)?.Descendants("div")
            .FirstOrDefault(div => div.ContainsAttributeValue("class", "source_info"))?
            .InnerText
            .Trim();

        SanitizeYouTubeLinks(ref body);

        return new FullArticle
        {
            Title = title,
            Section = section,
            Body = $"<div>{body}</div>",
            AuthorName = authorName,
            ImageUrl = imageUrl,
            PublishDate = Extensions.ToDateTime(dateTime, DateTimeFormat)
        };
    }

    private static void SanitizeYouTubeLinks(ref string body)
    {
        var regex = SrcLinksRegex();
        foreach (var match in regex.Matches(body).Cast<Match>())
        {
            if (!match.Success)
                continue;

            var originalLink = match.Groups[0].Value;
            var link = match.Groups[1].Value;

            body = body.Replace(originalLink, $"src=\"https://www.{link}\"");
        }
    }

    private static HtmlNode? GetArticleHeader(HtmlNode root) => root.Elements("div").FirstOrDefault(e => e.ContainsAttributeValue("class", "article-header"));

    private static HtmlNode? GetArticleContent(HtmlNode root) => root.Elements("div").FirstOrDefault(e => e.ContainsAttributeValue("class", "article-content"));

    [GeneratedRegex("src=\".*www\\.(.*)?\"")]
    private static partial Regex SrcLinksRegex();
}
