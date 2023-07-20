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
    public class RtvsApiScheduleService : IScheduleService
    {
        private const string DateTimeFormat = "yyyyMMddHHmmss zzz";

        private readonly ILogger<RtvsApiScheduleService> _logger;

        public RtvsApiScheduleService(ILogger<RtvsApiScheduleService> logger = null)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<ScheduleItem>> GetFullScheduleAsync(RadioStation radioStation)
        {
            if (radioStation == null)
                throw new ArgumentNullException(nameof(radioStation));

            try
            {
                var uri = new Uri(radioStation.ScheduleUrl);

                using var client = new HttpClient();

                var schedulePageContent = await client.GetStringAsync(uri).ConfigureAwait(false);

                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(schedulePageContent);

                return xmlDocument.HasChildNodes
                            ? ParseScheduleItems(xmlDocument)
                            : Enumerable.Empty<ScheduleItem>();
            }
            catch (XmlException xmlEx)
            {
                _logger?.LogError(xmlEx, $"Error getting full schedule for {radioStation.Name} from RTVS: {xmlEx.Message}");

                return Enumerable.Empty<ScheduleItem>();
            }
            catch (HttpRequestException hrEx)
            {
                _logger?.LogError(hrEx, $"Error getting full schedule for {radioStation.Name} from RTVS: {hrEx.Message}");

                return Enumerable.Empty<ScheduleItem>();
            }
        }

        public async Task<IEnumerable<ScheduleItem>> GetScheduleForSpecificDayAsync(RadioStation radioStation, DateTime date)
        {
            var scheduleItems = await GetFullScheduleAsync(radioStation).ConfigureAwait(false);

            return scheduleItems.Where(item => item.StartTime.Date == date.Date || item.EndTime.Date == date.Date);
        }


        private static IEnumerable<ScheduleItem> ParseScheduleItems(XmlDocument xmlDocument)
        {
            var scheduleItems = new List<ScheduleItem>();

            var items = xmlDocument.DocumentElement?.GetElementsByTagName("programme");

            if (items != null)
            {
                foreach (XmlNode childNode in items)
                {
                    var startTime = ParseDateTime(childNode.Attributes?.GetNamedItem("start")?.InnerText);
                    var endTime = ParseDateTime(childNode.Attributes?.GetNamedItem("stop")?.InnerText);
                    var title = childNode.SelectSingleNode("title")?.InnerText;
                    var description = childNode.SelectSingleNode("desc")?.InnerText;
                    var length = ParseLength(childNode.SelectSingleNode("length"));
                    var category = childNode.SelectSingleNode("category")?.InnerText;

                    scheduleItems.Add(new ScheduleItem
                    {
                        StartTime = startTime,
                        EndTime = endTime,
                        Title = title,
                        Description = description,
                        Category = category,
                        Length = length
                    });
                }
            }

            return scheduleItems;
        }

        private static DateTime ParseDateTime(string dateTimeString)
        {
            var dateTime = TypeConverter.ToDateTime(dateTimeString, DateTimeFormat, DateTime.MinValue);

            return DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
        }

        private static TimeSpan ParseLength(XmlNode lengthNode)
        {
            if (lengthNode == null)
                return TimeSpan.Zero;

            var hours = TypeConverter.ToInt(lengthNode.Attributes?.GetNamedItem("hours")?.InnerText);
            var minutes = TypeConverter.ToInt(lengthNode.Attributes?.GetNamedItem("minutes")?.InnerText);
            var seconds = TypeConverter.ToInt(lengthNode.Attributes?.GetNamedItem("seconds")?.InnerText);

            return new TimeSpan(hours, minutes, seconds);
        }
    }
}
