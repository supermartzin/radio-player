namespace Radio.Player.Services
{
    public interface ISensitiveDataStorage
    {
        void StoreSensitiveData(SensitiveDataType dataType, string username, string password);

        (string Username, string Password) GetSensitiveData(SensitiveDataType dataType);

        void RemoveSensitiveData(SensitiveDataType dataType);
    }
}