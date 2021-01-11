using System.Threading.Tasks;
using Windows.Storage;
using Radio.Player.Services;
using Rtvs.iRadio.Models;

namespace Rtvs.iRadio.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ApplicationDataContainer _localAppSettings;

        public SettingsService()
        {
            _localAppSettings = ApplicationData.Current.LocalSettings;
        }

        public Task<bool> IsAutoPlayEnabledAsync()
        {
            return Task<bool>.Factory.StartNew(() =>
            {
                bool? autoPlayEnabled = (bool?) _localAppSettings.Values["autoPlayEnabled"];

                return autoPlayEnabled.HasValue && autoPlayEnabled.Value;
            });
        }

        public Task SaveAutoPlayAsync(bool enabled)
        {
            return Task.Factory.StartNew(() => _localAppSettings.Values["autoPlayEnabled"] = enabled);
        }

        public Task<bool> IsAlbumCoversOnMeteredConnectionDisabledAsync()
        {
            return Task<bool>.Factory.StartNew(() =>
            {
                bool? albumCoversDisabled = (bool?)_localAppSettings.Values["albumCoversDisabled"];

                return albumCoversDisabled.HasValue && albumCoversDisabled.Value;
            });
        }

        public Task SaveAlbumCoversOnMeteredConnectionValueAsync(bool disabled)
        {
            return Task.Factory.StartNew(() => _localAppSettings.Values["albumCoversDisabled"] = disabled);
        }

        public Task<bool> IsAutomaticRefreshOnMeteredConnectionDisabledAsync()
        {
            return Task<bool>.Factory.StartNew(() =>
            {
                bool? automaticRefresh = (bool?)_localAppSettings.Values["automaticRefresh"];

                return automaticRefresh.HasValue && automaticRefresh.Value;
            });
        }

        public Task SaveAutomaticRefreshOnMeteredConnectionValueAsync(bool disabled)
        {
            return Task.Factory.StartNew(() => _localAppSettings.Values["automaticRefresh"] = disabled);
        }

        public Task<int> GetAutomaticRefreshIntervalAsync()
        {
            return Task<int>.Factory.StartNew(() =>
            {
                int? interval = (int?) _localAppSettings.Values["refreshIntervalValue"];

                return interval ?? 0;
            });
        }

        public Task SaveAutomaticRefreshIntervalAsync(int interval)
        {
            return Task.Factory.StartNew(() => _localAppSettings.Values["refreshIntervalValue"] = interval);
        }

        public Task<StreamQualityType> GetStreamQualityTypeAsync()
        {
            return Task<StreamQualityType>.Factory.StartNew(() =>
            {
                int? value = (int?) _localAppSettings.Values["streamQualityType"];
                if (value.HasValue)
                    return (StreamQualityType) value;

                // return default value
                return StreamQualityType.Medium;
            });
        }

        public Task SaveStreamQualityTypeAsync(StreamQualityType qualityType)
        {
            return Task.Factory.StartNew(() => _localAppSettings.Values["streamQualityType"] = (int) qualityType);
        }

        public Task<RadioStationId> GetSelectedRadioStationAsync()
        {
            return Task<RadioStationId>.Factory.StartNew(() =>
            {
                int? value = (int?) _localAppSettings.Values["selectedRadioStationId"];
                if (value.HasValue)
                    return (RadioStationId) value;

                // return default
                return RadioStationId.Slovensko;
            });
        }

        public Task SaveSelectedRadioStation(RadioStationId stationId)
        {
            return Task.Factory.StartNew(() =>
            {
                _localAppSettings.Values["selectedRadioStationId"] = (int) stationId;
            });
        }

        public Task<bool> IsTrackScrobblingEnabledAsync()
        {
            return Task<bool>.Factory.StartNew(() =>
            {
                bool? scrobblingEnabled = (bool?) _localAppSettings.Values["scrobblingEnabled"];

                return scrobblingEnabled.HasValue && scrobblingEnabled.Value;
            });
        }

        public Task SetTrackScrobblingAsync(bool enabled)
        {
            return Task.Factory.StartNew(() => _localAppSettings.Values["scrobblingEnabled"] = enabled);
        }

        public Task<bool> IsTrackScrobblingUserLoggedInAsync()
        {
            return Task<bool>.Factory.StartNew(() =>
            {
                bool? scrobblingUserLoggedIn = (bool?) _localAppSettings.Values["scrobblingUserLoggedIn"];

                return scrobblingUserLoggedIn.HasValue && scrobblingUserLoggedIn.Value;
            });
        }

        public Task SetIsTrackScrobblingUserLoggedInAsync(bool isLoggedIn)
        {
            return Task.Factory.StartNew(() => _localAppSettings.Values["scrobblingUserLoggedIn"] = isLoggedIn);
        }
    }
}