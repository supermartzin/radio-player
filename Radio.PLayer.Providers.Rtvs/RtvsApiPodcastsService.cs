using System.Xml;
using Microsoft.Extensions.Logging;

using Radio.Player.Models;
using Radio.Player.Providers.Rtvs.Utilities;
using Radio.Player.Services.Contracts;

namespace Radio.Player.Providers.Rtvs;

public class RtvsApiPodcastsService : IPodcastsService
{
    private readonly ILogger<RtvsApiPodcastsService>? _logger;

    private readonly IHttpClientFactory _httpClientFactory;

    public RtvsApiPodcastsService(IHttpClientFactory httpClientFactory, 
                                  ILogger<RtvsApiPodcastsService>? logger = default)
    {
        _httpClientFactory = httpClientFactory
            ?? throw new ArgumentNullException(nameof(httpClientFactory));

        _logger = logger;
    }

    public async Task<IEnumerable<PodcastShow>> GetLatestPodcastsAsync(RadioStation radioStation,
                                                                       int count = 0,
                                                                       CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(radioStation);

        try
        {
            var uri = new Uri(radioStation.PodcastsUrl);

            using var client = _httpClientFactory.CreateClient();

            var archivePageContent = await client.GetStringAsync(uri, cancellationToken);

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(archivePageContent);

            return xmlDocument.HasChildNodes
                ? ParsePodcasts(xmlDocument.DocumentElement?.SelectSingleNode("channel"))
                : Enumerable.Empty<PodcastShow>();
        }
        catch (XmlException xmlEx)
        {
            _logger?.LogError(xmlEx, $"Error getting latest podcasts from RTVS: {xmlEx.Message}");

            return Enumerable.Empty<PodcastShow>();
        }
        catch (HttpRequestException hrEx)
        {
            _logger?.LogError(hrEx, $"Error getting latest podcasts from RTVS: {hrEx.Message}");

            return Enumerable.Empty<PodcastShow>();
        }
    }

    private static IEnumerable<PodcastShow> ParsePodcasts(XmlNode? xmlNode)
    {
        var itemNodes = xmlNode?.SelectNodes("item");
        if (itemNodes is null)
            return Enumerable.Empty<PodcastShow>();

        return from XmlNode podcastItem in itemNodes
               let title = podcastItem.SelectSingleNode("title")?.InnerText
               let description = podcastItem.SelectSingleNode("description")?.InnerText
               let dateTime = podcastItem.SelectSingleNode("pubDate")?.InnerText
               let url = podcastItem.SelectSingleNode("link")?.InnerText
               select CreatePodcast(title, description, dateTime, url);
    }

    private static PodcastShow CreatePodcast(string? title, string? description, string? dateTime, string url)
    {
        if (title is not null && title.Contains('(') && title.Contains(')'))
            title = title.Remove(title.IndexOf('('));

        return new PodcastShow
        {
            Title = title,
            Description = description,
            TimeAired = Extensions.ToDateTime(dateTime),
            Url = url
        };
    }
}