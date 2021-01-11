using System.Threading.Tasks;

using Radio.Player.Models;

namespace Radio.Player.Services
{
    public interface ISettingsService
    {
        Task<bool> IsAutoPlayEnabledAsync();

        Task SaveAutoPlayAsync(bool enabled);

        Task<bool> IsAlbumCoversOnMeteredConnectionDisabledAsync();

        Task SaveAlbumCoversOnMeteredConnectionValueAsync(bool disabled);

        Task<bool> IsAutomaticRefreshOnMeteredConnectionDisabledAsync();

        Task SaveAutomaticRefreshOnMeteredConnectionValueAsync(bool disabled);

        Task<int> GetAutomaticRefreshIntervalAsync();

        Task SaveAutomaticRefreshIntervalAsync(int interval);

        Task<StreamQualityType> GetStreamQualityTypeAsync();

        Task SaveStreamQualityTypeAsync(StreamQualityType qualityType);

        Task<string> GetSelectedRadioStationAsync();

        Task SaveSelectedRadioStation(string stationName);

        Task<bool> IsTrackScrobblingEnabledAsync();

        Task SetTrackScrobblingAsync(bool enabled);

        Task<bool> IsTrackScrobblingUserLoggedInAsync();

        Task SetIsTrackScrobblingUserLoggedInAsync(bool isLoggedIn);
    }
}