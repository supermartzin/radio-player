using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Radio.Player.Services.Spotify
{
    public class SpotifyClient
    {
        private const string GetAccessTokenUrl = "https://accounts.spotify.com/api/token";
        private const string SearchApiUrl = "https://api.spotify.com/v1/search";

        private AccessToken _currentAccessToken;
        private bool _isTokenRefreshing;
        private AutoResetEvent _waitHandle;
        private string _clientId;
        private string _clientSecret;
        
        public async Task AuthorizeAsApp(string clientId, string clientSecret)
        {
            _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            _clientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));

            if (_isTokenRefreshing)
                _waitHandle.WaitOne(5000);

            _currentAccessToken = await GetAccessToken().ConfigureAwait(false);
        }

        private async Task<AccessToken> GetAccessToken()
        {
            using var client = new HttpClient();
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, GetAccessTokenUrl);

            requestMessage.Headers
                          .TryAddWithoutValidation("Authorization",
                                             $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes(_clientId + ":" + _clientSecret))}");
            requestMessage.Content = new StringContent("grant_type=client_credentials",
                                                       Encoding.UTF8,
                                                       "application/x-www-form-urlencoded");

            var response = await client.SendAsync(requestMessage).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            // parse and return token
            return JsonConvert.DeserializeObject<AccessToken>(content);
        }

        public async Task<string> GetAlbumCoverUrl(string artist, string track)
        {
            if (_currentAccessToken == null || !_currentAccessToken.IsValid)
                RefreshAccessToken();

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

            // build url
            var url = $"{SearchApiUrl}?q={searchQuery}&type=track";

            using var client = new HttpClient();
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers
                          .TryAddWithoutValidation("Accept", "application/json");
            requestMessage.Headers
                          .TryAddWithoutValidation("Authorization", $"{_currentAccessToken.Type} {_currentAccessToken.Value}");

            var response = await client.SendAsync(requestMessage).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            // parse and return album art url
            return ParseAlbumArtUrl(content);
        }

        private async void RefreshAccessToken()
        {
            if (_isTokenRefreshing)
            {
                _waitHandle.WaitOne(5000);
            }
            else
            {
                _isTokenRefreshing = true;

                // get new token
                _currentAccessToken = await GetAccessToken();
                _isTokenRefreshing = false;
                _waitHandle.Set();

                // reinitialize wait handle for future use
                _waitHandle = new AutoResetEvent(false);
            }
        }

        private static string ParseAlbumArtUrl(string contentObject)
        {
            var results = JsonConvert.DeserializeObject<dynamic>(contentObject);

            JArray itemsArray = results["tracks"]?["items"];
            if (itemsArray?.HasValues != true)
                return null;

            var imagesArray = (JArray) itemsArray[0]?["album"]?["images"];
            if (imagesArray?.HasValues != true)
                return null;

            return (string) imagesArray[1]?["url"];
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
    }
}