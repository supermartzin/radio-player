namespace Radio.Player.Services.Factories
{
    public interface ITrackScrobblingClientFactory<out T>
    {
        T CreateTrackScrobblingClient();
    }
}