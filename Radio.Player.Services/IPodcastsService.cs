using System.Collections.Generic;
using System.Threading.Tasks;

using Radio.Player.Models;

namespace Radio.Player.Services
{
    public interface IPodcastsService
    {
        Task<IEnumerable<PodcastShow>> GetLatestPodcastsAsync(RadioStation radioStation, int count = 0);
    }
}