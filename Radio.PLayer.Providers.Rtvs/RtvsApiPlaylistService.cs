using System.Xml;
using Microsoft.Extensions.Logging;

using Radio.Player.Models;
using Radio.Player.Providers.Rtvs.Utilities;
using Radio.Player.Services.Contracts;

namespace Radio.Player.Providers.Rtvs;

public class RtvsApiPlaylistService : IPlaylistService
{
    private const string DateTimeFormat = "yyyyMMddHHmmss zzz";

    private readonly ILogger<RtvsApiPlaylistService>? _logger;

    public RtvsApiPlaylistService(ILogger<RtvsApiPlaylistService>? logger = default)
    {
        _logger = logger;
    }

    public Task<Track?> GetLatestTrackAsync(RadioStation radioStation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(radioStation);

        return Task.Factory.StartNew(() =>
        {
            try
            {
                var document = new XmlDocument();
                document.Load(radioStation.PlaylistUrl);

                return document.HasChildNodes
                    ? ParseTrack(document)
                    : null;
            }
            catch (XmlException xmlEx)
            {
                _logger?.LogError(xmlEx, $"Error getting latest track in playlist from {radioStation.Name}: {xmlEx.Message}");

                return null;
            }
        }, cancellationToken);
    }

    public Task<IEnumerable<Track>> GetLatestTracksAsync(RadioStation radioStation, int count = 0, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(nameof(radioStation));

        return Task.Factory.StartNew(() =>
        {
            try
            {
                var document = new XmlDocument();
                document.Load(radioStation.PlaylistUrl);

                return document.HasChildNodes
                    ? ParseTracks(document)
                    : Enumerable.Empty<Track>();
            }
            catch (XmlException xmlEx)
            {
                _logger?.LogError(xmlEx, $"Error getting latest tracks in playlist from {radioStation.Name}: {xmlEx.Message}");

                return Enumerable.Empty<Track>();
            }
        }, cancellationToken);
    }


    private static Track? ParseTrack(XmlDocument xmlDocument)
    {
        var trackNodes = xmlDocument.DocumentElement?.GetElementsByTagName("programme");
        if (trackNodes is not {Count: > 0})
            return null;

        // get only first track
        var firstTrackNode = trackNodes[0];

        var dateTime = firstTrackNode?.Attributes?.GetNamedItem("start")?.InnerText;
        var artist = firstTrackNode?.SelectSingleNode("artist")?.InnerText;
        var title = firstTrackNode?.SelectSingleNode("track")?.InnerText;

        return CreateTrack(title, artist, dateTime);
    }

    private static IEnumerable<Track> ParseTracks(XmlDocument xmlDocument)
    {
        var tracksList = xmlDocument.DocumentElement?.GetElementsByTagName("programme");
        if (tracksList is null)
            return Enumerable.Empty<Track>();

        return from XmlNode childNode in tracksList
               let dateTime = childNode.Attributes?.GetNamedItem("start")?.InnerText
               let artist = childNode.SelectSingleNode("artist")?.InnerText
               let title = childNode.SelectSingleNode("track")?.InnerText
               select CreateTrack(title, artist, dateTime);
    }

    private static Track CreateTrack(string? title, string? artist, string? dateTime) => new()
    {
        Artist = artist,
        Title = title,
        TimeAired = Extensions.ToDateTime(dateTime, DateTimeFormat, DateTime.MinValue)
    };
}