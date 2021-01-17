using System;
using Windows.Security.Credentials;
using Microsoft.Extensions.Logging;

using Radio.Player.Services;

namespace Radio.Player.UI.UWP.Services
{
    public class WindowsBuiltInSensitiveDataStorage : ISensitiveDataStorage
    {
        private readonly ILogger<WindowsBuiltInSensitiveDataStorage> _logger;
        private readonly PasswordVault _vault;

        public WindowsBuiltInSensitiveDataStorage(ILogger<WindowsBuiltInSensitiveDataStorage> logger = null)
        {
            _logger = logger;

            _vault = new PasswordVault();
        }

        public void StoreSensitiveData(SensitiveDataType dataType, string username, string password)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            var dataTypeKey = ToString(dataType);
            if (string.IsNullOrEmpty(dataTypeKey))
                throw new InvalidOperationException("Unknown sensitive data type.");
            
            // add them to the vault
            _vault.Add(new PasswordCredential(dataTypeKey, username, password));
        }

        public (string Username, string Password) GetSensitiveData(SensitiveDataType dataType)
        {
            var dataTypeKey = ToString(dataType);
            if (string.IsNullOrEmpty(dataTypeKey))
                throw new InvalidOperationException("Unknown sensitive data type.");

            // get data from the vault
            var credentialList = _vault.FindAllByResource(dataTypeKey);

            if (credentialList.Count == 0)
                throw new InvalidOperationException("Storage does not contain any data for this type.");

            var credentials = credentialList[0];

            // retrieve password
            credentials.RetrievePassword();

            return (credentials.UserName, credentials.Password);
        }

        public void RemoveSensitiveData(SensitiveDataType dataType)
        {
            var dataTypeKey = ToString(dataType);
            if (string.IsNullOrEmpty(dataTypeKey))
                throw new InvalidOperationException("Unknown sensitive data type.");

            // get data from the vault
            var credentialList = _vault.FindAllByResource(dataTypeKey);

            if (credentialList.Count == 0)
                return;

            var credentials = credentialList[0];

            // remove from the vault
            _vault.Remove(credentials);
        }


        private string ToString(SensitiveDataType dataType)
        {
            switch (dataType)
            {
                case SensitiveDataType.LastFmServiceCredentials:
                    return "last-fm-service";

                default:
                    return null;
            }
        }
    }
}