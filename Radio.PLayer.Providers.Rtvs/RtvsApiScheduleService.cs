using System.Xml;
using Microsoft.Extensions.Logging;

using Radio.Player.Models;
using Radio.Player.Providers.Rtvs.Utilities;
using Radio.Player.Services.Contracts;

namespace Radio.Player.Providers.Rtvs;

public class RtvsApiScheduleService : IScheduleService
{
    private const string DateTimeFormat = "yyyyMMddHHmmss zzz";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RtvsApiScheduleService>? _logger;

    public RtvsApiScheduleService(IHttpClientFactory httpClientFactory,
                                  ILogger<RtvsApiScheduleService>? logger = default)
    {
        _httpClientFactory = httpClientFactory
            ?? throw new ArgumentNullException(nameof(httpClientFactory));

        _logger = logger;
    }

    public async Task<IEnumerable<ScheduleItem>> GetFullScheduleAsync(RadioStation radioStation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(radioStation);

        try
        {
            using var client = _httpClientFactory.CreateClient();

            var schedulePageContent = await client.GetStringAsync(new Uri(radioStation.ScheduleUrl), cancellationToken);

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

    public async Task<IEnumerable<ScheduleItem>> GetScheduleForSpecificDayAsync(RadioStation radioStation, DateTime date,
                                                                                CancellationToken cancellationToken = default)
    {
        var scheduleItems = await GetFullScheduleAsync(radioStation, cancellationToken);

        return scheduleItems.Where(item => item.StartTime.Date == date.Date || item.EndTime.Date == date.Date);
    }
    

    private static IEnumerable<ScheduleItem> ParseScheduleItems(XmlDocument xmlDocument)
    {
        var items = xmlDocument.DocumentElement?.GetElementsByTagName("programme");

        if (items is null)
            return Enumerable.Empty<ScheduleItem>();

        return from XmlNode childNode in items
               let startTime = ParseDateTime(childNode.Attributes?.GetNamedItem("start")?.InnerText)
               let endTime = ParseDateTime(childNode.Attributes?.GetNamedItem("stop")?.InnerText)
               let title = childNode.SelectSingleNode("title")?.InnerText
               let description = childNode.SelectSingleNode("desc")?.InnerText
               let length = ParseLength(childNode.SelectSingleNode("length"))
               let category = childNode.SelectSingleNode("category")?.InnerText
               select new ScheduleItem
               {
                   StartTime = startTime,
                   EndTime = endTime,
                   Title = title,
                   Description = description,
                   Category = category,
                   Length = length
               };
    }

    private static DateTime ParseDateTime(string dateTimeString)
    {
        var dateTime = Extensions.ToDateTime(dateTimeString, DateTimeFormat, DateTime.MinValue);

        return DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
    }

    private static TimeSpan ParseLength(XmlNode? lengthNode)
    {
        if (lengthNode is null)
            return TimeSpan.Zero;

        var hours = Extensions.ToInt(lengthNode.Attributes?.GetNamedItem("hours")?.InnerText);
        var minutes = Extensions.ToInt(lengthNode.Attributes?.GetNamedItem("minutes")?.InnerText);
        var seconds = Extensions.ToInt(lengthNode.Attributes?.GetNamedItem("seconds")?.InnerText);

        return new TimeSpan(hours, minutes, seconds);
    }
}