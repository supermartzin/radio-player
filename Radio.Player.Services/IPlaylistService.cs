using System.Collections.Generic;
using System.Threading.Tasks;

using Radio.Player.Models;

namespace Radio.Player.Services
{
    public interface IPlaylistService
    {
        Task<Track> GetLatestTrack(RadioStation radioStation);

        Task<IEnumerable<Track>> GetLatestTracks(RadioStation radioStation, int count = 0);
    }
}