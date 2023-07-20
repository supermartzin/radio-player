using Radio.Player.Models;

namespace Radio.Player.Services.Contracts;

public interface ISettingsService
{
    Task<bool> IsAutoPlayEnabledAsync(CancellationToken cancellationToken = default);

    Task SaveAutoPlayAsync(bool enabled, CancellationToken cancellationToken = default);

    Task<bool> IsAlbumCoversOnMeteredConnectionDisabledAsync(CancellationToken cancellationToken = default);

    Task SaveAlbumCoversOnMeteredConnectionValueAsync(bool disabled, CancellationToken cancellationToken = default);

    Task<bool> IsAutomaticRefreshOnMeteredConnectionDisabledAsync(CancellationToken cancellationToken = default);

    Task SaveAutomaticRefreshOnMeteredConnectionValueAsync(bool disabled, CancellationToken cancellationToken = default);

    Task<int> GetAutomaticRefreshIntervalAsync(CancellationToken cancellationToken = default);

    Task SaveAutomaticRefreshIntervalAsync(int interval, CancellationToken cancellationToken = default);

    Task<StreamQualityType> GetStreamQualityTypeAsync(CancellationToken cancellationToken = default);

    Task SaveStreamQualityTypeAsync(StreamQualityType qualityType, CancellationToken cancellationToken = default);

    Task<string> GetSelectedRadioStationAsync(CancellationToken cancellationToken = default);

    Task SaveSelectedRadioStation(string stationName, CancellationToken cancellationToken = default);

    Task<bool> IsTrackScrobblingEnabledAsync(CancellationToken cancellationToken = default);

    Task SetTrackScrobblingAsync(bool enabled, CancellationToken cancellationToken = default);

    Task<bool> IsTrackScrobblingUserLoggedInAsync(CancellationToken cancellationToken = default);

    Task SetIsTrackScrobblingUserLoggedInAsync(bool isLoggedIn, CancellationToken cancellationToken = default);
}