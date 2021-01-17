using System.Collections.Generic;
using System.Threading.Tasks;

using Radio.Player.Models;

namespace Radio.Player.Services
{
    public interface IPlaylistService
    {
        Task<Track> GetLatestTrackAsync(RadioStation radioStation);

        Task<IEnumerable<Track>> GetLatestTracksAsync(RadioStation radioStation, int count = 0);
    }
}