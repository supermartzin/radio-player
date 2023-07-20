using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

using Radio.Player.Services.Contracts.Exceptions;

namespace Radio.Player.Services.Spotify;

public class SpotifyClient
{
    private const string GetAccessTokenUrl = "https://accounts.spotify.com/api/token";
    private const string SearchApiUrl = "https://api.spotify.com/v1/search";

    private readonly ILogger<SpotifyClient>? _logger;

    private readonly string _clientId;
    private readonly string _clientSecret;

    private SpotifyAccessToken? _currentAccessToken;
    private bool _isTokenRefreshing;
    private AutoResetEvent _waitHandle;

    public SpotifyClient(string clientId, string clientSecret,
                         ILogger<SpotifyClient>? logger = default)
    {
        _clientId = clientId
            ?? throw new ArgumentNullException(nameof(clientId));
        _clientSecret = clientSecret
            ?? throw new ArgumentNullException(nameof(clientSecret));

        _logger = logger;

        _waitHandle = new AutoResetEvent(false);
    }

    public async Task AuthorizeAsAppAsync(CancellationToken cancellationToken = default)
    {
        if (_isTokenRefreshing)
            _waitHandle.WaitOne(5000);

        await AcquireAccessTokenAsync(cancellationToken);
    }

    public async Task<string?> GetAlbumCoverUrlAsync(string artist, string track, CancellationToken cancellationToken = default)
    {
        if (_currentAccessToken is null || !_currentAccessToken.IsValid)
            await RefreshAccessTokenAsync(cancellationToken);

        if (string.IsNullOrEmpty(artist) && string.IsNullOrEmpty(track))
            return null;

        var searchQuery = "";
        if (!string.IsNullOrEmpty(artist) && !string.IsNullOrEmpty(track))
        {
            searchQuery += $"{SanitizeQueryParameter(artist)}+{SanitizeQueryParameter(track)}";
        }
        else
        {
            if (!string.IsNullOrEmpty(artist))
                searchQuery += SanitizeQueryParameter(artist);
            if (!string.IsNullOrEmpty(track))
                searchQuery += SanitizeQueryParameter(track);
        }

        using var client = new HttpClient();
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{SearchApiUrl}?q={searchQuery}&type=track");
        requestMessage.Headers.TryAddWithoutValidation("Accept", "application/json");
        requestMessage.Headers.TryAddWithoutValidation("Authorization", $"{_currentAccessToken?.Type} {_currentAccessToken?.Value}");

        var response = await client.SendAsync(requestMessage, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStreamAsync(cancellationToken);

        // parse and return album art url
        return await ParseAlbumArtUrlAsync(content, cancellationToken);
    }


    #region Private helpers

    private async Task AcquireAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        using var client = new HttpClient();
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, GetAccessTokenUrl);

        requestMessage.Headers.TryAddWithoutValidation("Authorization",
            $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes(_clientId + ":" + _clientSecret))}");
        requestMessage.Content = new StringContent("grant_type=client_credentials",
            Encoding.UTF8, "application/x-www-form-urlencoded");

        var response = await client.SendAsync(requestMessage, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new ServiceException(nameof(SpotifyClient), $"Error sending request for Spotify API access token: {response.StatusCode}");

        var content = await response.Content.ReadAsStreamAsync(cancellationToken);

        try
        {
            // parse and return token
            _currentAccessToken = await JsonSerializer.DeserializeAsync<SpotifyAccessToken>(content, cancellationToken: cancellationToken);
        }
        catch (JsonException jsonEx)
        {
            throw new ServiceException(nameof(SpotifyClient), $"Error deserializing Spotify API response: {jsonEx.Message}", jsonEx);
        }
    }

    private async Task RefreshAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_isTokenRefreshing)
        {
            _waitHandle.WaitOne(5000);
        }
        else
        {
            _isTokenRefreshing = true;

            // get new token
            await AcquireAccessTokenAsync(cancellationToken);
            
            _isTokenRefreshing = false;
            _waitHandle.Set();

            // reinitialize wait handle for future use
            _waitHandle = new AutoResetEvent(false);
        }
    }

    private static async Task<string?> ParseAlbumArtUrlAsync(Stream contentStream, CancellationToken cancellationToken = default)
    {
        var results = await JsonSerializer.DeserializeAsync<dynamic>(contentStream, cancellationToken: cancellationToken);

        var itemsArray = results?["tracks"]?["items"];
        if (itemsArray?.HasValues != true)
            return null;

        var imagesArray = itemsArray[0]?["album"]?["images"];
        if (imagesArray?.HasValues != true)
            return null;

        return imagesArray[1]?["url"] as string;
    }

    private static string SanitizeQueryParameter(string value)
    {
        if (value.Contains("("))
            value = value.Remove(value.IndexOf('('));
        if (value.Contains(","))
            value = value.Remove(value.IndexOf(','));
        if (value.Contains("ft"))
            value = value.Remove(value.IndexOf("ft", StringComparison.CurrentCulture));
        if (value.Contains("feat"))
            value = value.Remove(value.IndexOf("feat", StringComparison.CurrentCulture));

        return value.Replace(" ", "+");
    }

    #endregion
}