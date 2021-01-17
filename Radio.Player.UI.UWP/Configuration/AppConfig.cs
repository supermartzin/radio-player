using System;
using Windows.ApplicationModel;
using Microsoft.Extensions.Configuration;

namespace Radio.Player.UI.UWP.Configuration
{
    public class AppConfig : IAppConfig
    {
        private readonly IConfigurationRoot _configurationRoot;

        public AppConfig(string configFile)
        {
            if (configFile == null)
                throw new ArgumentNullException(nameof(configFile));

            _configurationRoot = new ConfigurationBuilder()
                                        .SetBasePath(Package.Current.InstalledLocation.Path)
                                        .AddJsonFile(configFile, optional: false)
                                        .Build();
        }

        public T GetValue<T>(string key) => _configurationRoot.GetValue<T>(key);

        public T GetSection<T>(string sectionKey) => _configurationRoot.GetSection(sectionKey).Get<T>();
    }
}