using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

using Radio.Player.Models;
using Radio.Player.Services;
using Radio.Player.Services.Utilities;

using Radio.PLayer.Providers.Rtvs.Utilities;

namespace Radio.PLayer.Providers.Rtvs
{
    public class RtvsArticlesService : IArticlesService
    {
        private const string DateTimeFormat = "d. MM. yyyy HH:mm";

        public async Task<FullArticle> GetFullArticle(string articleUrl)
        {
            if (articleUrl == null)
                return null;

            try
            {
                using var client = new HttpClient();

                var downloadedPage = await client.GetStringAsync(articleUrl).ConfigureAwait(false);

                var html = new HtmlDocument();
                html.LoadHtml(downloadedPage); // load a string

                var articles = html.DocumentNode
                    .Descendants("div")
                    .Where(d => d.AttributeValueOrEmpty("class")
                        .Contains("article"))
                    .Select(ParseArticle);

                var article = articles.FirstOrDefault();
                if (article == null)
                    return null;

                article.Url = articleUrl;
                return article;
            }
            catch
            {
                return null;
            }
        }


        private static FullArticle ParseArticle(HtmlNode html)
        {
            var title = GetArticleHeader(html).Element("h2")
                                                     .InnerText;
            var section = GetArticleHeader(html).Element("p")
                                                      .InnerText;
            var dateTime = GetArticleContent(html).Descendants("p")
                                                        .FirstOrDefault(e => e.ContainsAttributeValue("class", "day"))?
                                                        .InnerText;
            var imageUrl = GetArticleContent(html).Descendants("span")
                                                        .FirstOrDefault(e => e.ContainsAttributeValue("class", "article-image"))?
                                                        .Element("img")
                                                        .AttributeValueOrEmpty("src");
            var bodyParts = GetArticleContent(html).Descendants("p")
                                                                     .Where(e => !e.ContainsAttributeValue("class", "day"))
                                                                     .Select(e => $"<p>{e.InnerHtml}</p>");
            var body = string.Join("", bodyParts);
            var authorName = GetArticleContent(html).Descendants("div")
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
                PublishDate = TypeConverter.ToDateTime(dateTime, DateTimeFormat)
            };
        }

        private static void SanitizeYouTubeLinks(ref string body)
        {
            var regex = new Regex("src=\".*www\\.(.*)?\"");
            foreach (Match match in regex.Matches(body))
            {
                if (!match.Success)
                    continue;

                var originalLink = match.Groups[0].Value;
                var link = match.Groups[1].Value;

                body = body.Replace(originalLink, $"src=\"https://www.{link}\"");
            }
        }
        
        private static HtmlNode GetArticleHeader(HtmlNode root) => root.Elements("div").FirstOrDefault(e => e.ContainsAttributeValue("class", "article-header"));

        private static HtmlNode GetArticleContent(HtmlNode root) => root.Elements("div").FirstOrDefault(e => e.ContainsAttributeValue("class", "article-content"));
    }
}