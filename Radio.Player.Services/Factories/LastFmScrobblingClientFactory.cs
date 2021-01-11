using System;

using IF.Lastfm.Core.Api;

namespace Radio.Player.Services.Factories
{
    public class LastFmScrobblingClientFactory : ITrackScrobblingClientFactory<LastfmClient>
    {
        private readonly string _apiKey;
        private readonly string _apiSecret;

        public LastFmScrobblingClientFactory(string apiKey, string apiSecret)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _apiSecret = apiSecret ?? throw new ArgumentNullException(nameof(apiSecret));
        }

        public LastfmClient CreateTrackScrobblingClient() => new LastfmClient(_apiKey, _apiSecret);
    }
}