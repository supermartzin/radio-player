using System.Collections.Generic;
using System.Threading.Tasks;

using Radio.Player.Models;

namespace Radio.Player.Services
{
    public interface IPodcastsService
    {
        Task<IEnumerable<PodcastShow>> GetLatestPodcasts(int count = 0);
    }
}