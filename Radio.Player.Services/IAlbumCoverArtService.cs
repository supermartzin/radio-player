using System.Threading.Tasks;

namespace Radio.Player.Services
{
    public interface IAlbumCoverArtService
    {
        Task<string> GetAlbumCoverUrl(string artist, string trackTitle);
    }
}