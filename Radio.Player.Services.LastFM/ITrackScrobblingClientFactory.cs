namespace Radio.Player.Services.Contracts.Factories
{
    public interface ITrackScrobblingClientFactory<out T>
    {
        T CreateTrackScrobblingClient();
    }
}