using HtmlAgilityPack;

namespace Radio.PLayer.Providers.Rtvs.Utilities
{
    internal static class Extensions
    {
        internal static string AttributeValueOrEmpty(this HtmlNode node, string attributeName) => node.Attributes.Contains(attributeName)
                ? node.Attributes[attributeName].Value
                : "";

        internal static bool ContainsAttributeValue(this HtmlNode node, string attributeName, string value) => node.AttributeValueOrEmpty(attributeName).Contains(value);
    }
}