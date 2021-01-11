using System.Threading.Tasks;

using Radio.Player.Models;

namespace Radio.Player.Services
{
    public interface ITrackScrobblingService
    {
        string AuthenticatedUserName { get; }

        Task<(bool Success, ServiceRequestResult Result)> AuthenticateUser(string username, string password);

        void DeauthenticateCurrentUser();

        Task ScrobbleTrack(Track track);
    }
}