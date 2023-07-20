using Radio.Player.Models;
using Radio.Player.Services.Contracts.Models;

namespace Radio.Player.Services.Contracts;

public interface ITrackScrobblingService
{
    string? AuthenticatedUserName { get; }

    Task<AuthenticationResult> AuthenticateUserAsync(Credentials credentials, CancellationToken cancellationToken = default);

    Task SignOutCurrentUser(CancellationToken cancellationToken = default);

    Task ScrobbleTrackAsync(Track track, CancellationToken cancellationToken = default);
}