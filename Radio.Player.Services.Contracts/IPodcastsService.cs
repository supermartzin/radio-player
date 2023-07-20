using Radio.Player.Models;

namespace Radio.Player.Services.Contracts;

public interface IPodcastsService
{
    Task<IEnumerable<PodcastShow>> GetLatestPodcastsAsync(RadioStation radioStation, int count = 0, CancellationToken cancellationToken = default);
}