using System.Globalization;
using HtmlAgilityPack;

namespace Radio.Player.Providers.Rtvs.Utilities;

internal static class Extensions
{
    internal static string AttributeValueOrEmpty(this HtmlNode node, string attributeName) => node.Attributes.Contains(attributeName)
        ? node.Attributes[attributeName].Value
        : "";

    internal static bool ContainsAttributeValue(this HtmlNode node, string attributeName, string value) => node.AttributeValueOrEmpty(attributeName).Contains(value);

    internal static DateTime ToDateTime(string? dateTimeString, string? format = null, DateTime? defaultValue = null)
    {
        if (string.IsNullOrEmpty(dateTimeString))
            return defaultValue ?? DateTime.Now;

        if (!string.IsNullOrEmpty(format))
        {
            if (DateTime.TryParseExact(dateTimeString,
                    format,
                    CultureInfo.CurrentCulture,
                    DateTimeStyles.None,
                    out var parsed))
                return parsed;
        }
        else
        {
            if (DateTime.TryParse(dateTimeString, out var parsed))
                return parsed;
        }

        return defaultValue ?? DateTime.Now;
    }

    internal static int ToInt(string? numberString, int defaultValue = 0)
    {
        if (numberString is null)
            return defaultValue;

        if (int.TryParse(numberString, out var parsed))
            return parsed;

        try
        {
            return Convert.ToInt32(numberString);
        }
        catch
        {
            return defaultValue;
        }
    }
}