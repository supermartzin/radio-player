using System;
using System.Threading.Tasks;

using Radio.Player.Services.Spotify;

namespace Radio.Player.Services
{
    public class SpotifyAlbumCoverArtService : IAlbumCoverArtService
    {
        private readonly SpotifyClient _client;
        
        public SpotifyAlbumCoverArtService(SpotifyClient spotifyClient)
        {
            _client = spotifyClient ?? throw new ArgumentNullException(nameof(spotifyClient));
        }

        public async Task<string> GetAlbumCoverUrlAsync(string artist, string trackTitle) => await _client.GetAlbumCoverUrl(artist, trackTitle).ConfigureAwait(false);
    }
}