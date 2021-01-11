using System;
using System.Linq;
using Windows.Security.Credentials;
using Radio.Player.Services;

namespace Rtvs.iRadio.Services
{
    public class WindowsBuiltInSensitiveDataStorage : ISensitiveDataStorage
    {
        private readonly PasswordVault _vault;

        public WindowsBuiltInSensitiveDataStorage()
        {
            _vault = new PasswordVault();
        }

        public void StoreSensitiveData(SensitiveDataType dataType, string username, string password)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            string dataTypeKey = ToString(dataType);
            if (string.IsNullOrEmpty(dataTypeKey))
                throw new InvalidOperationException("Unknown sensitive data type.");
            
            // add them to the vault
            _vault.Add(new PasswordCredential(dataTypeKey, username, password));
        }

        public (string Username, string Password) GetSensitiveData(SensitiveDataType dataType)
        {
            string dataTypeKey = ToString(dataType);
            if (string.IsNullOrEmpty(dataTypeKey))
                throw new InvalidOperationException("Unknown sensitive data type.");

            try
            {
                // get data from the vault
                var credentialList = _vault.FindAllByResource(dataTypeKey);

                var credentials = credentialList.First();

                // retrieve password
                credentials.RetrievePassword();

                return (credentials.UserName, credentials.Password);
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Storage does not contain any data for this type.");
            }
        }

        public void RemoveSensitiveData(SensitiveDataType dataType)
        {
            string dataTypeKey = ToString(dataType);
            if (string.IsNullOrEmpty(dataTypeKey))
                throw new InvalidOperationException("Unknown sensitive data type.");

            try
            {
                // get data from the vault
                var credentialList = _vault.FindAllByResource(dataTypeKey);
                if (credentialList.Count == 0)
                    return;

                var credentials = credentialList.First();

                // remove from the vault
                _vault.Remove(credentials);
            }
            catch (Exception)
            {
                // does not contain specific data type
            }
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