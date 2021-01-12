using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

using Radio.Player.Models;
using Radio.Player.Services;
using Radio.Player.Services.Utilities;

namespace Radio.PLayer.Providers.Rtvs
{
    public class RtvsScheduleService : IScheduleService
    {
        public async Task<IEnumerable<ScheduleItem>> GetSchedule(RadioStationId stationId)
        {
            try
            {
                var settings = new XmlLoadSettings { ProhibitDtd = false };
                var uri = new Uri(RadioConstants.ScheduleBaseUrl + RadioConstants.IdUrls[stationId]);
                string schedulePageContent;

                using (HttpClient client = new HttpClient())
                {
                    schedulePageContent = await client.GetStringAsync(uri);
                }
                
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(schedulePageContent, settings);

                return xmlDocument.HasChildNodes() 
                            ? ParseScheduleItems(xmlDocument)
                            : Enumerable.Empty<ScheduleItem>();
            }
            catch
            {
                return Enumerable.Empty<ScheduleItem>();
            }
        }


        private IEnumerable<ScheduleItem> ParseScheduleItems(XmlDocument xmlDocument)
        {
            var schedulItems = new List<ScheduleItem>();

            foreach (var childNode in xmlDocument.DocumentElement.GetElementsByTagName("programme"))
            {
                DateTime startTime = ParseDateTime(childNode.Attributes.GetNamedItem("start")?.InnerText);
                DateTime endTime = ParseDateTime(childNode.Attributes.GetNamedItem("stop")?.InnerText);
                string title = childNode.SelectSingleNode("title")?.InnerText;
                string description = childNode.SelectSingleNode("desc")?.InnerText;
                TimeSpan length = ParseLength(childNode.SelectSingleNode("length"));
                string category = childNode.SelectSingleNode("category")?.InnerText;

                schedulItems.Add(new ScheduleItem
                {
                    StartTime = startTime,
                    EndTime = endTime,
                    Title = title,
                    Description = description,
                    Category = category,
                    Length = length
                });
            }

            return schedulItems;
        }

        private DateTime ParseDateTime(string dateTimeString)
        {
            DateTime dateTime = TypeConverter.ToDateTime(dateTimeString, Constants.ApiXmlDateTimeFormat, DateTime.MinValue);

            return DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
        }

        private TimeSpan ParseLength(IXmlNode lengthNode)
        {
            if (lengthNode == null)
                return TimeSpan.Zero;

            int hours = TypeConverter.ToInt(lengthNode.Attributes.GetNamedItem("hours")?.InnerText);
            int minutes = TypeConverter.ToInt(lengthNode.Attributes.GetNamedItem("minutes")?.InnerText);
            int seconds = TypeConverter.ToInt(lengthNode.Attributes.GetNamedItem("seconds")?.InnerText);

            return new TimeSpan(hours, minutes, seconds);
        }
    }
}
