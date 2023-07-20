using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Enums;
using IF.Lastfm.Core.Objects;

using Radio.Player.Models;
using Radio.Player.Services.Contracts;
using Radio.Player.Services.Contracts.Factories;
using Radio.Player.Services.Contracts.Models;

namespace Radio.Player.Services.LastFM;

public class LastFmScrobblingService : ITrackScrobblingService
{
    private const int LastScrobbledTrackCapacity = 50;
    
    private readonly ITrackScrobblingClientFactory<LastfmClient> _trackScrobblingClientFactory;
    private readonly FixedSizeQueue<Track> _lastScrobbledTracks;

    private LastfmClient? _lastFmClient;

    public string? AuthenticatedUserName => GetAuthenticatedUserName();

    public LastFmScrobblingService(ITrackScrobblingClientFactory<LastfmClient> trackScrobblingClientFactory)
    {
        _trackScrobblingClientFactory = trackScrobblingClientFactory
            ?? throw new ArgumentNullException(nameof(trackScrobblingClientFactory));
        _lastScrobbledTracks = new FixedSizeQueue<Track>(LastScrobbledTrackCapacity);
    }

    public async Task<AuthenticationResult> AuthenticateUserAsync(Credentials credentials, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(credentials);

        if (_lastFmClient is null || _lastFmClient.Auth.Authenticated)
            throw new InvalidOperationException("Another user is already authenticated.");

        // create client
        _lastFmClient = _trackScrobblingClientFactory.CreateTrackScrobblingClient();

        // authenticate
        var response = await _lastFmClient.Auth.GetSessionTokenAsync(credentials.Username, credentials.Password);
        
        return ParseRequestResult(response.Status);
    }

    public Task SignOutCurrentUser(CancellationToken cancellationToken = default)
    {
        // dispose current client
        _lastFmClient?.Dispose();
        _lastFmClient = null;

        return Task.CompletedTask;
    }

    public async Task ScrobbleTrackAsync(Track track, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(track);
        if (_lastFmClient is null || !_lastFmClient.Auth.Authenticated)
            throw new InvalidOperationException("User is not authenticated.");

        // skip if already scrobbled recently
        if (_lastScrobbledTracks.Contains(track))
            return;

        // scrobble
        var response = await _lastFmClient.Scrobbler.ScrobbleAsync(
            new Scrobble(track.Artist, string.Empty, track.Title, track.TimeAired));

        if (response.Success)
        {
            // add to last scrobbled tracks
            _lastScrobbledTracks.Enqueue(track);
        }
    }


    private string? GetAuthenticatedUserName()
    {
        if (_lastFmClient is null || !_lastFmClient.Auth.Authenticated)
            return null;

        return _lastFmClient.Auth.UserSession.Username;
    }

    private static AuthenticationResult ParseRequestResult(LastResponseStatus status)
    {
        switch (status)
        {
            case LastResponseStatus.Successful:
                return new AuthenticationResult(true, null);

            case LastResponseStatus.BadAuth:
                return new AuthenticationResult(false, "Invalid credentials");

            case LastResponseStatus.Failure:
            case LastResponseStatus.RequestFailed:
            case LastResponseStatus.ServiceDown:
                return new AuthenticationResult(false, "Last.fm server is not responding");

            default:
                return new AuthenticationResult(false, "Internal error");
        }
    }
}