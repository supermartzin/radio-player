using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Logging;

using Radio.Player.Models;
using Radio.Player.Services;
using Radio.Player.Services.Utilities;

namespace Radio.PLayer.Providers.Rtvs
{
    public class RtvsApiPlaylistService : IPlaylistService
    {
        private const string DateTimeFormat = "yyyyMMddHHmmss zzz";

        private readonly ILogger<RtvsApiPlaylistService> _logger;

        public RtvsApiPlaylistService(ILogger<RtvsApiPlaylistService> logger = null)
        {
            _logger = logger;
        }

        public Task<Track> GetLatestTrack(RadioStation radioStation)
        {
            if (radioStation == null)
                throw new ArgumentNullException(nameof(radioStation));

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
                    _logger.LogError(xmlEx, $"Error getting latest track in playlist from {radioStation.Name}: {xmlEx.Message}");

                    return null;
                }
            });
        }

        public Task<IEnumerable<Track>> GetLatestTracks(RadioStation radioStation, int count = 0)
        {
            if (radioStation == null)
                throw new ArgumentNullException(nameof(radioStation));

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
                    _logger.LogError(xmlEx, $"Error getting latest tracks in playlist from {radioStation.Name}: {xmlEx.Message}");

                    return Enumerable.Empty<Track>();
                }
            });
        }


        private static Track ParseTrack(XmlDocument xmlDocument)
        {
            var trackNodes = xmlDocument.DocumentElement?.GetElementsByTagName("programme");
            if (trackNodes == null || trackNodes.Count <= 0)
                return null;

            // get only first track
            var firstTrackNode = trackNodes[0];

            var dateTime = firstTrackNode.Attributes?.GetNamedItem("start")?.InnerText;
            var artist = firstTrackNode.SelectSingleNode("artist")?.InnerText;
            var title = firstTrackNode.SelectSingleNode("track")?.InnerText;

            return CreateTrack(title, artist, dateTime);
        }

        private static IEnumerable<Track> ParseTracks(XmlDocument xmlDocument)
        {
            var tracks = new List<Track>();

            var tracksList = xmlDocument.DocumentElement?.GetElementsByTagName("programme");
            if (tracksList != null)
            {
                foreach (XmlNode childNode in tracksList)
                {
                    var dateTime = childNode.Attributes?.GetNamedItem("start")?.InnerText;
                    var artist = childNode.SelectSingleNode("artist")?.InnerText;
                    var title = childNode.SelectSingleNode("track")?.InnerText;

                    // add to tracks
                    tracks.Add(CreateTrack(title, artist, dateTime));
                }
            }

            return tracks;
        }

        private static Track CreateTrack(string title, string artist, string dateTime) => new()
        {
            Artist = artist,
            Title = title,
            TimeAired = TypeConverter.ToDateTime(dateTime, DateTimeFormat, DateTime.MinValue)
        };
    }
}