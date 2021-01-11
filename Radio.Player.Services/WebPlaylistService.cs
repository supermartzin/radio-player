using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Radio.Player.Services;
using Radio.Player.Services.Utilities;
using Rtvs.iRadio.Models;

namespace Rtvs.iRadio.Services
{
    public class WebPlaylistService : IPlaylistService
    {
        public async Task<Track> GetLastPlayedTrack(string playlistUrl)
        {
            if (string.IsNullOrEmpty(playlistUrl))
                return null;

            var htmlTracks = await GetHtmlPlaylistTracks(playlistUrl);

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

        public async Task<IEnumerable<Track>> GetLastPlayedTracks(string playlistUrl)
        {
            if (string.IsNullOrEmpty(playlistUrl))
                return Enumerable.Empty<Track>();

            var htmlTracks = await GetHtmlPlaylistTracks(playlistUrl);

            if (htmlTracks == null)
                return Enumerable.Empty<Track>();

            // process tracks
            var tracks = new List<Track>();
            foreach (HtmlNode trackNode in htmlTracks)
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


        private async Task<HtmlNodeCollection> GetHtmlPlaylistTracks(string url)
        {
            using (var client = new HttpClient())
            {
                string downloadedPage = await client.GetStringAsync(url);

                var html = new HtmlDocument();
                html.LoadHtml(downloadedPage);

                return html.DocumentNode
                           .Descendants("table")
                           .FirstOrDefault(descendant => descendant.ContainsAttributeValue("class", "playlist"))?
                           .ChildNodes
                           .FindFirst("tbody")
                           .ChildNodes;
            }
        }

        private Track CreateTrack(IList<HtmlNode> trackDetails)
        {
            // parse track
            string date = trackDetails[0].InnerText.Trim();
            string time = trackDetails[1].InnerText.Trim();
            string artist = trackDetails[2].InnerText;
            string title = trackDetails[3].InnerText;

            // convert DateTime
            DateTime airedTime = TypeConverter.ToDateTime($"{date} {time}", Constants.WebPlaylistDateTimeFormat, DateTime.MinValue);

            return new Track
            {
                Title = title.Trim(),
                Artist = artist.Trim(),
                TimeAired = airedTime
            };
        }
    }
}