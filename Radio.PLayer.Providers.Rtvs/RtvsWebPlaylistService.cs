using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

using Radio.Player.Models;
using Radio.Player.Services;
using Radio.PLayer.Providers.Rtvs.Utilities;
using Radio.Player.Services.Contracts;
using Radio.Player.Services.Contracts.Utilities;

namespace Radio.PLayer.Providers.Rtvs
{
    public class RtvsWebPlaylistService : IPlaylistService
    {
        private const string DateTimeFormat = "d.MM.yyyy HH:mm";

        public async Task<Track> GetLatestTrackAsync(RadioStation radioStation)
        {
            if (radioStation == null)
                throw new ArgumentNullException(nameof(radioStation));

            var htmlTracks = await GetHtmlPlaylistTracks(radioStation.PlaylistUrl).ConfigureAwait(false);

            if (htmlTracks == null || htmlTracks.Count == 0)
                return null;

            // get only first track node
            var tds = htmlTracks.FirstOrDefault(node => node.Name == "tr")?
                                             .ChildNodes
                                             .Where(node => node.Name == "td")
                                             .ToList();

            return tds?.Count != 4
                            ? null
                            : CreateTrack(tds);
        }

        public async Task<IEnumerable<Track>> GetLatestTracksAsync(RadioStation radioStation, int count = 0)
        {
            if (radioStation == null)
                throw new ArgumentNullException(nameof(radioStation));

            var htmlTracks = await GetHtmlPlaylistTracks(radioStation.PlaylistUrl);

            if (htmlTracks == null)
                return Enumerable.Empty<Track>();

            // process tracks
            var tracks = new List<Track>();
            foreach (var trackNode in htmlTracks)
            {
                var tds = trackNode.ChildNodes
                                                .Where(node => node.Name == "td")
                                                .ToList();

                if (tds.Count != 4)
                    continue;

                tracks.Add(CreateTrack(tds));
            }

            return tracks;
        }


        private static async Task<HtmlNodeCollection> GetHtmlPlaylistTracks(string url)
        {
            using var client = new HttpClient();

            var downloadedPage = await client.GetStringAsync(url).ConfigureAwait(false);

            var html = new HtmlDocument();
            html.LoadHtml(downloadedPage);

            return html.DocumentNode
                       .Descendants("table")
                       .FirstOrDefault(descendant => descendant.ContainsAttributeValue("class", "playlist"))?
                       .ChildNodes
                       .FindFirst("tbody")
                       .ChildNodes;
        }

        private static Track CreateTrack(IList<HtmlNode> trackDetails)
        {
            // parse track
            var date = trackDetails[0].InnerText.Trim();
            var time = trackDetails[1].InnerText.Trim();
            var artist = trackDetails[2].InnerText;
            var title = trackDetails[3].InnerText;
            
            return new Track
            {
                Title = title.Trim(),
                Artist = artist.Trim(),
                TimeAired = TypeConverter.ToDateTime($"{date} {time}", DateTimeFormat, DateTime.MinValue)
            };
        }
    }
}