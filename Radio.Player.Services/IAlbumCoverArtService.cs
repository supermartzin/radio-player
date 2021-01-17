using System.Threading.Tasks;

namespace Radio.Player.Services
{
    public interface IAlbumCoverArtService
    {
        Task<string> GetAlbumCoverUrlAsync(string artist, string trackTitle);
    }
}