using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

using Radio.Player.Models;
using Radio.Player.Providers.Rtvs.Utilities;
using Radio.Player.Services.Contracts;

namespace Radio.Player.Providers.Rtvs;

public class RtvsWebPlaylistService : IPlaylistService
{
    private const string DateTimeFormat = "d.MM.yyyy HH:mm";

    private readonly ILogger<RtvsWebPlaylistService>? _logger;

    private readonly IHttpClientFactory _httpClientFactory;

    public RtvsWebPlaylistService(IHttpClientFactory httpClientFactory,
                                  ILogger<RtvsWebPlaylistService>? logger = default)
    {
        _httpClientFactory = httpClientFactory
            ?? throw new ArgumentNullException(nameof(httpClientFactory));

        _logger = logger;
    }

    public async Task<Track?> GetLatestTrackAsync(RadioStation radioStation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(radioStation);

        var htmlTracks = await GetHtmlPlaylistTracksAsync(radioStation.PlaylistUrl, cancellationToken);

        if (htmlTracks is null || htmlTracks.Count is 0)
            return null;

        // get only first track node
        var tds = htmlTracks.FirstOrDefault(node => node.Name == "tr")?
                            .ChildNodes
                            .Where(node => node.Name == "td")
                            .ToList();

        return tds?.Count is 4 ? CreateTrack(tds) : null;
    }

    public async Task<IEnumerable<Track>> GetLatestTracksAsync(RadioStation radioStation, int count = 0, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(radioStation);

        var htmlTracks = await GetHtmlPlaylistTracksAsync(radioStation.PlaylistUrl, cancellationToken);

        if (htmlTracks is null)
            return Enumerable.Empty<Track>();

        // process tracks

        return from trackNode in htmlTracks
               select trackNode.ChildNodes.Where(node => node.Name is "td")
               into tds
               where tds.Count() is 4
               select CreateTrack(tds);
    }
    

    private async Task<HtmlNodeCollection?> GetHtmlPlaylistTracksAsync(string url, CancellationToken cancellationToken = default)
    {
        using var client = _httpClientFactory.CreateClient();

        var downloadedPage = await client.GetStringAsync(url, cancellationToken);

        var html = new HtmlDocument();

        html.LoadHtml(downloadedPage);

        return html.DocumentNode
                   .Descendants("table")
                   .FirstOrDefault(descendant => descendant.ContainsAttributeValue("class", "playlist"))?
                   .ChildNodes
                   .FindFirst("tbody")
                   .ChildNodes;
    }

    private static Track CreateTrack(IEnumerable<HtmlNode> trackDetails)
    {
        var trackDetailsList = trackDetails.ToList();

        // parse track
        var date = trackDetailsList[0].InnerText.Trim();
        var time = trackDetailsList[1].InnerText.Trim();
        var artist = trackDetailsList[2].InnerText;
        var title = trackDetailsList[3].InnerText;
            
        return new Track
        {
            Title = title.Trim(),
            Artist = artist.Trim(),
            TimeAired = Extensions.ToDateTime($"{date} {time}", DateTimeFormat, DateTime.MinValue)
        };
    }
}