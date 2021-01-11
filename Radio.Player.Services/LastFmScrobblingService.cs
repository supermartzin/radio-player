using System;
using System.Threading.Tasks;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Enums;
using IF.Lastfm.Core.Objects;

using Radio.Player.Models;
using Radio.Player.Services.Factories;
using Radio.Player.Services.Utilities;

namespace Radio.Player.Services
{
    public class LastFmScrobblingService : ITrackScrobblingService
    {
        private const int LastScrobbledTrackCapacity = 50;
        
        private readonly FixedSizeQueue<Track> _lastScrobbledTracks;
        private readonly ITrackScrobblingClientFactory<LastfmClient> _trackScrobblingClientFactory;

        private LastfmClient _lastFmClient;

        public string AuthenticatedUserName => GetAuthenticatedUserName();
        
        public LastFmScrobblingService(ITrackScrobblingClientFactory<LastfmClient> trackScrobblingClientFactory)
        {
            _trackScrobblingClientFactory = trackScrobblingClientFactory ?? throw new ArgumentNullException(nameof(trackScrobblingClientFactory));
            _lastScrobbledTracks = new FixedSizeQueue<Track>(LastScrobbledTrackCapacity);
        }
        
        public async Task<(bool Success, ServiceRequestResult Result)> AuthenticateUser(string username, string password)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));
            if (password == null)
                throw new ArgumentNullException(nameof(password));
            if (_lastFmClient != null && _lastFmClient.Auth.Authenticated)
                throw new InvalidOperationException("Another user is already authenticated.");

            // create client
            _lastFmClient = _trackScrobblingClientFactory.CreateTrackScrobblingClient();

            // authenticate
            var response = await _lastFmClient.Auth.GetSessionTokenAsync(username, password);

            // parse result
            ServiceRequestResult result = ParseRequestResult(response.Status);

            return (response.Success, result);
        }

        public void DeauthenticateCurrentUser()
        {
            // dispose current client
            _lastFmClient?.Dispose();
            _lastFmClient = null;
        }

        public async Task ScrobbleTrack(Track track)
        {
            if (track == null)
                throw new ArgumentNullException(nameof(track));
            if (_lastFmClient == null || !_lastFmClient.Auth.Authenticated)
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


        private string GetAuthenticatedUserName()
        {
            if (_lastFmClient == null || !_lastFmClient.Auth.Authenticated)
                return null;

            return _lastFmClient.Auth.UserSession.Username;
        }

        private static ServiceRequestResult ParseRequestResult(LastResponseStatus status)
        {
            switch (status)
            {
                case LastResponseStatus.Successful:
                    return ServiceRequestResult.Success;

                case LastResponseStatus.BadAuth:
                    return ServiceRequestResult.InvalidCredentials;

                case LastResponseStatus.Failure:
                case LastResponseStatus.RequestFailed:
                case LastResponseStatus.ServiceDown:
                    return ServiceRequestResult.RemoteServerUnreachable;

                default:
                    return ServiceRequestResult.InternalError;
            }
        }
    }
}