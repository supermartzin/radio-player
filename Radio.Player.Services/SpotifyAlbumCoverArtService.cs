using System;
using System.Threading;
using System.Threading.Tasks;

using Rtvs.iRadio.Services;
using Rtvs.iRadio.Services.Spotify;

namespace Radio.Player.Services
{
    public class SpotifyAlbumCoverArtService : IAlbumCoverArtService
    {
        private readonly SpotifyClient _client;
        
        private AccessToken _currentAccessToken;
        private bool _isTokenRefreshing;
        private AutoResetEvent _waitHandle;

        public SpotifyAlbumCoverArtService(SpotifyClient spotifyClient)
        {
            _client = spotifyClient ?? throw new ArgumentNullException(nameof(spotifyClient));

            _isTokenRefreshing = false;
            _waitHandle = new AutoResetEvent(false);

            _client.GetAccessToken()
                   .ContinueWith(task => _currentAccessToken = task.Result);
        }

        public Task<string> GetAlbumCoverUrl(string artist, string trackTitle)
        {
            if (_currentAccessToken == null || !_currentAccessToken.IsValid)
                RefreshAccessToken();

            return _client.GetAlbumCoverUrl(_currentAccessToken, artist, trackTitle);
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
                _currentAccessToken = await _client.GetAccessToken();
                _isTokenRefreshing = false;
                _waitHandle.Set();

                // reinitialize wait handle for future use
                _waitHandle = new AutoResetEvent(false);
            }
        }
    }
}