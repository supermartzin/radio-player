using HtmlAgilityPack;

namespace Radio.Player.Services.Utilities
{
    public static class Extensions
    {
        public static string AttributeValueOrEmpty(this HtmlNode node, string attributeName) => node.Attributes.Contains(attributeName)
                ? node.Attributes[attributeName].Value
                : "";

        public static bool ContainsAttributeValue(this HtmlNode node, string attributeName, string value) => node.AttributeValueOrEmpty(attributeName).Contains(value);
    }
}