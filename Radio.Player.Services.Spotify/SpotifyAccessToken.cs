using System.Text.Json.Serialization;

namespace Radio.Player.Services.Spotify;

internal record SpotifyAccessToken
{
    private readonly DateTime _created = DateTime.Now;

    [JsonPropertyName("access_token")]
    public string Value { get; set; }

    [JsonPropertyName("token_type")]
    public string Type { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpirationSeconds { get; set; }

    public bool IsValid => _created.AddSeconds(ExpirationSeconds) > DateTime.Now;
}