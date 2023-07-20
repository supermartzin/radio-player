using Radio.Player.Models;

namespace Radio.Player.Services.Contracts;

public interface IPlaylistService
{
    Task<Track?> GetLatestTrackAsync(RadioStation radioStation, CancellationToken cancellationToken = default);

    Task<IEnumerable<Track>> GetLatestTracksAsync(RadioStation radioStation, int count = 0, CancellationToken cancellationToken = default);
}