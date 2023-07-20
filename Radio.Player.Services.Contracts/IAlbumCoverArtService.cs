namespace Radio.Player.Services.Contracts;

public interface IAlbumCoverArtService
{
    Task<string> GetAlbumCoverUrlAsync(string artist, string trackTitle, CancellationToken cancellationToken = default);
}