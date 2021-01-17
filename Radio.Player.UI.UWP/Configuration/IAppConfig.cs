namespace Radio.Player.UI.UWP.Configuration
{
    public interface IAppConfig
    {
        T GetValue<T>(string key);

        T GetSection<T>(string sectionKey);
    }
}