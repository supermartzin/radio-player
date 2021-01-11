using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rtvs.iRadio.Services.Spotify
{
    public class SpotifyClient
    {
        private const string GetAccessTokenUrl = "https://accounts.spotify.com/api/token";
        private const string SearchApiUrl = "https://api.spotify.com/v1/search";

        private readonly string _clientId;
        private readonly string _clientSecret;
        
        public SpotifyClient(string clientId, string clientSecret)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        public async Task<AccessToken> GetAccessToken()
        {
            using (HttpClient client = new HttpClient())
            using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, GetAccessTokenUrl))
            {
                requestMessage.Headers
                              .TryAddWithoutValidation("Authorization",
                                                       $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes(_clientId + ":" + _clientSecret))}");
                requestMessage.Content = new StringContent("grant_type=client_credentials",
                                                           Encoding.UTF8,
                                                           "application/x-www-form-urlencoded");

                var response = await client.SendAsync(requestMessage);
                if (!response.IsSuccessStatusCode)
                    return await Task.FromResult<AccessToken>(null);

                var content = await response.Content.ReadAsStringAsync();

                // parse and return token
                return JsonConvert.DeserializeObject<AccessToken>(content);
            }
        }

        public async Task<string> GetAlbumCoverUrl(AccessToken accessToken, string artist, string track)
        {
            if (accessToken == null)
                return null;

            if (string.IsNullOrEmpty(artist) && string.IsNullOrEmpty(track))
                return null;

            string searchQuery = "";
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
            string url = $"{SearchApiUrl}?q={searchQuery}&type=track";

            using (HttpClient client = new HttpClient())
            using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, url))
            {
                requestMessage.Headers
                              .TryAddWithoutValidation("Accept", "application/json");
                requestMessage.Headers
                              .TryAddWithoutValidation("Authorization",
                                                       $"{accessToken.Type} {accessToken.Value}");

                var response = await client.SendAsync(requestMessage);
                if (!response.IsSuccessStatusCode)
                    return await Task.FromResult<string>(null);

                var content = await response.Content.ReadAsStringAsync();

                // parse and return album art url
                return ParseAlbumArtUrl(content);
            }
        }
        

        private string ParseAlbumArtUrl(string contentObject)
        {
            dynamic results = JsonConvert.DeserializeObject<dynamic>(contentObject);

            JArray itemsArray = results["tracks"]?["items"];
            if (itemsArray == null || !itemsArray.HasValues)
                return null;

            JArray imagesArray = (JArray) itemsArray[0]?["album"]?["images"];
            if (imagesArray == null || !imagesArray.HasValues)
                return null;

            return (string) imagesArray[1]?["url"];
        }

        private string SanitizeQueryParameter(string value)
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