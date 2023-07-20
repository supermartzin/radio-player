using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Logging;

using Radio.Player.Models;
using Radio.Player.Services;
using Radio.Player.Services.Contracts;
using Radio.Player.Services.Contracts.Utilities;

namespace Radio.PLayer.Providers.Rtvs
{
    public class RtvsApiPodcastsService : IPodcastsService
    {
        private readonly ILogger<RtvsApiPodcastsService> _logger;

        public RtvsApiPodcastsService(ILogger<RtvsApiPodcastsService> logger = null)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<PodcastShow>> GetLatestPodcastsAsync(RadioStation radioStation, int count = 0)
        {
            if (radioStation == null)
                throw new ArgumentNullException(nameof(radioStation));

            try
            {
                var uri = new Uri(radioStation.PodcastsUrl);

                using var client = new HttpClient();
                
                var archivePageContent = await client.GetStringAsync(uri).ConfigureAwait(false);

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


        private static IEnumerable<PodcastShow> ParsePodcasts(XmlNode xmlNode)
        {
            var podcasts = new List<PodcastShow>();

            var itemNodes = xmlNode.SelectNodes("item");
            if (itemNodes != null)
            {
                foreach (XmlNode podcastItem in itemNodes)
                {
                    var title = podcastItem.SelectSingleNode("title")?.InnerText;
                    var description = podcastItem.SelectSingleNode("description")?.InnerText;
                    var dateTime = podcastItem.SelectSingleNode("pubDate")?.InnerText;
                    var url = podcastItem.SelectSingleNode("link")?.InnerText;

                    // add to podcasts
                    podcasts.Add(CreatePodcast(title, description, dateTime, url));
                }
            }

            return podcasts;
        }

        private static PodcastShow CreatePodcast(string title, string description, string dateTime, string url)
        {
            DateTime convertedDateTime = TypeConverter.ToDateTime(dateTime);
            if (title.Contains("(") && title.Contains(")"))
                title = title.Remove(title.IndexOf('('));

            return new PodcastShow
            {
                Title = title,
                Description = description,
                TimeAired = convertedDateTime,
                Url = url
            };
        }
    }
}