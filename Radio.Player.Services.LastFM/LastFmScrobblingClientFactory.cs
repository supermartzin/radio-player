using IF.Lastfm.Core.Api;
using Microsoft.Extensions.Logging;

using Radio.Player.Services.Contracts;

namespace Radio.Player.Services.LastFm
{
    public class LastFmScrobblingClientFactory : ITrackScrobblingClientFactory<LastfmClient>
    {
        private readonly ILogger<LastFmScrobblingClientFactory>? _logger;

        private readonly string _apiKey;
        private readonly string _apiSecret;

        public LastFmScrobblingClientFactory(string apiKey, string apiSecret,
                                             ILogger<LastFmScrobblingClientFactory>? logger = default)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _apiSecret = apiSecret ?? throw new ArgumentNullException(nameof(apiSecret));

            _logger = logger;
        }

        public LastfmClient CreateTrackScrobblingClient() => new(_apiKey, _apiSecret);
    }
}