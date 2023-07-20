using Radio.Player.Services.Contracts;

namespace Radio.Player.Services.Spotify;

public class SpotifyAlbumCoverArtService : IAlbumCoverArtService
{
    private readonly SpotifyClient _client;
        
    public SpotifyAlbumCoverArtService(SpotifyClient spotifyClient)
    {
        _client = spotifyClient ?? throw new ArgumentNullException(nameof(spotifyClient));
    }
    
    public async Task<string?> GetAlbumCoverUrlAsync(string artist, string trackTitle, CancellationToken cancellationToken = default)
        => await _client.GetAlbumCoverUrlAsync(artist, trackTitle, cancellationToken);
}